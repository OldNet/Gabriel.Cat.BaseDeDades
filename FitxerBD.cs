using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
  public  class FitxerBD
    {
        string nomArchiuAmbExtensió;
        byte[] dades;
        const char CARACTERSEPARACIONOMYDADES=',';
        const char CARACTERSERPARADORARCHIUSBD = ';';

        public FitxerBD(string nomArchiuAmbExtensió, byte[] dades)
        {
            this.nomArchiuAmbExtensió = nomArchiuAmbExtensió;
            this.dades = dades;
        }

        public string NomArchiuAmbExtensió
        {
            get
            {
                return nomArchiuAmbExtensió;
            }

            set
            {
                if (nomArchiuAmbExtensió.Contains(new char[] { CARACTERSEPARACIONOMYDADES, CARACTERSERPARADORARCHIUSBD }))
                    throw new ArgumentException(String.Format("No pot contenir els caracters '{0}' i/o '{1}'", CARACTERSEPARACIONOMYDADES, CARACTERSERPARADORARCHIUSBD));
                nomArchiuAmbExtensió = value;
            }
        }

        public byte[] Dades
        {
            get
            {
                return dades;
            }

            set
            {
                dades = value;
            }
        }
        public string ToStringBD()
        {
            return NomArchiuAmbExtensió + CARACTERSEPARACIONOMYDADES + (Hex)Dades;
        }
        public static string ToAllStringBD(IEnumerable<FitxerBD> fitxers)
        {
            StringBuilder stringAll = new StringBuilder();
            if (fitxers != null)
            {
                foreach (FitxerBD archiu in fitxers)
                    stringAll.Append(archiu.ToStringBD()+ CARACTERSERPARADORARCHIUSBD);
                stringAll.Remove(1, stringAll.Length - 1);
            }
            return stringAll.ToString();
        }
        public static FitxerBD[] StringToArchiusBD(string stringAll)
        {
            List<FitxerBD> archius = new List<FitxerBD>();
            string[] archiusString;
            string[] camps;
            if (!String.IsNullOrEmpty(stringAll) && stringAll.Contains(CARACTERSEPARACIONOMYDADES))
            {
                if (stringAll.Contains(CARACTERSERPARADORARCHIUSBD))
                    archiusString = stringAll.Split(CARACTERSERPARADORARCHIUSBD);
                else archiusString = new string[] { stringAll };//si nomes hi ha un archiu
                for (int i = 0; i < archiusString.Length; i++)
                {
                    camps = archiusString[i].Split(CARACTERSEPARACIONOMYDADES);
                    archius.Add(new FitxerBD(camps[0], (Hex)camps[1]));
                }
            }
            return archius.ToArray();
        }
    }
}
