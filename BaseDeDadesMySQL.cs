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
            return DonamStringConnection(nomBaseDades, "localhost", user, password);
        }
        /// <summary>
        /// retorna la string per fer la conexió
        /// </summary>
        /// <param name="nomBaseDades"></param>
        /// <param name="ipHost"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string nomBaseDades, string ipHost, string user, string password)
        {
            return "Database=" + nomBaseDades + ";Data Source=" + ipHost + ";User Id=" + user + ";Password=" + password;
        }

        private void MiraSiEstaConnectat()
        {
            if (!EstaConectada)
            {
                Conecta(); if (!EstaConectada) { try { semafor.Release(); } catch { } throw new BDException("No es pot establir connexió amb la BD"); }
            }
        }

        public override string[,] ConsultaSQL(string sql)
        {//Camps,Files
            MySqlDataReader readerResult = GetResultadoSentenciaSql(sql);
            string[,] comlumsAndRows;
            int filaActual;
            if (readerResult == null)
                comlumsAndRows = new string[0, 0];
            else
            {
                filaActual = 0;
                comlumsAndRows = new string[readerResult.FieldCount, readerResult.RecordsAffected];//mirar si se obtiene las filas con RecordsAffected
                do
                {
                    for (int i = 0; i < readerResult.FieldCount; i++)
                        comlumsAndRows[i, filaActual] = readerResult[i].ToString();
                    filaActual++;
                } while (readerResult.NextResult());
            }
            return comlumsAndRows;
        }
        public override string ConsultaSQLLinea(string sql)
        {
            return GetResultadoSentenciaSql(sql)?.ToString();
        }
        public MySqlDataReader GetResultadoSentenciaSql(string sql)
        {

            MySqlCommand comand;
            MySqlDataReader reader;
            semafor.WaitOne();
            MiraSiEstaConnectat();
            comand = cnMySql.CreateCommand();
            comand.CommandText = sql;
            comand.CommandType = CommandType.Text;
            try
            {
                reader = comand.ExecuteReader();
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
            return reader;

        }

        public override string[,] ConsultaStoredProcedure(string nomProcediment, IList<Parametre> parametres)
        {
            MySqlParameter msqlP;
            List<MySqlParameter> parametresMySql = new List<MySqlParameter>();

            for (int i = 0; i < parametres.Count; i++)
            {
                msqlP = new MySqlParameter();
                switch (parametres[i].direccio)
                {
                    case DireccioParametre.IN: msqlP.Direction = ParameterDirection.Input; break;
                    case DireccioParametre.INOUT: msqlP.Direction = ParameterDirection.InputOutput; break;
                    case DireccioParametre.OUT: msqlP.Direction = ParameterDirection.Output; break;
                }
                switch (parametres[i].tipus)
                {
                    case TipusParametres.Cursor: msqlP.MySqlDbType = MySqlDbType.DateTime; break;//s'han de acabar de posar pero no calen per l'exercici
                    case TipusParametres.Integer: msqlP.MySqlDbType = MySqlDbType.Int32; break;
                }
                msqlP.ParameterName = parametres[i].nom;
                if (parametres[i].direccio != DireccioParametre.OUT)
                    msqlP.Value = parametres[i].valor;
                parametresMySql.Add(msqlP);
            }
            return ConsultaStoredProcedure(nomProcediment, parametresMySql);
        }

        public string[,] ConsultaStoredProcedure(string nomProcediment, IList<MySqlParameter> parametres)
        {
            MySqlDataReader reader = null;
            MySqlCommand comand = cnMySql.CreateCommand();
            string[] campos = null;
            List<string[]> llistaTaula = null;
            string[,] taulaProcediment=null;
            semafor.WaitOne();
            comand.CommandType = CommandType.StoredProcedure;
            for (int i=0;i<parametres.Count;i++)
            {
                comand.Parameters.Add(parametres[i]);
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

        public override string ConsultaUltimID()
         {
             return ConsultaSQLLinea("select last_insert_id();");
         }
        
    }
}
