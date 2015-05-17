/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 28/04/2015
 * Hora: 16:54
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Convertir.
	/// </summary>
	public static class Convertir
	{
		public static DataSet ExcelToDataSet(string pathExcel)
		{
			DataSet result = null;
			if (pathExcel.EndsWith(".xlsx")) {
				// Reading from a binary Excel file (format; *.xlsx)
				FileStream stream = File.Open(pathExcel, FileMode.Open, FileAccess.Read);
				Excel.IExcelDataReader excelReader = Excel.ExcelReaderFactory.CreateOpenXmlReader(stream);
				result = excelReader.AsDataSet(); 
				excelReader.Close(); 
			} else if (pathExcel.EndsWith(".xls")) {
				// Reading from a binary Excel file ('97-2003 format; *.xls)
				FileStream stream = File.Open(pathExcel, FileMode.Open, FileAccess.Read);
				Excel.IExcelDataReader excelReader = Excel.ExcelReaderFactory.CreateBinaryReader(stream);
				result = excelReader.AsDataSet();
				excelReader.Close();
			} else
				throw new Exception("El formato del archivo no es Excel!");
			return result;
		
		}
		public static IEnumerable<string>[] ExcelToCsv(string pathExcel, char separacion)
		{
			return ExcelToDataSet(pathExcel).ToCsv(separacion);
		}
		/// <summary>
		/// La separacion es ';'
		/// </summary>
		/// <param name="pathExcel"></param>
		/// <returns></returns>
		public static IEnumerable<string>[] ExcelToCsv(string pathExcel)
		{
			return ExcelToCsv(pathExcel, ';');
		}
		/// <summary>
		/// La separacion es ';'
		/// </summary>
		/// <param name="excelDataSet"></param>
		/// <returns></returns>
		public static IEnumerable<string>[] ToCsv(this DataSet excelDataSet)
		{
			return ToCsv(excelDataSet, ';');
		}
		public static IEnumerable<string>[] ToCsv(this DataSet excelDataSet, char separador)
		{
			Llista<IEnumerable<string>> taulesArxius = new Llista<IEnumerable<string>>();
			foreach (DataTable taula in excelDataSet.Tables) {
				taulesArxius.Afegir(taula.ToMatriu().ToCsv(separador));
				
			}
				
			return taulesArxius.ToTaula();
			
		}
		#region Extension
		public static string[,] ToMatriu(this DataTable taula)
		{
			string[,] matriu = new string[taula.Columns.Count, taula.Rows.Count];
			for (int y = 0; y < matriu.GetLength(DimensionMatriz.Y); y++)
				for (int x = 0; x < matriu.GetLength(DimensionMatriz.X); x++)
					matriu[x, y] = taula.Rows[y][x].ToString();
			return matriu;
					
			
		}
		public static IEnumerable<string> ToCsv(this string[,] datos, char separador)
		{
			Llista<string> llista = new Llista<string>();
			string fila = "";
			for (int y = 0; y < datos.GetLength(DimensionMatriz.Y); y++) {
				fila = datos[0, y];
				for (int x = 1; x < datos.GetLength(DimensionMatriz.X); x++) {
					fila += separador + datos[x, y];
				}
				llista.Afegir(fila);
			}
			return llista;
				
		}
		#endregion
		
	}
}
