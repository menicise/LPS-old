using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace LPS.ToolScript.Parser
{
	public class DBTableExpression : Dictionary<string, IDBColumn>, IDBTable
	{
		public string Name { get; private set; }

		/// <summary>
		/// If table should NOT be created
		/// </summary>
		public bool IsTemplate { get; private set; }

		/// <summary>
		/// If existing table with Name should be updated
		/// </summary>
		public bool IsExtension { get; private set; }
		public string TemplateName { get; private set; }
		public DBTableIndices Indices { get; private set; }
		public IDBColumnPrimary PrimaryKey { get; private set; }

		public EvaluatedAttributeList Attribs { get; private set; }
		public DBTableTriggers Triggers { get; private set; }

		public DBTableExpression(string Name, bool IsTemplate, bool IsExtension, string TemplateName)
		{
			this.Name = Name;
			this.IsTemplate = IsTemplate;
			this.IsExtension = IsExtension;
			this.Attribs = new EvaluatedAttributeList();
			this.Indices = new DBTableIndices();
			this.TemplateName = TemplateName;
			this.Triggers = new DBTableTriggers();
		}

		public void AddAttrib(object attrib)
		{
			if(attrib is KeyValuePair<string,EvaluatedAttribute>)
			{
				KeyValuePair<string,EvaluatedAttribute> kv = (KeyValuePair<string,EvaluatedAttribute>)attrib;
				Attribs.Add(kv.Key, kv.Value);
            }
			else if (attrib is IDBTableTrigger)
			{
				this.Triggers.Add((IDBTableTrigger)attrib);
			}
			else if (attrib is DBTableIndex)
			{
				this.Indices.Add((DBTableIndex)attrib);
			}
			else
				throw new NotImplementedException();
		}

		public void AddAttribs(IEnumerable collection)
		{
			foreach(object o in collection)
				AddAttrib(o);
		}

		public void Run (Context context)
		{
			Eval(context);
		}

		public object Eval (Context context)
		{
			if(this.IsExtension)
				throw new NotImplementedException();

			foreach(IDBColumn column in this.Values)
			{
				IDBColumn col = (IDBColumn)column.Eval(context);
				if(col is IDBColumnPrimary)
					this.PrimaryKey = (IDBColumnPrimary)col;
			}

			if(this.IsTemplate)
			{
				if(this.PrimaryKey != null)
					throw new Exception("Šablona tabulky nesmí mít primární klíč");
			}
			else
			{
				if(this.PrimaryKey == null)
					throw new Exception("Tabulka musí mít primární klíč");
			}

			return this;
		}

		public bool EvalAsBool (Context context)
		{
			throw new InvalidOperationException();
		}

		private List<IDBColumn> GetTemplateColumns(IDatabaseSchema database)
		{
			if(String.IsNullOrEmpty(TemplateName))
				return new List<IDBColumn>();
			IDBTable ttable = database[TemplateName];
			if(!ttable.IsTemplate)
				throw new Exception("Tabulku nelze použít jako šablonu tabulky");

			List<IDBColumn> list = new List<IDBColumn>();

			foreach(IDBColumn col in ttable.Values)
				list.Add(col);
			list.AddRange(((DBTableExpression)ttable).GetTemplateColumns(database));
			return list;
		}

		public void Resolve (IDatabaseSchema database)
		{
			if(this.IsTemplate)
				return;

			foreach(IDBColumn col in GetTemplateColumns(database))
				this.Add(col.Name, (IDBColumn)col.Clone());

			foreach(IDBColumn column in this.Values)
				column.Resolve(database, this);
		}

		public IDBColumnForeign[] FindTiesTo(IDBTable table)
		{
			List<IDBColumnForeign> fk = new List<IDBColumnForeign>();
			foreach(IDBColumn col in this.Values)
				if(col is IDBColumnForeign && ((IDBColumnForeign)col).ReferencesTable == table)
					fk.Add((IDBColumnForeign)col);

			return fk.ToArray();
		}

		public string CreateTableSQL()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("-- Table {0}\n", this.Name);
			sb.AppendFormat("CREATE TABLE {0} (\n", this.Name);

			List<string> colsqls = new List<string>();

			foreach(IDBColumn col in this.Values)
				colsqls.AddRange(col.CreateColumnsSQL(true));

			int lastrealcol=0;
			for(int i=colsqls.Count-1; i >= 0; i--)
			{
				if(colsqls[i].Trim().StartsWith("--"))
					continue;
				lastrealcol = i;
				break;
			}

			for(int i=0; i < colsqls.Count; i++)
			{
				string separator = (i < lastrealcol) ? "," : "";
				string line = colsqls[i];
				if(line.Trim().StartsWith("--"))
					separator = "";
				sb.AppendFormat("\t{0}{1}\n", line, separator);
			}

			sb.AppendLine(");");
			return sb.ToString();
		}

		public string CreateForeignKeysSQL()
		{
			StringBuilder sb = new StringBuilder();

			return sb.ToString();
		}

		public virtual DBTableExpression Clone()
		{
			DBTableExpression clone = new DBTableExpression(Name, IsTemplate, IsExtension, TemplateName);
			clone.Attribs = Attribs.Clone();
			clone.Indices = this.Indices.Clone();
			foreach(KeyValuePair<string, IDBColumn> kv in this)
				clone.Add(kv.Key, (IDBColumn)kv.Value.Clone());
			return clone;
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
	}
}
