using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;

using System.Threading;

namespace Gabriel.Cat
{
	public enum TempsEnMiliSegons:long
	{
		segon = 1000,
		minut = segon * 60,
		hora = minut * 60,
		dia = hora * 24,
		semana = dia * 7,
		mes = semana * 4,
		any = mes * 12,

	}
    public delegate string StringCreate(TipusBaseDeDades bdDesti);
	public delegate void ObjecteNouEventHandler(ObjecteSql objNew);
	public  abstract class ControlObjectesSql : IEnumerable<ObjecteSql>
	{

		public event ObjecteNouEventHandler ObjNou;
		LlistaOrdenada<ulong, ObjecteSql> controlObj;
		private BaseDeDades baseDeDades;
		System.Timers.Timer temporitzadorActualitzacions;
		Semaphore semaforActualitzacions;
		string[] creates;
		string[] tablas;
		public ControlObjectesSql(BaseDeDades baseDeDades, StringCreate[] creates,bool restaurarTotesDades=true)
		{
            string[] createsSql;
            createsSql = new string[creates.Length];
            for (int i = 0; i < creates.Length; i++)
                createsSql[i] = creates[i](baseDeDades.TipusBD);

            CreatesSql = createsSql;
			this.baseDeDades = baseDeDades;
			baseDeDades.Conecta();
			controlObj = new LlistaOrdenada<ulong, ObjecteSql>();
			temporitzadorActualitzacions = new System.Timers.Timer();
			temporitzadorActualitzacions.Interval = (int)TempsEnMiliSegons.hora;
			temporitzadorActualitzacions.Enabled = true;
			temporitzadorActualitzacions.Elapsed += new System.Timers.ElapsedEventHandler(ComprovaActualitzacionsEvent);
			semaforActualitzacions = new Semaphore(1, 1);
			Creates();
            if(restaurarTotesDades)
               Restaurar();
		}

		public ControlObjectesSql(TipusBaseDeDades tipusBD, StringCreate[] creates, bool restaurarTotesDades = true)
			: this(DonamBD(tipusBD), creates,restaurarTotesDades)
		{
		}
		public ControlObjectesSql(StringCreate[] creates, bool restaurarTotesDades = true)
			: this(TipusBaseDeDades.MySql, creates,restaurarTotesDades)
		{
		}
		protected BaseDeDades BaseDeDades {
			get {
				return baseDeDades;
			}
		}
		public double TempsComprovacioActualitzacions {
			get { return temporitzadorActualitzacions.Interval; }
			set { temporitzadorActualitzacions.Interval = value; }
		}

		public string[] CreatesSql {
			get {
				return creates;
			}

			set {
				if (value == null || value.Length == 0)
					throw new ArgumentException("Los Creates se usan para poder crear,resetear,eliminar y migrar la BD");
				
				string[] tablas = new string[value.Length];
                string nombre;
                int indexParentesis;
                for (int i = 0; i < tablas.Length; i++) {
                    nombre = value[i].Split(' ')[2];//create table nombreTabla...
                    if(nombre.Contains('('))
                    {
                        indexParentesis = nombre.IndexOf('(');
                        nombre = nombre.Remove(indexParentesis, nombre.Length - indexParentesis);
                    }
                    tablas[i] = nombre.ToLower();
                }
				if (tablas.Distinct().Count() != tablas.Length) {
					throw new Exception("Hay dos o mas creates para una misma tabla...");
				}
				creates = value;
				this.tablas = tablas;
			}
		}

		public Tipus[] DonamObjectes<Tipus>() where Tipus:ObjecteSql
		{
			return this.OfType<Tipus>().ToArray<Tipus>();
		}


