using DataLayer;
using System;
using System.Collections.ObjectModel;

namespace LibationSearchEngine;

public class BookRule : IIndexRule
{
	public FieldType FieldType { get; }
	public Func<Book, string> ValueGetter { get; }
	public ReadOnlyCollection<string> FieldNames { get; }

	public BookRule(FieldType fieldType, Func<Book, string> valueGetter, params string[] fieldNames)
	{
		ValueGetter = valueGetter;
		FieldType = fieldType;
		FieldNames = new ReadOnlyCollection<string>(fieldNames);
	}

	public string GetValue(LibraryBook libraryBook) => ValueGetter(libraryBook.Book);
}
