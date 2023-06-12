using DataLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LibationSearchEngine;

public class IndexRuleCollection : IEnumerable<IIndexRule>
{
	private readonly List<IIndexRule> rules = new();
	public IEnumerable<string> IdFieldNames => rules.Where(x => x.FieldType is FieldType.ID).SelectMany(r => r.FieldNames);
	public IEnumerable<string> BoolFieldNames => rules.Where(x => x.FieldType is FieldType.Bool).SelectMany(r => r.FieldNames);
	public IEnumerable<string> StringFieldNames => rules.Where(x => x.FieldType is FieldType.String).SelectMany(r => r.FieldNames);
	public IEnumerable<string> NumberFieldNames => rules.Where(x => x.FieldType is FieldType.Number).SelectMany(r => r.FieldNames);

	public void Add(FieldType fieldType, Func<LibraryBook, string> getter, params string[] fieldNames)
		=> rules.Add(new LibraryBookRule(fieldType, getter, fieldNames));

	public void Add(FieldType fieldType, Func<Book, string> getter, params string[] fieldNames)
		=> rules.Add(new BookRule(fieldType, getter, fieldNames));
	
	public T GetRuleByFieldName<T>(string fieldName) where T : IIndexRule
		=> (T)rules.SingleOrDefault(r => r.FieldNames.Any(n => n.Equals(fieldName, StringComparison.OrdinalIgnoreCase)));

	public IEnumerator<IIndexRule> GetEnumerator() => rules.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
