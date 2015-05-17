using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Threading;

namespace Gabriel.Cat
{
   public class BaseDeDadesMySQL:BaseDeDades
    {
        MySqlConnection cnMySql;
        Semaphore semafor;
        static string missatgeErrorPrimaryKeyDuplicated = "for key 'PRIMARY'";
        public BaseDeDadesMySQL()
        {
            nomBaseDades = "MySQL";
            connectionString = "Database=test;Data Source=localhost;User Id=root;Password=";
            semafor = new Semaphore(1, 1);
        }
        public override bool Conecta()
        {
            bool conectat = true;
                semafor.WaitOne();
               
                try
                {
                    cnMySql = new MySqlConnection(connectionString);
                    cnMySql.Open();

                }
                catch { conectat = false; }
                finally{
                	semafor.Release();}
             return conectat;

        }

		#region implemented abstract members of BaseDeDades
		public override TipusBaseDeDades TipusBD {
			get {
				return TipusBaseDeDades.MySql;
			}
		}
		#endregion
        public override void Desconecta()
        {
            
                semafor.WaitOne();
                cnMySql.Close();
                semafor.Release();
            
        }
        /// <summary>
        /// et conecta a la base de dades indicada amb els valors per defecte
        /// </summary>
        /// <param name="nomBaseDades"></param>
        /// <returns></returns>
        public static string DonamStrinConnection(string nomBaseDades)
        {
            return DonamStrinConnection(nomBaseDades, "");
        }
        /// <summary>
        /// et conecta a la base de dades indicada amb els valors per defecte però amb la password canviada
        /// </summary>
        /// <param name="nomBaseDades"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStrinConnection(string nomBaseDades, string password)
        {
            return DonamStrinConnection(nomBaseDades, "root", password);
        }
        /// <summary>
        /// et conecta a la base de dades amb el host per defecte
        /// </summary>
        /// <param name="nomBaseDades"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStrinConnection(string nomBaseDades,string user, string password)
        {
            return DonamStrinConnection(nomBaseDades, "localhost", user, password);
        }
        /// <summary>
        /// retorna la string per fer la conexió
        /// </summary>
        /// <param name="nomBaseDades"></param>
        /// <param name="ipHost"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStrinConnection(string nomBaseDades, string ipHost, string user, string password)
        {
            return "Database="+nomBaseDades+";Data Source="+ipHost+";User Id="+user+";Password="+password;
        }

        public override string[,] ConsultaTableDirect(string nomTaula)
        {
            
            MiraSiEstaConnectat();
            semafor.WaitOne();
            MySqlCommand comand = cnMySql.CreateCommand();
            comand.CommandType = CommandType.TableDirect;
            comand.CommandText = nomTaula;
            string[] campos = null;
            System.Collections.Generic.List<string[]> llistaTaula = null;
            string[,] taula= new string[0,0];
            MySqlDataReader reader=null;
            try
            {
                reader = comand.ExecuteReader();
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
/// <summary>
/// Pone los valores en la matriz en su sitio
/// </summary>
                taula=new string[llistaTaula.Count,llistaTaula[0].Length];
                for(int y=0;y<llistaTaula.Count;y++)
                	for(int x=0;x<llistaTaula[0].Length;x++)
                		taula[y,x]=llistaTaula[y][x];
            }
            catch { }
            finally { semafor.Release();
                if(reader!=null) 
                    reader.Close(); }
            if (reader != null)
                if (!reader.IsClosed)
                    throw new BDException("");

            return taula;
        }

        private void MiraSiEstaConnectat()
        {
            if (!EstaConectada)
            {
            	Conecta(); if (!EstaConectada) {try{ semafor.Release();}catch{} throw new BDException("No es pot establir connexió amb la BD"); }
            }
        }

        public override void ConsultaSQL(string SQL)
        {
            semafor.WaitOne();
            MiraSiEstaConnectat();
            
            //var a=new MySqlCommand();
            var comand=cnMySql.CreateCommand();
            comand.CommandText = SQL;
            comand.CommandType = CommandType.Text;
            try
            {
                comand.ExecuteScalar();//da un numero que es el resultado de una funcion
            }
            catch (Exception m)
            {
                if (((text)m.Message.ToString()).CountSubString(missatgeErrorPrimaryKeyDuplicated)>0)
                {

                    throw new SQLException("PrimaryKey is duplicated");
                }
                else
                	throw m;
            }
            finally
            {
                semafor.Release();
            }

        
        }

