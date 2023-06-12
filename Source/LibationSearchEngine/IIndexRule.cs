using DataLayer;
using System.Collections.ObjectModel;

namespace LibationSearchEngine;

public enum FieldType
{
	Bool,
	String,
	Number,
	ID,
	Raw
}

public interface IIndexRule
{
	/// <summary> This rule's value type. </summary>
	FieldType FieldType { get; }
	/// <summary> All aliases of this search index rule </summary>
	ReadOnlyCollection<string> FieldNames { get; }
	string GetValue(LibraryBook libraryBook);
}
