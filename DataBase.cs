﻿/*
 * Created by SharpDevelop.
 * User: tetradog
 * Date: 30/11/2017
 * Licencia GNU V3
 */
using System;
using System.Collections.Generic;
using System.Text;
using Gabriel.Cat.Seguretat;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat.BaseDeDades
{
	/// <summary>
	/// Description of DataBase.
	/// </summary>
	public abstract class DataBase:IComparable,IComparable<DataBase>
	{
		
		Key keyGenerica;
		LlistaOrdenada<Type,Key> keyTabla;
		LlistaOrdenada<Type,LlistaOrdenada<string,Key>> keyCampoTabla;
		LlistaOrdenada<Type,string> dicNombreTabla;
		LlistaOrdenada<Type,string> dicCampoPrimaryKey;
		LlistaOrdenada<Type,LlistaOrdenada<IDataBase>> objetosBD;
		bool usarKeyGenerica;
		bool usarKeyTabla;
		
		string loginDataBase;
		public DataBase()
		{
			objetosBD=new LlistaOrdenada<Type, LlistaOrdenada<IDataBase>>();
			dicNombreTabla=new LlistaOrdenada<Type, string>();
			dicCampoPrimaryKey=new LlistaOrdenada<Type, string>();
			keyCampoTabla = new LlistaOrdenada<Type, LlistaOrdenada<string, Key>>();
			keyCampoTabla.Updated += ContraseñaCampoTablaActualizado;//mirar si incluye cuando se añade y se elimina
			keyTabla = new LlistaOrdenada<Type, Key>();
			keyTabla.Updated += ContraseñaTablaActualizado;//mirar si incluye cuando se añade y se elimina
		}
		/// <summary>
		/// Si es True cuando se cifre el campo se incluirá además el cifrado generico, solo se tendrá en cuenta si hay un cifrado para el campo,sino se usará si hay el cifrado generico asignado;un campo con el atributo NoCifrado no se tendrá en cuenta.
		/// </summary>
		public bool UsarKeyGenerica {
			get {
				return usarKeyGenerica;
			}
			set {
				usarKeyGenerica = value;
				CifradoTablas();
			}
		}

		/// <summary>
		/// Si es True cuando se cifre el campo se incluirá además el cifrado de la tabla, solo se tendrá en cuenta si hay para el campo en concreto, sino lo hay se cifrará siempre que haya cifrado para la tabla;un campo con el atributo NoCifrado no se tendrá en cuenta.
		/// </summary>
		public bool UsarKeyTablaSiHayDeConcreta {
			get {
				return usarKeyTabla;
			}
			set {
				usarKeyTabla = value;
				CifradoTablas();
			}
		}
		public Key KeyGenerica {
			get {
				return keyGenerica;
			}
			set {
				keyGenerica = value;
				ContraseñaGenericaActualizada();
			}
		}
		/// <summary>
		/// Clase,Key
		/// </summary>
		public LlistaOrdenada<Type, Key> KeyTabla {
			get {
				return keyTabla;
			}
		}

		/// <summary>
		/// Clase,NombreCampo,Key;un campo con el atributo NoCifrado no se tendrá en cuenta.
		/// </summary>
		public LlistaOrdenada<Type, LlistaOrdenada<string, Key>> KeyCampoTabla {
			get {
				return keyCampoTabla;
			}
		}

		public string LoginDataBase {
			get {
				return loginDataBase;
			}
			set {
				loginDataBase = value;
			}
		}
		public LlistaOrdenada<IDataBase> this[Type tabla]
		{
			get{
				LlistaOrdenada<IDataBase> objsTabla;
				if(!objetosBD.ContainsKey(tabla))
				{
					objsTabla=new LlistaOrdenada<IDataBase>();
					objsTabla.Added+=AñadirObjeto;
					objsTabla.Removed+=QuitarObjeto;
					objetosBD.Add(tabla,objsTabla);
					
				}
				else
					objsTabla=objetosBD[tabla];
				return objsTabla;
			}
		}

		void QuitarObjeto(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
		void AñadirObjeto(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		void ContraseñaCampoTablaActualizado(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		void ContraseñaTablaActualizado(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		void ContraseñaGenericaActualizada()
		{
			throw new NotImplementedException();
		}

		void CifradoTablas()
		{
			throw new NotImplementedException();
		}
		public  void Add(IDataBase obj)
		{
		}
		public void Add(IList<IDataBase> objs)
		{
			if (objs != null)
				for (int i = 0; i < objs.Count; i++)
					Add(objs[i]);
		}
		public void Remove(IDataBase obj)
		{

			Type tipo;
			StringBuilder strRemove;
			if(obj!=null){
				tipo=obj.GetType();
				strRemove=new StringBuilder();
				strRemove.Append("Delete from ");
				strRemove.Append(GetTableName(tipo));
				strRemove.Append(" where ");
				strRemove.Append(GetPrimaryKeyColumn(tipo));
				strRemove.Append("=");
				strRemove.Append(GetPrimaryKeyValue(obj));
				strRemove.Append(";");
				SQLRequest(strRemove.ToString());
			}
		}
		public void Remove(IList<IDataBase> objs)
		{
			if (objs != null)
				for (int i = 0; i < objs.Count; i++)
					Remove(objs[i]);
		}
		public void Update(IDataBase obj)
		{
			IOptimitzeUpdate objOptimo;
			IList<KeyValuePair<string,string>> update;
			StringBuilder strUpdate=new StringBuilder();
			Type tipo=obj.GetType();
			objOptimo=obj as IOptimitzeUpdate;
			
			if(objOptimo!=null)
				update=objOptimo.GetUpdate(this);
			else
			{
				update=new List<KeyValuePair<string,string>>();
				//cojo todos los campos
			}
			if(update.Count>0){
				strUpdate.Append("update ");
				strUpdate.Append(GetTableName(tipo));
				strUpdate.Append(" set ");
				
				for(int i=0;i<update.Count;i++)
				{
					//nombreColumna
					//valorColumna
					strUpdate.Append(GetColumnName(tipo,update[i].Key));
					strUpdate.Append(" = ");
					strUpdate.Append(SetColumnValue(tipo,update[i].Key,update[i].Value));
					strUpdate.Append(",");
				}
				strUpdate.Remove(strUpdate.Length-1,1);//quito la coma que sobra
				strUpdate.Append(" where ");
				strUpdate.Append(GetPrimaryKeyColumn(tipo));
				strUpdate.Append(" = ");
				strUpdate.Append(GetPrimaryKeyValue(obj));
				strUpdate.Append(";");
				
				SQLRequest(strUpdate.ToString());
				
				if(objOptimo!=null)
					objOptimo.Updated(this);
			}
		}

		string GetPrimaryKeyValue(IDataBase obj)
		{
			throw new NotImplementedException();
		}
		string GetColumnName(Type tipo, string nombrePropiedad)
		{
			throw new NotImplementedException();
		}
		string SetColumnValue(Type tipo, string nombrePropiedad, string valorPropiedad)
		{
			throw new NotImplementedException();
		}

		public void Update(IList<IDataBase> objs)
		{
			if (objs != null)
				for (int i = 0; i < objs.Count; i++)
					Update(objs[i]);
		}
		public LlistaOrdenada<IDataBase> Load(Type table, bool fullLoad = false)
		{
			const int PRIMEROBJETO=1;//en la fila 0 va el nombre de las columnas
			const int PRIMARYKETCOLUMN=0;

			string[,] tablaIdsObjetos;
			IDataBase objAct;
			StringBuilder strSelect=new StringBuilder();
			LlistaOrdenada<IDataBase> objetos;
			strSelect.Append("Select ");
			strSelect.Append(GetPrimaryKeyColumn(table));
			strSelect.Append(" from ");
			strSelect.Append(GetTableName(table));
			strSelect.Append(";");
			tablaIdsObjetos=SQLRequest(strSelect.ToString());
			//cargo los elementos con solo el id
			for(int y=PRIMEROBJETO,yFin=tablaIdsObjetos.GetLength(DimensionMatriz.Fila);y<yFin;y++)
			{
				objAct=(IDataBase)Activator.CreateInstance(table);
				objAct.IdBD=tablaIdsObjetos[PRIMARYKETCOLUMN,y];
				if(!objetos.ContainsKey(objAct))
					objetos.Add(objAct);
			}
			//si los tengo que acabar de cargar lo hago
			if (fullLoad)
				Load(objetos.GetValues());
			
			return this[table];
		}

		public string GetTableName(Type tabla)
		{
			if(!dicNombreTabla.ContainsKey(tabla))
			{
				//obtengo el nombre de la tabla
			}
			return dicNombreTabla[tabla];
		}

		public string GetPrimaryKeyColumn(Type tabla)
		{
			if(!dicCampoPrimaryKey.ContainsKey(tabla))
			{
				//obtengo el campoPrimaryKey de la tabla
			}
			return dicCampoPrimaryKey[tabla];
		}
		/// <summary>
		/// Acaba de cargar el objeto con los datos
		/// </summary>
		/// <param name="obj"></param>
		public void Load(IDataBase obj)
		{
			Type type;
			type = obj.GetType();
			ILoad(obj, GetTableName(type), GetPrimaryKeyColumn(type));
			
		}
		void ILoad(IDataBase obj, string tableName, string primaryKeyName)
		{
			const int NOMBRECOLUMNA=0;
			const int DATOSOBJ=1;
			StringBuilder strSelect;
			string[,] result;
			Type type=obj.GetType();
			strSelect = new StringBuilder();
			strSelect.Append("select * from ");
			strSelect.Append(tableName);
			strSelect.Append(" where ");
			strSelect.Append(primaryKeyName);
			strSelect.Append("=");
			strSelect.Append(GetPrimaryKeyValue(obj));
			strSelect.Append(";");
			
			result = SQLRequest(strSelect.ToString());
			//tengo todos los campos algunos estaran cifrados y otros no
			for(int i=0,f=result.GetLength(DimensionMatriz.Columna);i<f;i++)
			{
				obj.SetProperty(GetPropertyName(type,result[i,NOMBRECOLUMNA]),GetPropertyValue(type,result[i,NOMBRECOLUMNA],result[i,DATOSOBJ]));
			}
		}

		string GetPropertyName(Type type, string nombreColumna)
		{
			throw new NotImplementedException();
		}

		object GetPropertyValue(Type type, string nombreColumna, string valorColumna)
		{
			throw new NotImplementedException();
			//tiene que tener en cuenta si esta cifrada
		}
		/// <summary>
		/// Acaba de cargar el objeto con los datos
		/// </summary>
		/// <param name="objs"></param>
		public void Load(IList<IDataBase> objs)
		{
			Type tipoActual;
			Type tipoAnt;
			string nombreTabla;
			string nombrePrimaryKey;
			if (objs != null) {
				tipoAnt = objs[0].GetType();
				nombreTabla = GetTableName(tipoAnt);
				nombrePrimaryKey = GetPrimaryKeyColumn(tipoAnt);
				for (int i = 0; i < objs.Count; i++) {
					tipoActual = objs[i].GetType();
					if (!tipoActual.Equals(tipoAnt)) {
						nombreTabla = GetTableName(tipoActual);
						nombrePrimaryKey = GetPrimaryKeyColumn(tipoActual);
						tipoAnt=tipoActual;
					}
					ILoad(objs[i], nombreTabla, nombrePrimaryKey);
				}
			}
		}
		/// <summary>
		/// Elimina todos los elementos de la tabla
		/// </summary>
		/// <param name="table"></param>
		public void Delete(Type table)
		{
			SQLRequest("Delete * from "+GetTableName(table)+";");
		}
		/// <summary>
		/// Elimina la tabla
		/// </summary>
		/// <param name="table"></param>
		public void Drop(Type table)
		{
			SQLRequest("Drop table "+GetTableName(table)+";");
		}
		public bool Exist(Type table)
		{
			return SQLRequest("desc "+GetTableName(table)+";")[0,0]!=MensajeTablaNoExistente();
		}
		public abstract string[,] SQLRequest(string sql);
		
		protected abstract string MensajeTablaNoExistente();

		#region IComparable implementation

		public int CompareTo(object obj)
		{
			return CompareTo(obj as DataBase);
		}

		public int CompareTo(DataBase other)
		{
			int compareTo;
			if(other!=null)
				compareTo=string.Compare(LoginDataBase,other.LoginDataBase);
			else compareTo=(int)Gabriel.Cat.CompareTo.Inferior;
			return compareTo;
		}

		#endregion
	}
}
