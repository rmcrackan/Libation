using DataLayer;
using System;
using System.Collections.ObjectModel;

namespace LibationSearchEngine;

public class LibraryBookRule : IIndexRule
{
	public FieldType FieldType { get; }
	public Func<LibraryBook, string> ValueGetter { get; }
	public ReadOnlyCollection<string> FieldNames { get; }

	public LibraryBookRule(FieldType fieldType, Func<LibraryBook, string> valueGetter, params string[] fieldNames)
	{
		ValueGetter = valueGetter;
		FieldType = fieldType;
		FieldNames = new ReadOnlyCollection<string>(fieldNames);
	}

	public string GetValue(LibraryBook libraryBook) => ValueGetter(libraryBook);
}
