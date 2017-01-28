using System.Data;
using System.Linq;
using System.Data.OleDb;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace Gabriel.Cat
{
    public class BaseDeDadesAcces : BaseDeDades
    {
        static readonly string missatgeErrorPrimaryKeyDuplicated = "Los cambios solicitados en la tabla no se realizaron correctamente porque crearían valores duplicados en el índice, clave principal o relación. Cambie los datos en el campo o los campos que contienen datos duplicados, quite el índice o vuelva a definirlo para permitir entradas duplicadas e inténtelo de nuevo.";

        OleDbConnection cnOleDb;
        Semaphore semafor;
        public BaseDeDadesAcces():this(DonamStringConnection("BaseDeDadesAcces/bdDades.mdb")){}
        public BaseDeDadesAcces(string connectionString)
        {

            nomBaseDades = "Acces";
            this.connectionString = connectionString;
            semafor = new Semaphore(1, 1);
        }

        public override bool Conecta()
        {
            bool conectat = true;
            semafor.WaitOne();
            
            try
            {
                cnOleDb = new OleDbConnection(connectionString);
                if (!File.Exists(cnOleDb.DataSource))
                {
                    throw new Exception("No es pot conectar amb la base de dades per que no existix :" + cnOleDb.DataSource);
                }
                cnOleDb.Open();

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
                return TipusBaseDeDades.Acces;
            }
        }


        #endregion

        public override void Desconecta()
        {
            semafor.WaitOne();
            cnOleDb.Close();
            semafor.Release();

        }
        public override bool PotFerStoredProcedure
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// dona la conexio string amb user Admin i sense password
        /// </summary>
        /// <param name="direccioBaseDades"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string direccioBaseDades)
        {
            return DonamStringConnection(direccioBaseDades, "");
        }
        /// <summary>
        /// dona la conexio string amb user Admin
        /// </summary>
        /// <param name="direccioBaseDades"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string direccioBaseDades, string password)
        {
            return DonamStringConnection(direccioBaseDades, "admin", password);
        }
        /// <summary>
        /// dona la conexio string
        /// </summary>
        /// <param name="direccioBaseDades"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DonamStringConnection(string direccioBaseDades, string user, string password)
        {
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + direccioBaseDades + ";User Id=" + user + ";Password=" + password + ";";
            //   return "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + direccioBaseDades + ";User Id=" + user + ";Password=" + password + ";";
        }


        
        public override string[,] ConsultaSQL(string sql)
        {//Camps,Files
            OleDbDataReader readerResult = GetResultadoSentenciaSql(sql);
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
                        comlumsAndRows[i, filaActual]=readerResult[i].ToString();
                    filaActual++;
                } while (readerResult.NextResult());
            }
            return comlumsAndRows;
        }
        public override string ConsultaSQLLinea(string sql)
        {
            return GetResultadoSentenciaSql(sql)?.ToString(); 
        }
        public  OleDbDataReader GetResultadoSentenciaSql(string sql)
        {
            OleDbCommand comand;
            OleDbDataReader resultado=null;

            semafor.WaitOne();
            ComprovaConnexio();
            if (EstaConectada)
            {
                comand = cnOleDb.CreateCommand();
                comand.CommandText = sql;
                comand.CommandType = CommandType.Text;
                try
                {
                    resultado = comand.ExecuteReader();

                }
                catch (Exception m)
                {
                    if (m.Message.ToString() == missatgeErrorPrimaryKeyDuplicated && sql.ToLower().Contains("pdate"))
                    {
                        semafor.Release();
                        throw new BDException("PrimaryKey is duplicated");
                    }
                }
            }
            else resultado = null;
            semafor.Release();
            return resultado;
        }
        private void ComprovaConnexio()
        {
            if (!EstaConectada)
            {
                Conecta();
                if (!EstaConectada)
                    throw new BDException("No es pot establir connexió amb la BD");
            }
        }



        public override string[,] ConsultaStoredProcedure(string nomProcediment, IList<Parametre> parametres)
        {
            //no en te
            return new string[0,0];
        }

        public override bool EstaConectada
        {
            get { return cnOleDb.State == ConnectionState.Open; }
        }


        public override string ConsultaUltimID()
        {
            return ConsultaSQLLinea("Select @@Identity");
        }
    }
}
