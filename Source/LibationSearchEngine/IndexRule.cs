using DataLayer;
using Dinah.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibationSearchEngine;

public enum FieldType
{
	Bool,
	String,
	Number,
	ID,
	Raw
}

public class IndexRule
{
	public FieldType FieldType { get; }
	public Func<LibraryBook, string> GetValue { get; }
	public ReadOnlyCollection<string> FieldNames { get; }

	public IndexRule(FieldType fieldType, Func<LibraryBook, string> valueGetter, params string[] fieldNames)
	{
		ArgumentValidator.EnsureNotNull(valueGetter, nameof(valueGetter));
		ArgumentValidator.EnsureNotNull(fieldNames, nameof(fieldNames));
		ArgumentValidator.EnsureGreaterThan(fieldNames.Length, $"{nameof(fieldNames)}.{nameof(fieldNames.Length)}", 0);
		var fieldNamesValidated
			= fieldNames
			.Select((n, i) => ArgumentValidator.EnsureNotNullOrWhiteSpace(n, $"{nameof(fieldNames)}[{i}]")
			.Trim());

		GetValue = valueGetter;
		FieldType = fieldType;
		FieldNames = new ReadOnlyCollection<string>(fieldNamesValidated.ToList());
	}
	public override string ToString()
		=> FieldNames.Count == 1
		? $"{FieldNames.First()}"
		: $"{FieldNames.First()} ({string.Join(", ", FieldNames.Skip(1))})";	
}
