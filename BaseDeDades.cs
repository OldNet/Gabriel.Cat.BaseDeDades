using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
    public enum ResultatSQL
    {
        Escalar,ConjuntDeRegistres,Nonquery
    }
 
    public enum DireccioParametre{IN,INOUT,OUT}
    public struct Parametre
    {
      public string nom;
      public string valor;
      public  TipusParametres tipus;
      public  DireccioParametre direccio;
    }
    public enum TipusParametres
    {
        Integer,Decimal,Varchar2,Varchar,Cursor
    }

    public abstract class BaseDeDades
    {
        protected BaseDeDades()
        {
            nomBaseDades = TipusBD.ToString();
        }
        protected string connectionString;
        protected string nomBaseDades;
        public abstract bool Conecta();

		public abstract TipusBaseDeDades TipusBD {
        	get;
		
		}

        public bool Conecta(string conexio)
        {
            connectionString = conexio;
            return Conecta();
        }
        public abstract bool EstaConectada
        {
            get;
        }
        public abstract void Desconecta();
        public override string ToString()
        {
            return nomBaseDades;
        }
        public virtual bool PotFerStoredProcedure
        { get { return true; } }
        public string NomBaseDeDades
        {
            get { return nomBaseDades; }
            set { if (!string.IsNullOrEmpty(value))
                     nomBaseDades = value; 
                 else 
                     throw new Exception("Es nessari un nom"); 
               }
        }
 
        public  string[,] ConsultaTableDirect(string nomTaula)
        {     
            return ConsultaSQL("select * from " + nomTaula + ";");
        }
        public abstract string[,] ConsultaSQL(string sql,bool addColumnsNames=true);

        public bool ExisteixTaula(string nomTaula)
        {
            bool existeix = false;
            try
            {
                ConsultaTableDirect(nomTaula);
                existeix = true;
            }catch
            {

            }
            return existeix;
        }
        public abstract string ConsultaUltimID();//com es una sentencia sql diferent a cada BD no puc fer-hi mes...

        /// <summary>
        /// si retorna una linea es el resultat d'un escalar si retorna més es el resultat d'un cursor llavors
        /// la primera linea serán els noms de les columnes i les altres les linees
        /// 
        /// </summary>
        /// <param name="nomProcediment"></param>
        /// 
        /// <param name="parametres"></param>
        /// <returns>si retorna null es que no ho pot fer</returns>
        public abstract string[,] ConsultaStoredProcedure(string nomProcediment,IList<Parametre> parametres,bool addColumnNames=true);
        public string[,] DescTable(string tableName)
        {
            return ConsultaSQL("desc " + tableName + ";");
        }
        /// <summary>
        /// Elimina la tabla
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName)
        {
            try
            {
                ConsultaSQL("drop table " + tableName + ";");
            }
            catch { }
        }
        /// <summary>
        /// Elimina todas las filas de la tabla
        /// </summary>
        /// <param name="tableName"></param>
        public void TruncateTable(string tableName)
        {
            try
            {
                ConsultaSQL("truncate table " + tableName + ";");
            }
            catch { }
        }
    }
 
    public class BDException:System.Exception
    {
    	public BDException(string mensaje):base(mensaje){}
    }
      public class SQLException:System.Exception
    {
    	public SQLException(string mensaje):base(mensaje){}
    } 
}