        public override System.Collections.Generic.List<string[]> ConsultaStoredProcedure(string nomProcediment, List<Parametre> parametres)
        {
            List<MySqlParameter> parametresMySql = new List<MySqlParameter>();
            for (int i = 0; i < parametres.Count; i++)
            {
                MySqlParameter msqlP = new MySqlParameter();
                switch (parametres.ElementAt(i).direccio)
                {
                    case DireccioParametre.IN: msqlP.Direction = ParameterDirection.Input; break;
                    case DireccioParametre.INOUT: msqlP.Direction = ParameterDirection.InputOutput; break;
                    case DireccioParametre.OUT: msqlP.Direction = ParameterDirection.Output; break;
                }
                switch (parametres.ElementAt(i).tipus)
                {
                    case TipusParametres.Cursor: msqlP.MySqlDbType = MySqlDbType.DateTime; break;//s'han de acabar de posar pero no calen per l'exercici
                    case TipusParametres.Integer: msqlP.MySqlDbType = MySqlDbType.Int32; break;
                }
                msqlP.ParameterName = parametres.ElementAt(i).nom;
                if(parametres.ElementAt(i).direccio!=DireccioParametre.OUT)
                msqlP.Value = parametres.ElementAt(i).valor;
                parametresMySql.Add(msqlP);
            }
            return ConsultaStoredProcedure(nomProcediment, parametresMySql);
        }

        public System.Collections.Generic.List<string[]> ConsultaStoredProcedure(string nomProcediment, List<MySqlParameter> parametres)
        {
            MySqlCommand comand = cnMySql.CreateCommand();
            comand.CommandType = CommandType.StoredProcedure;
            System.Collections.Generic.List<string[]> llistaTaula = null;
            string[] campos = null;
            foreach (MySqlParameter paramatre in parametres)
                comand.Parameters.Add(paramatre);
            comand.CommandText = nomProcediment;
            MySqlDataReader reader = null;
            try
            {
                reader = comand.ExecuteReader();
                llistaTaula = new System.Collections.Generic.List<string[]>();
                campos = new string[reader.FieldCount];
                for (int i = 0; i < campos.Length; i++)
                    campos[i] = reader.GetName(i);
                llistaTaula.Add(campos);
                foreach (System.Data.Common.DbDataRecord fila in reader)
                {
                    campos = new string[fila.FieldCount];
                    for (int i = 0; i < campos.Length; i++)
                        campos[i] = fila.GetValue(i).ToString();
                    llistaTaula.Add(campos);
                }

                reader.Close();

            }
            catch { if (reader != null) reader.Close(); throw new SQLException("El procediment no s'ha executat comprova que estigui ben escrit"); }

            return llistaTaula;
        }

        public override bool EstaConectada
        {
            get { return cnMySql.State.Equals(ConnectionState.Open); }
        }

        public override bool ConsultaSiEsPot(string sql)//mirar si lo hace bien
        {
            bool esPot=false;
            semafor.WaitOne();
            MiraSiEstaConnectat();
            var comand = cnMySql.CreateCommand();
            comand.CommandText = sql;
            comand.CommandType = CommandType.Text;
            try
            {
                esPot = comand.ExecuteReader().FieldCount != 0;//da un numero que es el resultado de una funcion
            }
            catch { }
            finally
            {
                semafor.Release();
            }
            return esPot;
          
        }
        public override bool ConsultaSiExisteix(string sql)
        {
            bool existeix = false;
            semafor.WaitOne();
            MiraSiEstaConnectat();
            var comand = cnMySql.CreateCommand();
            comand.CommandText = sql;
            comand.CommandType = CommandType.Text;
            try
            {
                existeix = comand.ExecuteReader().FieldCount == 1;//da un numero que es el resultado de una funcion
            }
            catch { }
            finally
            {
                semafor.Release();
            }
            return existeix;
        }

        public override string ConsultaUltimID()
        {
            string id=null;
            semafor.WaitOne();
            MiraSiEstaConnectat();
            var comand = cnMySql.CreateCommand();
            comand.CommandText = "select last_insert_id();";
            comand.CommandType = CommandType.Text;
            try
            {
          
                    id = comand.ExecuteScalar().ToString();
            }
            catch { }
            finally
            {
                semafor.Release();
            }
            return id;
        }
    }
}
