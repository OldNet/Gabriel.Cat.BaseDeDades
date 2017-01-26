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
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{
    public class BaseDeDadesMySQL : BaseDeDades
    {
        static readonly string missatgeErrorPrimaryKeyDuplicated = "for key 'PRIMARY'";

        MySqlConnection cnMySql;
        Semaphore semafor;

        public BaseDeDadesMySQL(string connectionString= "Database=test;Data Source=localhost;User Id=root;Password=")
        {
            nomBaseDades = "MySQL";
            this.connectionString = connectionString;
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
            finally
            {
                semafor.Release();
            }
            return conectat;

        }

        #region implemented abstract members of BaseDeDades
        public override TipusBaseDeDades TipusBD
        {
            get
            {
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
        public static string DonamStrinConnection(string nomBaseDades, string user, string password)
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
            return "Database=" + nomBaseDades + ";Data Source=" + ipHost + ";User Id=" + user + ";Password=" + password;
        }

        public override string[,] ConsultaTableDirect(string nomTaula)
        {
            MySqlCommand comand;
            MySqlDataReader reader = null;
            MiraSiEstaConnectat();
            semafor.WaitOne();
            comand = cnMySql.CreateCommand();
            comand.CommandType = CommandType.TableDirect;
            comand.CommandText = nomTaula;
            string[] campos = null;
            System.Collections.Generic.List<string[]> llistaTaula = null;
            string[,] taula = new string[0, 0];

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
                taula = new string[llistaTaula.Count, llistaTaula[0].Length];
                for (int y = 0; y < llistaTaula.Count; y++)
                    for (int x = 0; x < llistaTaula[0].Length; x++)
                        taula[y, x] = llistaTaula[y][x];
            }
            catch { }
            finally
            {
                semafor.Release();
                if (reader != null)
                    reader.Close();
            }
            if (reader != null)
                if (!reader.IsClosed)
                    throw new BDException("");

            return taula;
        }

        private void MiraSiEstaConnectat()
        {
            if (!EstaConectada)
            {
                Conecta(); if (!EstaConectada) { try { semafor.Release(); } catch { } throw new BDException("No es pot establir connexió amb la BD"); }
            }
        }

        public override string ConsultaSQL(string SQL)
        {
            MySqlCommand comand;
            string resultado = null;
            semafor.WaitOne();
            MiraSiEstaConnectat();
            comand = cnMySql.CreateCommand();
            comand.CommandText = SQL;
            comand.CommandType = CommandType.Text;
            try
            {
                resultado = comand.ExecuteScalar().ToString();//da un numero que es el resultado de una funcion
            }
            catch (Exception m)
            {
                if (m.Message.Contains(missatgeErrorPrimaryKeyDuplicated))
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
            return resultado;

        }

        public override string[,] ConsultaStoredProcedure(string nomProcediment, IEnumerable<Parametre> parametres)
        {
            MySqlParameter msqlP;
            List<MySqlParameter> parametresMySql = new List<MySqlParameter>();
            List<Parametre> parametresList = new List<Parametre>(parametres);

            for (int i = 0; i < parametresList.Count; i++)
            {
                msqlP = new MySqlParameter();
                switch (parametresList.ElementAt(i).direccio)
                {
                    case DireccioParametre.IN: msqlP.Direction = ParameterDirection.Input; break;
                    case DireccioParametre.INOUT: msqlP.Direction = ParameterDirection.InputOutput; break;
                    case DireccioParametre.OUT: msqlP.Direction = ParameterDirection.Output; break;
                }
                switch (parametresList.ElementAt(i).tipus)
                {
                    case TipusParametres.Cursor: msqlP.MySqlDbType = MySqlDbType.DateTime; break;//s'han de acabar de posar pero no calen per l'exercici
                    case TipusParametres.Integer: msqlP.MySqlDbType = MySqlDbType.Int32; break;
                }
                msqlP.ParameterName = parametresList.ElementAt(i).nom;
                if (parametresList.ElementAt(i).direccio != DireccioParametre.OUT)
                    msqlP.Value = parametresList.ElementAt(i).valor;
                parametresMySql.Add(msqlP);
            }
            return ConsultaStoredProcedure(nomProcediment, parametresMySql);
        }

        public string[,] ConsultaStoredProcedure(string nomProcediment, IEnumerable<MySqlParameter> parametres)
        {
            MySqlDataReader reader = null;
            MySqlCommand comand = cnMySql.CreateCommand();
            string[] campos = null;
            List<string[]> llistaTaula = null;
            string[,] taulaProcediment=null;
            semafor.WaitOne();
            comand.CommandType = CommandType.StoredProcedure;
            foreach (MySqlParameter paramatre in parametres)
            {
                comand.Parameters.Add(paramatre);
            }
            comand.CommandText = nomProcediment;
            try
            {
                reader = comand.ExecuteReader();
                llistaTaula = new List<string[]>();
                campos = new string[reader.FieldCount];
                for (int i = 0; i < campos.Length; i++)
                {
                    campos[i] = reader.GetName(i);
                }
                llistaTaula.Add(campos);
                foreach (System.Data.Common.DbDataRecord fila in reader)
                {
                    campos = new string[fila.FieldCount];
                    for (int i = 0; i < campos.Length; i++)
                    {
                        campos[i] = fila.GetValue(i).ToString();
                    }
                    llistaTaula.Add(campos);
                }
                taulaProcediment = llistaTaula.ToMatriu();
            }
            catch
            {
                throw new SQLException("El procediment no s'ha executat comprova que estigui ben escrit");
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                semafor.Release();
            }

            return taulaProcediment;
        }

        public override bool EstaConectada
        {
            get { return cnMySql.State.Equals(ConnectionState.Open); }
        }

        public override bool ConsultaSiEsPot(string sql)//mirar si lo hace bien
        {
            bool esPot = false;
            MySqlCommand comand;
            semafor.WaitOne();
            MiraSiEstaConnectat();
            comand = cnMySql.CreateCommand();
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
        public override bool CompruebaSiFunciona(string sql)
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
             return ConsultaSQL("select last_insert_id();");
         }
        
    }
}
