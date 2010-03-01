using System;

namespace LPS.ToolScript.Parser
{
	public interface IDBColumn : IDBSchemaItem
	{
		IDBTable Table { get; }
		Type DataType { get; }
		EvaluatedAttributeList Attribs { get; }
		bool IsPrimary { get; }
		bool IsUnique { get; }
		bool IsNotNull { get; }
		bool IsIndex { get; }

		object NormalizeValue(object value);
		string DisplayValue(object value, string format, string ifnull);
		string DisplayValue(object value, bool allow_tags);
	}
}
