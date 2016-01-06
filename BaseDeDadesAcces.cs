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

        public BaseDeDadesAcces()
        {

            nomBaseDades = "Acces";
            connectionString = DonamStringConnection("BaseDeDadesAcces/bdDades.mdb");
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


        public override string[,] ConsultaTableDirect(string nomTaula)
        {
            OleDbDataReader reader;
            OleDbCommand comand;
            string[] campos;
            semafor.WaitOne();
            ComprovaConnexio();
            comand = cnOleDb.CreateCommand();
            campos = null;
            comand.CommandType = CommandType.TableDirect;
            comand.CommandText = nomTaula;
            List<string[]> llistaTaula = null;
            string[,] taula = new string[0, 0];
            try
            {
                reader = comand.ExecuteReader();
                llistaTaula = new List<string[]>();
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
                taula = new string[llistaTaula.Count, llistaTaula[0].Length];
                for (int y = 0; y < llistaTaula.Count; y++)
                    for (int x = 0; x < llistaTaula[0].Length; x++)
                        taula[y, x] = llistaTaula[y][x];
            }
            catch { }
            finally
            {
                semafor.Release();
            }
            return taula;
        }

        public override void ConsultaSQL(string sql)
        {
            OleDbCommand comand;
            semafor.WaitOne();
            ComprovaConnexio();
            if (EstaConectada)
            {
                comand = cnOleDb.CreateCommand();
                comand.CommandText = sql;
                comand.CommandType = CommandType.Text;
                try
                {
                    comand.ExecuteReader();//da la select
                }
                catch (Exception m)
                {
                    if (m.Message.ToString() == missatgeErrorPrimaryKeyDuplicated && sql.Contains("pdate"))
                    {
                        semafor.Release();
                        throw new BDException("PrimaryKey is duplicated");
                    }
                }
            }
            semafor.Release();

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



        public override string[,] ConsultaStoredProcedure(string nomProcediment, IEnumerable<Parametre> parametres)
        {
            //no en te
            return new string[0,0];
        }

        public override bool EstaConectada
        {
            get { return cnOleDb.State == ConnectionState.Open; }
        }

        public override bool ConsultaSiEsPot(string sql)
        {
            bool esPot = false;
            OleDbCommand comand;
            semafor.WaitOne();
            ComprovaConnexio();
            comand = cnOleDb.CreateCommand();
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
            OleDbCommand comand;
            OleDbDataReader tablaConsulta;
            semafor.WaitOne();
            ComprovaConnexio();
            comand = cnOleDb.CreateCommand();
            comand.CommandText = sql;
            comand.CommandType = CommandType.Text;
            try
            {
                tablaConsulta = comand.ExecuteReader();
                existeix = tablaConsulta.HasRows;
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

            string id = null;
            OleDbCommand comand;
            semafor.WaitOne();
            ComprovaConnexio();
            comand = cnOleDb.CreateCommand();
            comand.CommandText = "Select @@Identity";
            comand.CommandType = CommandType.Text;
            try
            {

                id = (int)comand.ExecuteScalar() + "";
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
