/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 30/11/2017
 * Licencia GNU v3
 */
using System;
using System.Collections.Generic;

namespace Gabriel.Cat.BaseDeDades
{
	/// <summary>
	/// Description of IOptimitzeUpdate.
	/// </summary>
	public interface IOptimitzeUpdate
	{
		/// <summary>
		/// Obtiene la actualización para esta base de datos
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		IList<KeyValuePair<string,string>> GetUpdate(DataBase db);
		/// <summary>
		/// Al llamar este metodo la actualización para la base de datos pasado como parametro deja de estar pendiente
		/// </summary>
		/// <param name="db">Base de datos actualizada</param>
		void Updated(DataBase db);
		
	}
}
