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
        OleDbConnection cnOleDb;
        Semaphore semafor;
        static string missatgeErrorPrimaryKeyDuplicated = "Los cambios solicitados en la tabla no se realizaron correctamente porque crearían valores duplicados en el índice, clave principal o relación. Cambie los datos en el campo o los campos que contienen datos duplicados, quite el índice o vuelva a definirlo para permitir entradas duplicadas e inténtelo de nuevo.";
        public static bool AutoCrearBDSiNoEsta = true;
        public BaseDeDadesAcces()
        {

            nomBaseDades = "Acces";
            connectionString = DonamStringConnection("BaseDeDadesAcces/bdDades.mdb");
            semafor = new Semaphore(1, 1);
        }

        public override bool Conecta()
        {
            semafor.WaitOne();
            bool conectat = true;
            try
            {
                cnOleDb = new OleDbConnection(connectionString);
                if (!File.Exists(cnOleDb.DataSource))
                {
                /*    if (AutoCrearBDSiNoEsta)
                    {
                        CreaBD(cnOleDb.DataSource, cnOleDb.Provider);
                        
                    }
                    else*/ //por arreglar!!
                        throw new Exception("No es pot conectar amb la base de dades per que no existix :" + cnOleDb.DataSource);

                }
                cnOleDb.Open();
              
            }
            catch { conectat = false; }
            semafor.Release();
            return conectat;



        }

		#region implemented abstract members of BaseDeDades


		public override TipusBaseDeDades TipusBD {
			get {
				return TipusBaseDeDades.Acces;
			}
		}


		#endregion

#region porArreglarArchivoBD
/*
        private void CreaBD(string path, string proveidorBD)
        {
            string fitxer = Path.GetFileName(path);
            text directori = Path.GetFullPath(path);
            directori.Replace(fitxer, "");
            CreaDirectori(directori);
            if (fitxer.Contains('.'))
                switch (fitxer.Split('.')[1])
                {
                    //mirar si el proveedorDB es el correcto si no lo es se tiene que cambiar...
                    case "mdb":
                        //UnZip(path, Estoc.Recursos.);
                        break;
                    case "accdb":
                        //saco el fixero de recursos
                        UnZip(path,Estoc.Recursos.bdd_accdb);
                        break;

                }
  
        }


        private static void UnZip(string path,byte[] recursZip)
        {
            var fsZip = new FileStream("temp.zip", FileMode.Create, FileAccess.Write);
            var bwZip = new BinaryWriter(fsZip);
            bwZip.Write(recursZip);
            bwZip.Close();
            fsZip.Close();
            var options = new Ionic.Zip.ReadOptions {StatusMessageWriter = System.Console.Out };
            using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read("temp.zip", options))
            {
                zip.FlattenFoldersOnExtract = false;
                zip.ExtractAll(path,Ionic.Zip.ExtractExistingFileAction.OverwriteSilently); 
            }
            //elimino el fixeroTemp
            File.Delete("temp.zip");
            string pathParent = new DirectoryInfo(path).Parent.FullName;
            new DirectoryInfo(path).MoveTo(pathParent + "\\temp");
            foreach (FileInfo file in new DirectoryInfo(pathParent + "\\temp").GetFiles())
            {

                file.MoveTo(pathParent+"\\"+Path.GetFileName(path));

            }
            Directory.Delete(pathParent + "\\temp");
        }

        private void CreaDirectori(string directori)
        {
            DirectoryInfo dir = new DirectoryInfo(directori);
            if (!Directory.Exists(dir.FullName))
                Directory.CreateDirectory(dir.FullName);
        }*/
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
            semafor.WaitOne();
            ComprovaConnexio();
            OleDbCommand comand = cnOleDb.CreateCommand();
            string[] campos = null;
            comand.CommandType = CommandType.TableDirect;
            comand.CommandText = nomTaula;
            System.Collections.Generic.List<string[]> llistaTaula = null;
            string[,] taula=new string[0,0];
            try
            {
                OleDbDataReader reader = comand.ExecuteReader();
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
            semafor.Release();

            return taula;
        }

        public override void ConsultaSQL(string SQL)
        {
            semafor.WaitOne();
            ComprovaConnexio();
            if (EstaConectada)
            {
                var comand = cnOleDb.CreateCommand();
                comand.CommandText = SQL;
                comand.CommandType = CommandType.Text;
                try
                {
                    comand.ExecuteReader();//da la select
                }
                catch (Exception m)
                {
                    if (m.Message.ToString() == missatgeErrorPrimaryKeyDuplicated&&((text)SQL).CountSubString("pdate")>0)
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
                Conecta(); if (!EstaConectada) throw new BDException("No es pot establir connexió amb la BD"); 
            }
        }



        public override System.Collections.Generic.List<string[]> ConsultaStoredProcedure(string nomProcediment, List<Parametre> parametres)
        {
            //no en te
            return null;
        }

        public override bool EstaConectada
        {
            get { return cnOleDb.State == ConnectionState.Open; }
        }

        public override bool ConsultaSiEsPot(string sql)
        {
            bool esPot = false;
            semafor.WaitOne();
            ComprovaConnexio();
            var comand = cnOleDb.CreateCommand();
            comand.CommandText = sql;
            comand.CommandType = CommandType.Text;
            try
            {
                esPot = comand.ExecuteReader().FieldCount != 0;//da un numero que es el resultado de una funcion
            }
            catch { }

            semafor.Release();
            return esPot;

        }

        public override bool ConsultaSiExisteix(string sql)
        {
            bool existeix = false;
            semafor.WaitOne();
            ComprovaConnexio();
            var comand = cnOleDb.CreateCommand();
            comand.CommandText = sql;
            comand.CommandType = CommandType.Text;
            try
            {

                var r= comand.ExecuteReader();
                existeix = r.HasRows;
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
            semafor.WaitOne();
            ComprovaConnexio();
            var comand = cnOleDb.CreateCommand();
            comand.CommandText = "Select @@Identity";
            comand.CommandType = CommandType.Text;
            try
            {
              
                    id = (int)comand.ExecuteScalar()+"";
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
