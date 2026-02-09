using DataLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LibationSearchEngine;

[DebuggerDisplay("Count = {rules.Count,nq}")]
public class IndexRuleCollection : IEnumerable<IndexRule>
{
	private readonly List<IndexRule> rules = new();
	public IEnumerable<string> IdFieldNames => rules.Where(x => x.FieldType is FieldType.ID).SelectMany(r => r.FieldNames);
	public IEnumerable<string> BoolFieldNames => rules.Where(x => x.FieldType is FieldType.Bool).SelectMany(r => r.FieldNames);
	public IEnumerable<string> StringFieldNames => rules.Where(x => x.FieldType is FieldType.String).SelectMany(r => r.FieldNames);
	public IEnumerable<string> NumberFieldNames => rules.Where(x => x.FieldType is FieldType.Number).SelectMany(r => r.FieldNames);

	public void Add(FieldType fieldType, Func<LibraryBook, string?> getter, params string[] fieldNames)
		=> rules.Add(new IndexRule(fieldType, getter, fieldNames));

	public IndexRule? GetRuleByFieldName(string fieldName)
		=> rules.SingleOrDefault(r => r.FieldNames.Any(n => n.Equals(fieldName, StringComparison.OrdinalIgnoreCase)));

	public IEnumerator<IndexRule> GetEnumerator() => rules.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
