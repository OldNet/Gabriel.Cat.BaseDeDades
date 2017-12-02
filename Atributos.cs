/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 30/11/2017
 * Licencia GNU v3
 */
using System;

namespace Gabriel.Cat.BaseDeDades
{

	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Class)]
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
	[AttributeUsage(AttributeTargets.Property)]
	public class PrimaryKey:System.Attribute
	{}
	[AttributeUsage(AttributeTargets.Property)]
	public class SQLIgnore:System.Attribute
	{}

}
