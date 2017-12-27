/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 30/11/2017
Licencia GNU v3
 */
using System;

namespace Gabriel.Cat.BaseDeDades
{
	/// <summary>
	/// Se implementa para decir que se obtendrán los datos de forma postuma(cuando sea necesaria)
	/// </summary>
	public interface IDataBase:IComparable
	{
		string GetIdBD(DataBase db);
		void SetIdBD(DataBase db,string idBD);
		DateTime UltimaVersionBD(DataBase db);
	}
}
