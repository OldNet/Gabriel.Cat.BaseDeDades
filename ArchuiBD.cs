using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
  public  class ArchuiBD
    {
        string nomArchiuAmbExtensió;
        byte[] dades;
        const char CARACTERSEPARACIONOMYDADES=',';
        const char CARACTERSERPARADORARCHIUSBD = ';';

        public ArchuiBD(string nomArchiuAmbExtensió, byte[] dades)
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
            return NomArchiuAmbExtensió + CARACTERSEPARACIONOMYDADES + Dades.ToHex();
        }
        public static string ToAllStringBD(IEnumerable<ArchuiBD> archius)
        {
            string stringAll = "";
            if (archius != null)
            {
                foreach (ArchuiBD archiu in archius)
                    stringAll += CARACTERSERPARADORARCHIUSBD + archiu.ToStringBD();
                stringAll = stringAll.Substring(1, stringAll.Length - 1);
            }
            return stringAll;
        }
        public static ArchuiBD[] StringToArchiusBD(string stringAll)
        {
            List<ArchuiBD> archius = new List<ArchuiBD>();
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
                    archius.Add(new ArchuiBD(camps[0], camps[1].HexStringToByteArray()));
                }
            }
            return archius.ToArray();
        }
    }
}
