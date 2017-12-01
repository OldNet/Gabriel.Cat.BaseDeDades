/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 30/11/2017
 * Licencia GNU v3
 */
using System;

namespace Gabriel.Cat.BaseDeDades
{

	/// <summary>
	/// Tiene más importancia que una propiedad con el mismo nombre(El sql)
	/// </summary>
	public class SQLName:System.Attribute
	{
		string name;
		public SQLName(string name)
		{
			this.name=name;
		}

		public string Name {
			get {
				return name;
			}
		}
		public override string ToString()
		{
			return Name;
		}

	}
	public class PrimaryKey:System.Attribute
	{}
	public class SQLIgnore:System.Attribute
	{}
}
