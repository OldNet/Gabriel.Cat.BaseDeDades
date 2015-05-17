using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;


namespace Gabriel.Cat
{
  public  class BaseDeDadesOracle:BaseDeDades
    {
        OracleConnection cnOracle;
        
        public BaseDeDadesOracle()
        {
            nomBaseDades = "Oracle";
            connectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe)));User Id=system;Password=linux;";
        }
        public BaseDeDadesOracle(string connexion)
        {
        	this.connectionString=connexion;
        }
        public override bool Conecta()
        {
            bool conectat=true;
            try
            {
                cnOracle = new OracleConnection(connectionString);
                cnOracle.Open();
            }
            catch { conectat = false; }
        
            return conectat;
          
        }

        public override void Desconecta()
        {
            cnOracle.Close();
        }
        /// <summary>
        /// posa el host i el port per defecte
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string user, string password)
        {
            return DonamStringConnection("locahost", user, password);
        }
        /// <summary>
        ///  posa el port per defecte
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string ipHost, string user, string password)
        {
            return DonamStringConnection(ipHost, 1521, user, password);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string ipHost, int port, string user, string password)
        {
            return "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST="+ipHost+")(PORT="+port+")))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe)));User Id="+user+";Password="+password+";";
        }


        public override string[,] ConsultaTableDirect(string nomTaula)
        {
            OracleCommand comand = cnOracle.CreateCommand();
            string[] campos = null;
            comand.CommandType = CommandType.TableDirect;
            comand.CommandText = nomTaula;
            System.Collections.Generic.List<string[]> llistaTaula = null;
            string[,] taula;
            try
            {
              
                OracleDataReader reader = comand.ExecuteReader();
                llistaTaula = new System.Collections.Generic.List<string[]>();
                //poso el nom de les columnes
                campos = new string[reader.FieldCount];
                for (int i = 0; i < campos.Length; i++)
                    campos[i] = reader.GetName(i);
                llistaTaula.Add(campos);
                //poso les files
                foreach (System.Data.Common.DbDataRecord fila in reader)
                {
                    campos = new string[fila.FieldCount];
                    for (int i = 0; i < campos.Length; i++)
                        campos[i] = fila.GetValue(i).ToString();
                    llistaTaula.Add(campos);
                }

                reader.Close();
/// <summary>
/// Pone los valores en la matriz en su sitio
/// </summary>
                taula=new string[llistaTaula.Count,llistaTaula[0].Length];
                for(int y=0;y<llistaTaula.Count;y++)
                	for(int x=0;x<llistaTaula[0].Length;x++)
                		taula[y,x]=llistaTaula[y][x];
            }
            catch { }


            return taula;
        }

        public override string[,] ConsultaSQL(string SQL)
        {
            OracleCommand comand = cnOracle.CreateCommand();
            ResultatSQL resultat=ResultatSQL.Escalar;
            comand.CommandText = SQL;
            comand.CommandType = CommandType.Text;
            var a = comand.ExecuteScalar();//da un numero que es el resultado de una funcion
            var b = comand.ExecuteNonQuery();//da el numero filas afectadas
            var c = comand.ExecuteReader();//da la select
            if (c.Read())
            {

                if (c.Read())
                    resultat = ResultatSQL.ConjuntDeRegistres;
            }
            else if (b != -1)
                resultat = ResultatSQL.Nonquery;
            //poner una manera para saber que tipo de resultado da!
       
            return resultat;
        }

        public System.Collections.Generic.List<string[]> ConsultaStoredProcedure(string nomProcediment, List<OracleParameter> parametres)
        {
            OracleCommand comand = cnOracle.CreateCommand();
            comand.CommandType = CommandType.StoredProcedure;
            System.Collections.Generic.List<string[]> llistaTaula = null;
            string[] campos = null;
            foreach (OracleParameter paramatre in parametres)
                comand.Parameters.Add(paramatre);
            comand.CommandText = nomProcediment;
            try
            {

                OracleDataReader reader = comand.ExecuteReader();
                llistaTaula = new System.Collections.Generic.List<string[]>();
                campos = new string[reader.FieldCount];
                for (int i = 0; i < campos.Length; i++)
                    campos[i] = reader.GetName(i);
                if(campos.Length!=0)
                llistaTaula.Add(campos);
                foreach (System.Data.Common.DbDataRecord fila in reader)
                {
                    campos = new string[fila.FieldCount];
                    for (int i = 0; i < campos.Length; i++)
                        campos[i] = fila.GetValue(i).ToString();
                   
                    llistaTaula.Add(campos);
                }
                if (llistaTaula.Count == 0)
                {
                    comand.ExecuteNonQuery();
                    string parametre=null;
                    foreach(OracleParameter p in parametres)
                        if(p.Direction==ParameterDirection.Output)
                            parametre=p.ParameterName;
                    llistaTaula.Add(new string[] { comand.Parameters[parametre].Value.ToString() });
                }

                reader.Close();

            }
            catch { throw new Exception("El procediment no s'ha executat comprova que estigui ben escrit"); }
            return llistaTaula;
        }


        public override System.Collections.Generic.List<string[]> ConsultaStoredProcedure(string nomProcediment, List<Parametre> parametres)
        {
            List<OracleParameter> parametresOracle = new List<OracleParameter>();
            for (int i = 0; i < parametres.Count; i++)
            {
                OracleParameter oracleP = new OracleParameter();
                switch (parametres.ElementAt(i).direccio)
                {
                    case DireccioParametre.IN: oracleP.Direction = ParameterDirection.Input; break;
                    case DireccioParametre.INOUT: oracleP.Direction = ParameterDirection.InputOutput; break;
                    case DireccioParametre.OUT: oracleP.Direction = ParameterDirection.Output; break;
                }
                switch (parametres.ElementAt(i).tipus)
                {
                    case TipusParametres.Cursor: oracleP.OracleDbType = OracleDbType.RefCursor; break;
                    case TipusParametres.Integer: oracleP.OracleDbType = OracleDbType.Int32; break;
                    case TipusParametres.Decimal: oracleP.OracleDbType = OracleDbType.Decimal; break;
                }
                oracleP.ParameterName = parametres.ElementAt(i).nom;
                if(parametres.ElementAt(i).valor!=null)
                oracleP.Value = parametres.ElementAt(i).valor;
                parametresOracle.Add(oracleP);
            }
            return ConsultaStoredProcedure(nomProcediment, parametresOracle);
        }

		#region implemented abstract members of BaseDeDades

		public override bool ConsultaSiEsPot(string sql)
		{
			throw new NotImplementedException();
		}

		public override bool ConsultaSiExisteix(string sql)
		{
			throw new NotImplementedException();
		}

		public override string ConsultaUltimID()
		{
			throw new NotImplementedException();
		}

		public override bool EstaConectada {
			get {
				throw new NotImplementedException();
			}
		}

		#endregion
    }
}
