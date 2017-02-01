using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
    //para verlo en wpf se usa
    /*
     *1.       añadir la dll de windows forms :D
      2.       xmlns:wfBd="clr-namespace:Gabriel.Cat;assembly=Gabriel.Cat.BaseDeDades"
      3.
               <WindowsFormsHost >
                     <wfBd:VisualitzaTaula x:Name="vtTaulaAVeure"/>
               </WindowsFormsHost>
         */
    public partial class VisualitzaTaula : UserControl
    {
        BaseDeDades baseDades;
        string nomTaulaActual;

        public VisualitzaTaula()
        {
            InitializeComponent();
        }
        public BaseDeDades BaseDades
        {
            get { return baseDades; }
            set { baseDades = value; ReCarregaTaula();}
        }
        public void ReCarregaTaula()
        {
            if (nomTaulaActual != null)
                Visualitza(nomTaulaActual);
        }
        public void Visualitza(string nomTaula)
        {
            nomTaulaActual = nomTaula;


            try
            {
                VisualitzaSqlResult("select * from " + nomTaula + ";");
            }
            catch
            {

                MessageBox.Show("El nom de la taula no es en  la base de dades " + baseDades);
            }
            
        }

        public void VisualitzaSqlResult(string sql)
        {

            VisualitzaTaulaResult(baseDades.ConsultaSQL(sql));
              
        }

        public void VisualitzaTaulaResult(string[,] tabla)
        {
            DataGridViewRow row;
            string[] camps;


            if (baseDades.EstaConectada)
            {

                dgvDadesTaulaConsultada.Rows.Clear();
                dgvDadesTaulaConsultada.Columns.Clear();
                if (tabla != null)
                {
                    camps = tabla.Fila(0);//la primera fila son los nombres de las columnas

                    for (int i = 0; i < camps.Length; i++)
                        dgvDadesTaulaConsultada.Columns.Add(camps[i], camps[i]);
                    for (int j = 1; j < tabla.GetLength(DimensionMatriz.Fila); j++)
                    {//pongo los datos
                        camps = tabla.Fila(j);
                        row = new DataGridViewRow();
                        row.SetValues(camps.ToList<string>());
                        row.CreateCells(dgvDadesTaulaConsultada, camps);
                        dgvDadesTaulaConsultada.Rows.Add(row);

                    }

                }
                else throw new Exception("la sentencia sql no devuelve filas o es incorrecta");
            }
        }
    }
}