		public void Afegir(ObjecteSql objSql)
		{
			if (objSql != null)
				if (!controlObj.ContainsKey(objSql.IdIntern)) {
				try {
					baseDeDades.ConsultaSQL(objSql.StringInsertSql(baseDeDades.TipusBD));//si peta no lo pone...
					if (objSql is ObjecteSqlIdAuto) {
						objSql.PrimaryKey = baseDeDades.ConsultaUltimID();
						objSql.DessaCanvis();
					}

				} catch {
				} finally {
					if (!controlObj.ContainsKey(objSql.IdIntern)) {
						semaforActualitzacions.WaitOne();
						controlObj.Add(objSql.IdIntern, objSql);
						semaforActualitzacions.Release();
						try {
							objSql.Baixa += Treu;
							objSql.Actualitzat += ComprovaActualitzacions;
							objSql.Alta -= Afegir;
						} catch {
						}
						if (ObjNou != null)
							ObjNou(objSql);
					}
				}
			}
		}
		public void Afegir(IEnumerable<ObjecteSql> objectesSql)
		{
			if (objectesSql != null)
				foreach (ObjecteSql obj in objectesSql)
					Afegir(obj);
		}
		public void Treu(ObjecteSql objSql)
		{
			if (objSql != null)
				Treu(objSql.IdIntern);
		}
		public void Treu(IEnumerable<ObjecteSql> objectesSql)
		{
			if (objectesSql != null)
				foreach (ObjecteSql obj in objectesSql)
					Treu(obj);
		}
		public void Treu(ulong idInternObjSql)
		{
			if (controlObj.ContainsKey(idInternObjSql)) {
				try {
					baseDeDades.ConsultaSQL(controlObj[idInternObjSql].StringDeleteSql());//elimina de la base de dades,si peta no el treu...
					controlObj[idInternObjSql].Baixa -= new ObjecteSqlEventHandler(Treu);
					controlObj[idInternObjSql].Actualitzat -= ComprovaActualitzacions;
					controlObj[idInternObjSql].Alta += new ObjecteSqlEventHandler(Afegir);
					controlObj.Remove(idInternObjSql);
				} catch {
				}


			}
		}
		private void ComprovaActualitzacionsEvent(object sender, System.Timers.ElapsedEventArgs e)
		{
			semaforActualitzacions.WaitOne();
			foreach (KeyValuePair<ulong,ObjecteSql> obj in controlObj) {
				ComprovaActualitzacions(obj.Value);
			}
			semaforActualitzacions.Release();




		}
		public void ComprovaActualitzacions()
		{
			ComprovaActualitzacionsEvent(null, null);
		}
		/// <summary>
		/// Crea si no existeixen les taules per defecte Productes,Receptes,Ingredients,UnitatsProductes
		/// </summary>
		public  void Creates()
		{
            //por probar
			for (int i = 0; i < creates.Length; i++)
              if (!BaseDeDades.ExisteixTaula(tablas[i]))
                     BaseDeDades.ConsultaSQL(creates[i]);

		}
		public  void Drops()
		{
			for (int i = 0; i < tablas.Length; i++)
				BaseDeDades.DropTable(tablas[i]);
		}
		public void Reset()
		{
			Drops();
			Creates();
		}
		public bool EstructuraTablasIdentica(BaseDeDades bdDestino)
		{//por probar
			bool correcto=true;
			for(int i=0;i<tablas.Length&&correcto;i++)
			{
				if(BaseDeDades.ExisteixTaula(tablas[i]))
				{
					correcto=bdDestino.DescTable(tablas[i])==BaseDeDades.DescTable(tablas[i]);
				}
			}
			return correcto;
		}
		public void MigrarBD(BaseDeDades bdDestino, bool borrarDatosBdActual = false, bool resetBDTablasAfactadasDestino = true)
		{//por testear...
			if(!resetBDTablasAfactadasDestino&&!EstructuraTablasIdentica(bdDestino))
				throw new SQLException("La base de datos del destino no tiene la misma estructura en las tablas");
			ObjecteSql[] objs = this.controlObj.ValuesToArray();
			if (borrarDatosBdActual)
				Drops();
			this.baseDeDades = bdDestino;
			if (resetBDTablasAfactadasDestino)
				Drops();
			Creates();//crea las que no existan
			Afegir(objs);//añade
		}
		protected abstract void Restaurar();
		private static BaseDeDades DonamBD(TipusBaseDeDades tipusBD)
		{
			BaseDeDades bd = null;
			switch (tipusBD) {
					//case TipusBaseDeDades.Acces: bd = new BaseDeDadesAcces(); break;
				case TipusBaseDeDades.MySql:
					bd = new BaseDeDadesMySQL();
					break;
			}
			return bd;
		}

		public IEnumerator<ObjecteSql> GetEnumerator()
		{
			semaforActualitzacions.WaitOne();
			foreach (KeyValuePair<ulong,ObjecteSql> obj in controlObj)
				yield return obj.Value;
			semaforActualitzacions.Release();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public override bool Equals(object obj)
		{
			ControlObjectesSql other = obj as ControlObjectesSql;
			bool iguals = other != null;
			bool potPrimer = true, potSegon = false;
			IEnumerator<ObjecteSql> dit1 = null, dit2 = null;
			if (iguals) {
				dit1 = GetEnumerator();
				dit2 = other.GetEnumerator();
				while (iguals && potPrimer) {
					potPrimer = dit1.MoveNext();
					potSegon = dit2.MoveNext();
					if (potSegon.Equals(potPrimer)) {
						if (potPrimer)
							iguals = dit2.Current.Equals(dit1.Current);
					} else
						iguals = false;
				}
				if (iguals)
					iguals = dit1.MoveNext().Equals(dit2.MoveNext());
			}

			return iguals;
		}
		public override string ToString()
		{
			Gabriel.Cat.text toString = "Objectes Controlats:\n";
			foreach (ObjecteSql obj in this)
				toString += "\n" + obj;
			return toString;
		}

		public void ComprovaActualitzacions(ObjecteSql obj)
		{

			string upDate = null;

			upDate = obj.StringUpdateSql(baseDeDades.TipusBD);
			if (upDate != null) {
				try {
					obj.Actualitzat -= ComprovaActualitzacions;
					baseDeDades.ConsultaSQL(upDate);
					obj.DessaCanvis();
					
				} catch (SQLException m) {//mirar si hace falta el if...
//					if (((text)(m.Message.ToString())).CountSubString("PrimaryKey") > 0)//si da problemas en la primarykey por el update es que no se puede poner esa...
					obj.RestauraPrimaryKey(m.Message);//la restauro...
				} finally {
					obj.Actualitzat += ComprovaActualitzacions;
				}
			}

			
		}
	}
}
