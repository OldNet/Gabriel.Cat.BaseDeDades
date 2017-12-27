/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 27/12/2017
 * Licencia GNU v3
 */
using System;

namespace Gabriel.Cat.BaseDeDades
{

	public interface ISQLCustomInsert
	{
		string Insert(DataBase db);
	
	}
	public interface ISQLCustomDelete
	{
		string Delete(DataBase db);		
	}
	public interface ISQLCustomUpdate
	{
		string Update(DataBase db);
		
	}
	public interface ISQLCustomFullLoad
	{
		void FullLoad(DataBase db);
		
	}
		/// <summary>
	/// Si se necesita una forma más concreta para funcionar en la Base de datos(La seguridad tambien se trata aqui)
	/// </summary>
	public interface ISQLCustom:ISQLCustomInsert,ISQLCustomDelete,ISQLCustomUpdate,ISQLCustomFullLoad
	{
		
	}
}
