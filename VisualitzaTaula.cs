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
    //hacer version para Wpf
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
            if (baseDades.EstaConectada)
            {
                nomTaulaActual = nomTaula;
                DataGridViewRow row;
                string[] camps;
                string[,] taulaDirectLlista = baseDades.ConsultaTableDirect(nomTaula);
                dgvDadesTaulaConsultada.Rows.Clear();
                dgvDadesTaulaConsultada.Columns.Clear();
                if (taulaDirectLlista != null)
                {
                    camps = taulaDirectLlista.Fila(0);//la primera fila son los nombres de las columnas

                    for (int i = 0; i < camps.Length; i++)
                        dgvDadesTaulaConsultada.Columns.Add(camps[i], camps[i]);
                    for (int j = 1; j < taulaDirectLlista.GetLength(DimensionMatriz.Fila); j++)
                    {//pongo los datos
                        camps = taulaDirectLlista.Fila(j);
                        row = new DataGridViewRow();
                        row.SetValues(camps.ToList<string>());
                        row.CreateCells(dgvDadesTaulaConsultada, camps);
                        dgvDadesTaulaConsultada.Rows.Add(row);

                    }

                }
                else
                    MessageBox.Show("El nom de la taula no es en  la base de dades " + baseDades);

            }
        }
    }
}

