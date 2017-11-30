/*
 * Created by SharpDevelop.
 * User: tetradog
 * Date: 30/11/2017
 * Licencia GNU V3
 */
using System;
using System.Collections.Generic;
using System.Text;
using Gabriel.Cat.Seguretat;
namespace Gabriel.Cat.BaseDeDades
{
	/// <summary>
	/// Description of DataBase.
	/// </summary>
	public abstract class DataBase
	{
		
		Key keyGenerica;
		LlistaOrdenada<Type,Key> keyTabla;
		LlistaOrdenada<Type,LlistaOrdenada<string,Key>> keyCampoTabla;
		bool usarKeyGenerica;
		bool usarKeyTabla;
		
		string loginDataBase;
		public DataBase()
		{
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
				strRemove.Append(obj.IdBD);
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
		}
		public void Update(IList<IDataBase> objs)
		{
			if (objs != null)
				for (int i = 0; i < objs.Count; i++)
					Update(objs[i]);
		}
		public IList<IDataBase> Get(Type table, bool fullLoad = false)
		{
			List<IDataBase> lstTable = new List<IDataBase>();
			//cargo los elementos con solo el id
			//si los tengo que acabar de cargar lo hago
			if (fullLoad)
				Load(lstTable);
			return lstTable;
		}

		public string GetTableName(Type obj)
		{
			throw new NotImplementedException();
		}

		public string GetPrimaryKeyColumn(Type type)
		{
			throw new NotImplementedException();
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

			StringBuilder strSelect;
			string[,] result;

			strSelect = new StringBuilder();
			strSelect.Append("select * from ");
			strSelect.Append(tableName);
			strSelect.Append(" where ");
			strSelect.Append(primaryKeyName);
			strSelect.Append("=");
			strSelect.Append(obj.IdBD);
			strSelect.Append(";");
			
			result = SQLRequest(strSelect.ToString());
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
	}
}
