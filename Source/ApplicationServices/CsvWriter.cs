using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApplicationServices;

/// <summary>Column header for csv and xlsx export. Mirrors the CsvHelper attribute it replaced.</summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class NameAttribute(params string[] names) : Attribute
{
	public string[] Names { get; } = names;
}

/// <summary>Excludes a property from csv export. Mirrors the CsvHelper attribute it replaced.</summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class IgnoreAttribute : Attribute { }

/// <summary>
/// Minimal write-only CSV serializer, output-compatible with the way this project used
/// CsvHelper: the delimiter is the culture's list separator, records end with CRLF,
/// fields containing the delimiter, quotes, newlines, or leading/trailing spaces are
/// quoted per RFC 4180, and values are formatted with the culture. Columns are the
/// public instance properties of each record's runtime type, in declaration order,
/// honoring <see cref="NameAttribute"/> and <see cref="IgnoreAttribute"/>.
/// </summary>
internal sealed class CsvWriter(TextWriter writer, CultureInfo culture) : IDisposable
{
	private readonly string delimiter = culture.TextInfo.ListSeparator;
	private readonly Dictionary<Type, List<(string Header, PropertyInfo Property)>> columnCache = new();
	private bool rowHasFields;

	public void WriteHeader(Type type)
	{
		foreach (var (header, _) in getColumns(type))
			writeField(header);
	}

	public void NextRecord()
	{
		writer.Write("\r\n");
		rowHasFields = false;
	}

	public void WriteRecords<T>(IEnumerable<T> records) where T : notnull
	{
		foreach (var record in records)
		{
			foreach (var (_, property) in getColumns(record.GetType()))
				writeField(toString(property.GetValue(record)));
			NextRecord();
		}
	}

	public void Dispose() => writer.Dispose();

	private void writeField(string field)
	{
		if (rowHasFields)
			writer.Write(delimiter);
		writer.Write(escape(field));
		rowHasFields = true;
	}

	private string escape(string field)
	{
		if (field.Length == 0)
			return field;

		var needsQuoting
			= field.Contains('"')
			|| field[0] == ' '
			|| field[^1] == ' '
			|| field.Contains(delimiter)
			|| field.Contains('\r')
			|| field.Contains('\n');

		return needsQuoting ? $"\"{field.Replace("\"", "\"\"")}\"" : field;
	}

	private string toString(object? value)
		=> value switch
		{
			null => "",
			string s => s,
			IFormattable formattable => formattable.ToString(null, culture),
			_ => value.ToString() ?? ""
		};

	private List<(string Header, PropertyInfo Property)> getColumns(Type type)
	{
		if (!columnCache.TryGetValue(type, out var columns))
			columnCache[type] = columns = type
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.GetMethod is not null
					&& p.GetIndexParameters().Length == 0
					&& p.GetCustomAttribute<IgnoreAttribute>() is null)
				.Select(p => (p.GetCustomAttribute<NameAttribute>()?.Names.FirstOrDefault() ?? p.Name, p))
				.ToList();
		return columns;
	}
}
