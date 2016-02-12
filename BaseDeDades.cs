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
            set { if (value != "")
                     nomBaseDades = value; 
                 else 
                     throw new Exception("Es nessari un nom"); 
               }
        }
 
        public abstract string[,] ConsultaTableDirect(string nomTaula);
        public abstract string ConsultaSQL(string sql);
        public abstract bool ConsultaSiEsPot(string sql);
        public abstract bool CompruebaSiFunciona(string sql);
        public bool ExisteixTaula(string nomTaula)
        {
        	return ConsultaTableDirect(nomTaula).GetLength(DimensionMatriz.Fila)!=0;
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
        public abstract string[,] ConsultaStoredProcedure(string nomProcediment,IEnumerable<Parametre> parametres);
        public string DescTable(string tableName)
        {
            return ConsultaSQL("desc " + tableName + ";");
        }
        public void DropTable(string tableName)
        {
            try
            {
                ConsultaSQL("drop " + tableName + ";");
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
