using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;

using System.Threading;

namespace Gabriel.Cat
{
	public enum TempsEnMiliSegons:ulong
	{
		segon = 1000,
		minut = segon * 60,
		hora = minut * 60,
		dia = hora * 24,
		semana = dia * 7,
		mes = semana * 4,
		any = mes * 12,

	}
	public delegate void ObjecteNouEventHandler(ObjecteSql objNew);
	public  abstract class ControlObjectesSql : IEnumerable<ObjecteSql>
	{

		public event ObjecteNouEventHandler ObjNou;
		LlistaOrdenada<ulong, ObjecteSql> controlObj;
		private BaseDeDades baseDeDades;
		System.Timers.Timer temporitzadorActualitzacions;
		Semaphore semaforActualitzacions;

		public ControlObjectesSql(BaseDeDades baseDeDades)
		{
			this.baseDeDades = baseDeDades;
			baseDeDades.Conecta();
			controlObj = new LlistaOrdenada<ulong, ObjecteSql>();
			temporitzadorActualitzacions = new System.Timers.Timer();
			temporitzadorActualitzacions.Interval = (int)TempsEnMiliSegons.hora;
			temporitzadorActualitzacions.Enabled = true;
			temporitzadorActualitzacions.Elapsed += new System.Timers.ElapsedEventHandler(ComprovaActualitzacionsEvent);
			semaforActualitzacions = new Semaphore(1, 1);
			Creates();
		}

		public ControlObjectesSql(TipusBaseDeDades tipusBD)
			: this(DonamBD(tipusBD))
		{
		}
		public ControlObjectesSql()
			: this(TipusBaseDeDades.MySql)
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
		public Tipus[] DonamObjectes<Tipus>()
		{
			return this.OfType<Tipus>().ToArray<Tipus>();
		}


		public void Afegir(ObjecteSql objSql)
		{
			if (objSql != null)
			if (!controlObj.Existeix(objSql.IdIntern)) {
				try {
					baseDeDades.ConsultaSQL(objSql.StringInsertSql(baseDeDades.TipusBD));//si peta no lo pone...
					if (objSql is ObjecteSqlIdAuto) {
						objSql.PrimaryKey = baseDeDades.ConsultaUltimID();
						objSql.DessaCanvis();
					}

				} catch {
				} finally {
					if (!controlObj.Existeix(objSql.IdIntern)) {
						semaforActualitzacions.WaitOne();
						controlObj.Afegir(objSql.IdIntern, objSql);
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
			if (controlObj.Existeix(idInternObjSql)) {
				try {
					baseDeDades.ConsultaSQL(controlObj[idInternObjSql].StringDeleteSql());//elimina de la base de dades,si peta no el treu...
					controlObj[idInternObjSql].Baixa -= new ObjecteSqlEventHandler(Treu);
					controlObj[idInternObjSql].Actualitzat -= ComprovaActualitzacions;
					controlObj[idInternObjSql].Alta += new ObjecteSqlEventHandler(Afegir);
				    controlObj.Elimina(idInternObjSql);
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
		public  abstract void Creates();
		public abstract void Drops();
		public void Reset()
		{
			Drops();
			Creates();
		}
        public void MigrarBD(BaseDeDades bdDestion,bool borrarDatosBdActual=false,bool resetBDTablasAfactadasDestino=true)
        {//por testear...
            ObjecteSql[] objs = this.controlObj.ValuesToArray();
            if (borrarDatosBdActual)
                Drops();
            this.baseDeDades = bdDestion;
            if (resetBDTablasAfactadasDestino)
                Drops();//lo tengo que hacer porque no se como son las tablas de la bd actual...mirar de comprobar si es compatible si lo es no se resetea...solo se pasan los datos...a no ser que se ponga true lo de hacer reset
            Creates();//crea las que no existan
            Afegir(objs);//añade...deberia petar si la estructura es diferente de la tabla destino...
        }
		public abstract dynamic Restaurar();
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
