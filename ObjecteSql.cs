using Gabriel.Cat.Seguretat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
	public delegate void ObjecteSqlEventHandler(ObjecteSql sqlObj);
	public delegate void PrimaryKeyControlEventHandler(ObjecteSql obj, InfoEventArgs informacioEvent);
	public enum TipusBaseDeDades
	{
		Acces,
		MySql,
	//	Oracle
	}
    public enum CampXifratTipus
    {
        Text,Int,Long,Double
    }
	public abstract class ObjecteSql : IComparable<ObjecteSql>
	{
		private struct DadesSql
		{
			private bool esDateTimeCamp;
			private string dades;
			public DadesSql(string dades, bool necessitaCanvi)
			{
				this.dades = dades;
				this.esDateTimeCamp = necessitaCanvi;
			}

			public bool EsDataTimeCamp {
				get {
					return esDateTimeCamp;
				}
			}

			public string Dades {
				get {
					return dades;
				}
			}
		}
		public event ObjecteSqlEventHandler Actualitzat;
		public event ObjecteSqlEventHandler Alta;
		public event ObjecteSqlEventHandler Baixa;
		public event PrimaryKeyControlEventHandler PrimaryKeyChanged;
		private LlistaOrdenada<string, DadesSql?> canvisObj;
        private LlistaOrdenada<string, string> campsXifrats;
		string primaryKey;
		string primaryKeyAnt;
		string taula;
		string campPrimaryKey;
		ulong idIntern;
		TipusBaseDeDades tipusBD;
		static ulong genID = 0;
		static Semaphore semaforId = new Semaphore(1, 1);
		#region Constructors
		public ObjecteSql()
		{

			canvisObj = new LlistaOrdenada<string, DadesSql?>();
            campsXifrats = new LlistaOrdenada<string, string>();
			campPrimaryKey = "Id";
			taula = "";
			primaryKey = "";
			primaryKeyAnt = "";
			AltaCanvi(CampPrimaryKey);
			semaforId.WaitOne();
			idIntern = genID++;
			semaforId.Release();
			tipusBD = TipusBaseDeDades.MySql;
		}
		public ObjecteSql(string nomTaula, string primaryKey, string campPrimaryKey)
			: this()
		{
			this.CampPrimaryKey = campPrimaryKey;
			PrimaryKey = primaryKey;
			taula = nomTaula;
			this.tipusBD = TipusBaseDeDades.MySql;
			DessaCanvis();
		}
		public ObjecteSql(TipusBaseDeDades tipusBD, string nomTaula, string primaryKey, string campPrimaryKey)
			: this(nomTaula, primaryKey, campPrimaryKey)
		{

			this.tipusBD = tipusBD;

		}
		#endregion
		#region Propietats
		public ulong IdIntern {
			get { return idIntern; }
		}

		public TipusBaseDeDades TipusBD {
			get {
				return tipusBD;
			}
			set {
				tipusBD = value;
			}
		}

		public string Taula {
			get { return taula; }
			set {
				if (value == null)
					value = "";
				if (Baixa != null)
					Baixa(this);
				taula = value;
				if (Alta != null)
					Alta(this);
			}
		}
		public string CampPrimaryKey {
			get { return campPrimaryKey; }
			set {

				if (!String.IsNullOrEmpty(value)) {
					value = value.ToUpper();
				}
				BaixaCanvi(campPrimaryKey);
				campPrimaryKey = value;
				AltaCanvi(campPrimaryKey);
			}
		}
		public virtual string PrimaryKey {
			get { return primaryKey; }
			set {
				if (value == null)
					value = "";

				primaryKey = value;
				if (primaryKey != "") {
					if (primaryKeyAnt != "")
						CanviString(CampPrimaryKey, primaryKey);
					else
						primaryKeyAnt = primaryKey;
				}
			}
		}
        protected string PrimaryKeyAnt
        {
            get { return primaryKeyAnt; }
        }
        #endregion
        #region Alta Proteccio
        protected void AltaProteccio(Type enumCampType)
        {
            Enum[] enumCampsAProtegir = Enum.GetValues(enumCampType).Cast<Enum>().ToArray();
            for (int i = 0; i < enumCampsAProtegir.Length; i++)
                AltaProteccio(enumCampsAProtegir[i]);
        }
        protected void AltaProteccio(Enum enumCamp)
        {
            AltaProteccio(enumCamp.ToString());
        }
        protected void AltaProteccio(string nomCamp)
        {
            if (!campsXifrats.ContainsKey(nomCamp))
                campsXifrats.Add(nomCamp, nomCamp);
        }
        protected void BaixaProteccio(Enum enumCamp)
        {
            BaixaProteccio(enumCamp.ToString());
        }
        protected void BaixaProteccio(string nomCamp)
        {
            if (!campsXifrats.ContainsKey(nomCamp))
                campsXifrats.Add(nomCamp, nomCamp);
        }
        #endregion
        protected bool[] AltaCanvis(Type enumType)
        {
            Enum[] enumADonarDalta =Enum.GetValues(enumType).Cast<Enum>().ToArray();
            bool[] resultAlta = new bool[enumADonarDalta.Length];
            for (int i = 0; i < enumADonarDalta.Length; i++)
                resultAlta[i] = AltaCanvi(enumADonarDalta[i]);
            return resultAlta;
        }
        protected bool AltaCanvi(Enum enumCaluCanvi)
        {
            return AltaCanvi(enumCaluCanvi.ToString());
        }
		protected bool AltaCanvi(string clauCanvi)
		{
			bool alta = !ExisteixCanvi(clauCanvi);
			if (alta)
				canvisObj.Add(clauCanvi, null);
			return alta;
		}
        protected bool BaixaCanvi(Enum enumCaluCanvi)
        {
            return BaixaCanvi(enumCaluCanvi.ToString());
        }
        protected bool BaixaCanvi(string clauCanvi)
		{
			bool baixa = ExisteixCanvi(clauCanvi);
			if (baixa)
				canvisObj.Remove(clauCanvi);
			return baixa;
		}
        protected bool ExisteixCanvi(Enum enumCaluCanvi)
        {
            return ExisteixCanvi(enumCaluCanvi.ToString());
        }
        protected bool ExisteixCanvi(string clauCanvi)
		{
			return canvisObj.ContainsKey(clauCanvi);
		}
        protected void CanviNumero(Enum enumCaluCanvi,string numero)
        {
            CanviNumero(enumCaluCanvi.ToString(),numero);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clauCanvi"></param>
        /// <param name="numero"></param>
        protected void CanviNumero(string clauCanvi, string numero)
		{
            
			if (!ExisteixCanvi(clauCanvi))
				throw new Exception("Error la clau:" + clauCanvi + " no s'ha donat d'alta");
            if (numero == null) numero = "NULL";
			canvisObj[clauCanvi] = new DadesSql?(new DadesSql(numero, false));
		}
        protected void CanviString(Enum enumCaluCanvi, string text)
        {
            CanviString(enumCaluCanvi.ToString(), text);
        }
        protected void CanviString(string clauCanvi, string text)
		{
			if (!ExisteixCanvi(clauCanvi))
				throw new Exception("Error la clau:" + clauCanvi + " no s'ha donat d'alta");
            if (text == null) text = "NULL";
            else text = "'" + text + "'";
            canvisObj[clauCanvi] = new DadesSql?(new DadesSql(text, false));
		}
        protected void CanviData(Enum enumCaluCanvi, DateTime data)
        {
            CanviData(enumCaluCanvi.ToString(), data);
        }


        /// <summary>
        /// Dona d'alta el canvi en format mysql
        /// </summary>
        /// <param name="clauCanvi"></param>
        /// <param name="data"></param>
        protected void CanviData(string clauCanvi, DateTime data)
		{
			CanviData(clauCanvi, data.ToShortDateString());
		}
    

        protected void CanviData(Enum enumCaluCanvi, string data)
        {
            CanviData(enumCaluCanvi.ToString(), data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clauCanvi"></param>
        /// <param name="data">En format ShortDate '%d/%m/%Y'</param>
        protected void CanviData(string clauCanvi, string data)
		{
			if (!ExisteixCanvi(clauCanvi))
				throw new Exception("Error la clau:" + clauCanvi + " no s'ha donat d'alta");
            if (data == null) data = "NULL";
			canvisObj[clauCanvi] = new DadesSql?(new DadesSql(data, true));

		}
        protected void CanviTimeSpan(Enum enumCaluCanvi, TimeSpan temps)
        {
            CanviTimeSpan(enumCaluCanvi.ToString(), temps);
        }
        protected void CanviTimeSpan(string clauCanvi, TimeSpan temps)
		{
			CanviString(clauCanvi, temps.Days + ";" + temps.Hours + ";" + temps.Minutes + ";" + temps.Seconds + ";" + temps.Milliseconds);
		}
        protected void TreuCanvi(Enum enumCaluCanvi)
        {
            TreuCanvi(enumCaluCanvi.ToString());
        }
        protected void TreuCanvi(string clauCanvi)
		{
			if (ExisteixCanvi(clauCanvi))
				canvisObj[clauCanvi] = null;
		}
		public virtual void OnAlta()
		{
			if (Alta != null)
				Alta(this);

		}
		public virtual void OnBaixa()
		{
			if (Baixa != null)
				Baixa(this);
		}
		public void OnActualitzat()
		{
			if (Actualitzat != null)
				Actualitzat(this);
		}
		public void ActualitzaPrimaryKey()
		{
			if (PrimaryKeyChanged != null)
				PrimaryKeyChanged(this, new InfoEventArgs(primaryKeyAnt, primaryKey, "Actualització de la clau primaria"));
			primaryKeyAnt = primaryKey;
		}
		public void RestauraPrimaryKey()
		{
			RestauraPrimaryKey("Desactualització de la clau primaria");
		}

		public void RestauraPrimaryKey(string causa)
		{
			if (PrimaryKeyChanged != null)
				PrimaryKeyChanged(this, new InfoEventArgs(primaryKeyAnt, primaryKey, causa));
			primaryKey = primaryKeyAnt;
		}
		public void DessaCanvis()
		{
			if (Actualitzat != null)
				Actualitzat(this);
			List<string> claus = new List<string>();
			foreach (KeyValuePair<string,DadesSql?> keyValue in canvisObj)
				claus.Add(keyValue.Key);
			for (int i = 0; i < claus.Count; i++)
				canvisObj[claus[i]] = null;
			ActualitzaPrimaryKey();
		}
        public int CompareTo(ObjecteSql other)
        {
            return IdIntern.CompareTo(other.IdIntern);
        }
        public override bool Equals(object obj)
        {
            ObjecteSql other = obj as ObjecteSql;
            bool iguals = other != null;
            if (iguals)
                iguals = other.Taula == Taula && other.PrimaryKey == PrimaryKey;
            return iguals;
        }
        #region sentenciesSQL
        public string StringInsertSql(Key keyXifrat = null)
		{
			return StringInsertSql(tipusBD,keyXifrat);
		}
		public abstract string StringInsertSql(TipusBaseDeDades tipusBD,Key keyXifrat=null);
		public string StringUpdateSql(Key keyXifrat = null)
		{
			return StringUpdateSql(tipusBD,keyXifrat);
		}
        public  string StringUpDateCampsXifrats(Key oldKey, Key newKey)
        {
            if (newKey == null)
                newKey = new Key();
            if (oldKey == null)
                oldKey = new Key();
            return IStringUpDateCampsXifrats(oldKey,newKey);
        }
        /// <summary>
        /// String que actualiza los campos cifrados
        /// </summary>
        /// <param name="oldKey">si cifran la primaryKey se necesita, nunca sera null</param>
        /// <param name="newKey">para cifrar los campos,nunca sera null</param>
        /// <returns></returns>
        protected abstract string IStringUpDateCampsXifrats(Key oldKey, Key newKey);

        public string StringUpdateSql(TipusBaseDeDades tipusBD,Key keyXifrat = null)
		{
            if (campsXifrats.Count > 0 && keyXifrat == null)
                throw new ArgumentNullException("keyXifrat");
			StringBuilder strUpdate = new StringBuilder();
            StringBuilder strAux = new StringBuilder();
            List<string> canvis;

            if (ComprovacioSiEsPot()) {
				canvis = new List<string>();
				foreach (KeyValuePair<string, DadesSql?> campValor in canvisObj)
					if (campValor.Value != null) {

                        if (campValor.Key != "Id")
                        {
                            strAux.Append(campValor.Key);
                            strAux.Append("=");
                            if (!campValor.Value.Value.EsDataTimeCamp)
                            {
                                if (campsXifrats.ContainsKey(campValor.Key))
                                {
                                    strAux.Append(keyXifrat.Encrypt(campValor.Value.Value.Dades));
                                }
                                else
                                    strAux.Append(campValor.Value);
                            }
                            else
                            {

                                if (campsXifrats.ContainsKey(campValor.Key))
                                {
                                    strAux.Append(keyXifrat.Encrypt(DateTimeToStringSQL(tipusBD, campValor.Value.Value.Dades)));
                                }
                                else
                                    strAux.Append(DateTimeToStringSQL(tipusBD, campValor.Value.Value.Dades));

                            }
                        }
                        else
                        {
                            strAux.Append(CampPrimaryKey);
                            strAux.Append("=");
                            if (campsXifrats.ContainsKey(CampPrimaryKey))
                            {
                                strAux.Append(keyXifrat.Encrypt(primaryKeyAnt));
                            }else
                            strAux.Append(primaryKeyAnt);
                        }

                        canvis.Add(strAux.ToString());
                        strAux.Clear();
				}
				if (canvis.Count > 0) {
                    strUpdate.Append("update ");
                    strUpdate.Append(taula);
                    strUpdate.Append(" set ");
                    for (int i = 0,f= canvis.Count - 1; i < f; i++)
                    {
                        strUpdate.Append(canvis[i]);
                        strUpdate.Append(",");
                    }
                    strUpdate.Append(canvis[canvis.Count - 1]);
                    strUpdate.Append(" where ");
                    strUpdate.Append(CampPrimaryKey);
                    strUpdate.Append("='");
                    if (!campsXifrats.ContainsKey(CampPrimaryKey))
                    {
                        strUpdate.Append(primaryKeyAnt);
                    }else
                    {
                        strUpdate.Append(keyXifrat.Encrypt(primaryKeyAnt));
                    }
                    strUpdate.Append("'");
				}
			}
            return strUpdate.ToString();
		}
		public string StringConsultaSql(Key xifratCamps=null)
		{
            if (campsXifrats.ContainsKey(CampPrimaryKey) && xifratCamps == null)
                throw new ArgumentNullException("xifratCamps");
            StringBuilder strConsulta = new StringBuilder();
            if (ComprovacioSiEsPot())
            {
                strConsulta.Append("select * from ");
                strConsulta.Append(Taula);
                strConsulta.Append(" where ");
                strConsulta.Append(CampPrimaryKey);
                strConsulta.Append("='");
                if (campsXifrats.ContainsKey(CampPrimaryKey))
                {
                    strConsulta.Append(xifratCamps.Encrypt(primaryKey));
                } else {

                    strConsulta.Append(primaryKey);
                } 
               strConsulta.Append("';");
            }
			return strConsulta.ToString();
		}
		protected virtual bool ComprovacioSiEsPot()
		{
			return taula != "" && primaryKey != "" && CampPrimaryKey != "" && primaryKeyAnt != "";
		}
		public string StringDeleteSql(Key xifratCamps=null)
		{
            if (campsXifrats.ContainsKey(CampPrimaryKey) && xifratCamps == null)
                throw new ArgumentNullException("xifratCamps");
            StringBuilder  strDelete=new StringBuilder();
			if (ComprovacioSiEsPot()) {
                strDelete.Append("delete from ");
                strDelete.Append(taula);
                strDelete.Append(" where ");
                strDelete.Append(CampPrimaryKey);
                strDelete.Append("='");
                if (campsXifrats.ContainsKey(CampPrimaryKey))
                {
                    strDelete.Append(xifratCamps.Encrypt(primaryKey));
                }
                else
                {

                    strDelete.Append(primaryKey);
                }
                strDelete.Append("'");
			}
			return strDelete.ToString();
		}

        #endregion
        public static string DoubleToString(TipusBaseDeDades tipusBD, int precicion)
        {
            string doubleString = "";
            switch (tipusBD)
            {
                case TipusBaseDeDades.Acces: doubleString = "doube"; break;
                case TipusBaseDeDades.MySql: doubleString = "float(" + precicion + ")"; break;
                    //  case TipusBaseDeDades.Oracle: 
            }
            return doubleString;
        }
        /// <summary>
        /// Convertiex si pot l'string en TimeSpan,
        /// Pot llençar excepcions si els numeros no son convertibles a int32
        /// </summary>
        /// <param name="timeSpan">string amb format d;h;m;s;mili</param>
        /// <exception cref="Exception">Excepció produïda al convertir a Int32 una part del timeSpan</exception>
        /// <returns>si no esta bé l'string retorna un new TimeSpan()</returns>
        public static TimeSpan StringToTimeSpan(string timeSpan)
		{
			TimeSpan timeSpanRespota = new TimeSpan();
			string[] camps;
			if (timeSpan != null)
				if (timeSpan.Contains(';')) {
				camps = timeSpan.Split(';');
				if (camps.Length == 5) {
					try {
						timeSpanRespota = new TimeSpan(Convert.ToInt32(camps[0]), Convert.ToInt32(camps[1]), Convert.ToInt32(camps[2]), Convert.ToInt32(camps[3]), Convert.ToInt32(camps[4]));
					} catch {
						throw new Exception("L'String no te el format adequat!\n" + timeSpan);
					}
				}
			}
			return timeSpanRespota;
		}
		public static string TimeSpanToString(TimeSpan temps)
		{
			return "'" + temps.Days + ";" + temps.Hours + ";" + temps.Minutes + ";" + temps.Seconds + ";" + temps.Milliseconds + "'";
		}
		public static string DateTimeToString(DateTime data)
		{
            StringBuilder strData = new StringBuilder();
            strData.Append(data.Year);
            strData.Append(";");
            strData.Append(data.Month);
            strData.Append(";");
            strData.Append(data.Day);
            strData.Append(";");
            strData.Append(data.Hour);
            strData.Append(";");
            strData.Append(data.Minute);
            strData.Append(";");
            strData.Append(data.Second);
            strData.Append(";");
            strData.Append(data.Millisecond);

            return strData.ToString();
        }
		/// <summary>
		/// Dona una string de MySql
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string DateTimeToStringSQL(DateTime data)
		{
			return DateTimeToStringSQL(TipusBaseDeDades.MySql, data);
		}
   
		public static string DateTimeToStringSQL(TipusBaseDeDades baseDeDadesDesti, DateTime data)
		{
			return DateTimeToStringSQL(baseDeDadesDesti, data.ToString(@"MM\/dd\/yyyy HH:mm:ss"));
		}

        public static string DateTimeToStringSQL(TipusBaseDeDades baseDeDadesDesti, string data)
		{//se tiene que probar
			string dateTimeToStringSQL = "";
			switch (baseDeDadesDesti) {
				case TipusBaseDeDades.MySql:
					dateTimeToStringSQL = "str_to_date('" + data + "', '%d/%m/%Y %H:%i:%s')";
					break;
			/*	case TipusBaseDeDades.Oracle:
					dateTimeToStringSQL = "to_date('" + data + "', 'DD/MM/YYYY HH:MI:SS')";
					break;*/
				case TipusBaseDeDades.Acces:
					dateTimeToStringSQL = "Format (#" + data + "#, \"dd/mm/yyyy hh:mm:ss\")";
					break;
			}
			return dateTimeToStringSQL;
		}
        public static string StringAutoIncrement(TipusBaseDeDades tipusBD)
        { return StringAutoIncrement(tipusBD, typeof(long)); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipusBD"></param>
        /// <param name="type">int o long</param>
        /// <returns>si devuelve null es que no es compatible con el tipo</returns>
        public static string StringAutoIncrement(TipusBaseDeDades tipusBD, Type type)
        {
            string strAutoIncrement = null;
            switch (tipusBD)
            {
                case TipusBaseDeDades.MySql:
                    switch (type.AssemblyQualifiedName)
                    {
                        case Serializar.LONGASSEMBLYNAME: strAutoIncrement = " bigint AUTO_INCREMENT"; break;
                        case Serializar.INTASSEMBLYNAME: strAutoIncrement = " int AUTO_INCREMENT"; break;
                            //cuando necesite mas tipos los añado :D
                    }
                    break;
                case TipusBaseDeDades.Acces:
                    switch (type.AssemblyQualifiedName)
                    {
                        case Serializar.INTASSEMBLYNAME: strAutoIncrement = " AUTOINCREMENT"; break;
                        case Serializar.LONGASSEMBLYNAME: strAutoIncrement = " COUNTER"; break;
                    }
                    break;
                    //Oracle no tiene ...tiene secuencias pero se usan distintamente...
            }
            return strAutoIncrement;
        }
        public static string StringAutoIncrementReferenceNumber(TipusBaseDeDades tipusBD)
        {
            return StringAutoIncrementReferenceNumber(tipusBD, typeof(long));
        }
        /// <summary>
        /// Se usa para saber el tipo de la columna foreing key que hace referencia a una columna id autoincrement
        /// </summary>
        /// <param name="tipusBD"></param>
        /// <param name="type">int o long</param>
        /// <returns>si devuelve null es que no es compatible con el tipo</returns>
        public static string StringAutoIncrementReferenceNumber(TipusBaseDeDades tipusBD, Type type)
        {
            string strAutoIncrement = null;
            switch (tipusBD)
            {
                case TipusBaseDeDades.MySql:
                    switch (type.AssemblyQualifiedName)
                    {
                        case Serializar.LONGASSEMBLYNAME: strAutoIncrement = " bigint "; break;
                        case Serializar.INTASSEMBLYNAME: strAutoIncrement = " int "; break;
                            //cuando necesite mas tipos los añado :D
                    }
                    break;
                case TipusBaseDeDades.Acces:
                    switch (type.AssemblyQualifiedName)
                    {
                        default: strAutoIncrement = " long "; break;//por mirar
                    }
                    break;
                    //Oracle no tiene ...tiene secuencias pero se usan distintamente...
            }
            return strAutoIncrement;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data">el format serà any;mes;dia;hora;minuts;segons;milisegons</param>
		/// <exception cref="Exception">Excepció produïda al convertir a Int32 una part de la data</exception>
		/// <returns>retorna un new DateTime() si no es pot realitzar la operació</returns>
		protected static DateTime StringToDateTime(string data)
		{
			DateTime dateTimeConvertit = new DateTime();
			string[] camps;
			if (data != null)
				if (data.Contains(';')) {
				camps = data.Split(';');
				if (camps.Length == 3) {
					try {
						dateTimeConvertit = new DateTime(Convert.ToInt32(camps[0]), Convert.ToInt32(camps[1]), Convert.ToInt32(camps[2]), Convert.ToInt32(camps[3]), Convert.ToInt32(camps[4]), Convert.ToInt32(camps[5]), Convert.ToInt32(camps[6]));
					} catch {
						throw new Exception("El format no es el correcte\n" + data);
					}
				}
			}
			return dateTimeConvertit;
			
		}
        /// <summary>
        /// Convierte  la string devuelta por la base de datos con el campo tipo DateTime en DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="tipus"></param>
        /// <returns></returns>
        public static DateTime StringBdDateTimeToDateTime(string stringBdDateTime,TipusBaseDeDades tipus)
        {
            return DateTime.Now;//por hacer
        }
        /// <summary>
        /// Convierte un DateTime en la string devuelta por la base de datos con el campo tipo DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="tipus"></param>
        /// <returns></returns>
        public static string DateTimeToStringBdDateTime(DateTime dateTime, TipusBaseDeDades tipus)
        {
            return String.Empty;//por hacer
        }
    }
	public class InfoEventArgs : EventArgs
	{
		string primaryKeyAnterior;
		string primaryKeyActual;
		private string causa;


		public InfoEventArgs(string primaryKeyAnt, string primaryKeyActual)
		{
			this.primaryKeyActual = primaryKeyActual;
			this.primaryKeyAnterior = primaryKeyAnt;
			causa = "";
		}

		public InfoEventArgs(string primaryKeyAnt, string primaryKey, string causa)
			: this(primaryKeyAnt, primaryKey)
		{
			this.causa = causa;
		}
		public string PrimaryKeyActual {
			get { return primaryKeyActual; }
		}
		public string PrimaryKeyAnterior {
			get { return primaryKeyAnterior; }

		}
		public string Causa {
			get { return causa; }
		}
	}
}
