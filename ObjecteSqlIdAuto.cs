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
	public abstract class ObjecteSqlIdAuto:ObjecteSql,IClauUnicaPerObjecte
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taula"></param>
        /// <param name="campPrimaryKey"></param>
        /// <param name="primaryKey">si no ha sido insertada tendrá el valor por defecto asta que se le asigne en el insert</param>
		protected ObjecteSqlIdAuto(string taula, string campPrimaryKey, string primaryKey="") : base(taula, primaryKey,campPrimaryKey)
		{}

        public IComparable Clau
        {
            get
            {
                return IdIntern;
            }
        }
    }
}
