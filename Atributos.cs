/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 30/11/2017
 * Licencia GNU v3
 */
using Gabriel.Cat.Extension;
using System;
using System.Linq;
using System.Reflection;

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
	/// <summary>
	/// Poner en los objetos SQL
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class OnDelete:System.Attribute
	{
		
		public enum DeleteOption
		{
			Cascade,SetNull,SetDefault
		}
		DeleteOption option;
		public OnDelete(DeleteOption option=DeleteOption.SetNull)
		{
			Option=option;
		}
		public DeleteOption Option {
			get {
				return option;
			}
			private set {
				option = value;
			}
		}
	}
    public abstract class ConstrainBase : System.Attribute
    {
        string name;
        public ConstrainBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// If is Null then NameTableNamePropertyConstrainType
        /// </summary>
        public string Name
        {
            get { return name; }
            private set { name = value; }
        }
    }
	[AttributeUsage(AttributeTargets.Property)]
	public class Constrain: ConstrainBase
    {
		public enum ConstrainType{
			/// <summary>
			/// La columna no puede tener valor NULL
			/// </summary>
			NotNull,
			/// <summary>
			/// Los valores tienen que ser unicos
			/// </summary>
			Unique,
			
			
			/// <summary>
			/// Si el valor no se especifica en la columna se pone el valor por defecto.
			/// </summary>
			Default,
            //Creo clase aparte
               //Index
              //Check,
        }

        ConstrainType constrain;
		public Constrain(ConstrainType constrain,string name=null):base(name)
		{
		
			ConstrainColumn=constrain;
		}

	

		
		public  ConstrainType ConstrainColumn{
			get {
				return constrain;
			}
			private set {
				constrain = value;
			}
		}
	}
	[AttributeUsage(AttributeTargets.Property,AllowMultiple=false)]
	public class ConstrainCheck:ConstrainBase
	{
		
		string constrain;
		public ConstrainCheck(string constrain,string name=null):base(name)
		{
		
			Constrain=constrain;
		}
	
		/// <summary>
		/// "ex: PropertyNumeric>=Number AND Property='Value'"
		/// </summary>
		public string Constrain {
			get {
				return constrain;
			}
			private set {
				constrain = value;
			}
		}
	}
		[AttributeUsage(AttributeTargets.Property)]
	public class ConstrainIndex:ConstrainBase
	{
	
		Type objectProperty;
		string property;
        string columnSql;
        /// <summary>
        /// exemple [ConstrainIndex(typeof(ConstrainCheck),"Constrain","constrainName")]
        /// </summary>
        /// <param name="objProperty"></param>
        /// <param name="property"></param>
        /// <param name="name"></param>
		public ConstrainIndex(Type objProperty,string property,string name=null):base(name)
		{
            string columnSql;
            PropiedadTipo propiedad=objProperty.GetPropiedades().Filtra((p) => p.Nombre == property)[0];
            SQLName nameSql;

          
			Property=property;

            nameSql = propiedad.Atributos.Filtra((a) => a is SQLName).FirstOrDefault() as SQLName;
            if (nameSql != null)
                columnSql = nameSql.Name;
            else columnSql = property;
          

        

        }

		public string Property {
			get {
				return property;
			}
			private set {
                property = value;
			}
		}

        public Type ObjectProperty {
            get { return objectProperty; }
            private set { objectProperty = value; }
        }
        public string ColumnSql
        {
            get {
                return columnSql;
            }
        }
    }
	
}
