/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 22/01/2015
 * Hora: 15:11
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of ObjecteSqlIdAuto.
	/// </summary>
	public abstract class ObjecteSqlIdAuto:ObjecteSql
	{
		protected ObjecteSqlIdAuto(string taula,string primaryKey,string campPrimaryKey ): base(taula, primaryKey,campPrimaryKey)
		{}
	}
}
