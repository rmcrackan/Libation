using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

#nullable enable
namespace LibationCli;

public enum Justify
{
	Left,
	Right,
	Center
}

public class TextTableOptions
{
	public Justify Justify { get; set; }
	public Justify CenterTiebreak { get; set; }
	public char PaddingCharacter { get; set; } = ' ';
	public int SideBorderPadding { get; set; } = 1;
	public int IntercellPadding { get; set; } = 1;
	public bool DrawBorder { get; set; } = true;
	public bool DrawHeader { get; set; } = true;
	public BorderDefinition Border { get; set; } = BorderDefinition.LightRounded;
}

public record BorderDefinition
{
	public char Vertical { get; set; }
	public char Horizontal { get; set; }
	public char VerticalSeparator { get; set; }
	public char HorizontalSeparator { get; set; }
	public char CornerTopLeft { get; set; }
	public char CornerTopRight { get; set; }
	public char CornerBottomLeft { get; set; }
	public char CornerBottomRight { get; set; }
	public char Tee { get; set; }
	public char TeeTop { get; set; }
	public char TeeBottom { get; set; }
	public char TeeLeft { get; set; }
	public char TeeRight { get; set; }

	public BorderDefinition(
		char vertical,
		char horizontal,
		char verticalSeparator,
		char horizontalSeparator,
		char cornerTopLef,
		char cornerTopRight,
		char cornerBottomLeft,
		char cornerBottomRight,
		char tee,
		char teeTop,
		char teeBottom,
		char teeLeft,
		char teeRight)
	{
		Vertical = vertical;
		Horizontal = horizontal;
		VerticalSeparator = verticalSeparator;
		HorizontalSeparator = horizontalSeparator;
		CornerTopLeft = cornerTopLef;
		CornerTopRight = cornerTopRight;
		CornerBottomLeft = cornerBottomLeft;
		CornerBottomRight = cornerBottomRight;
		Tee = tee;
		TeeTop = teeTop;
		TeeBottom = teeBottom;
		TeeLeft = teeLeft;
		TeeRight = teeRight;
	}

	public void TestPrint(TextWriter writer)
		=> writer.DrawTable<TestObject>([], new TextTableOptions { Border = this }, t => t.ColA, t => t.ColB, t => t.ColC);

	public static BorderDefinition Ascii => new BorderDefinition('|', '-', '|', '-', '-', '-', '-', '-', '|', '-', '-', '|', '|');
	public static BorderDefinition Light => new BorderDefinition('│', '─', '│', '─', '┌', '┐', '└', '┘', '┼', '┬', '┴', '├', '┤');
	public static BorderDefinition Heavy => new BorderDefinition('┃', '━', '┃', '━', '┏', '┓', '┗', '┛', '╋', '┳', '┻', '┣', '┫');
	public static BorderDefinition Double => new BorderDefinition('║', '═', '║', '═', '╔', '╗', '╚', '╝', '╬', '╦', '╩', '╠', '╣');
	public static BorderDefinition LightRounded => Light with { CornerTopLeft = '╭', CornerTopRight = '╮', CornerBottomLeft = '╰', CornerBottomRight = '╯' };
	public static BorderDefinition DoubleHorizontal => Light with { HorizontalSeparator = '═', Tee = '╪', TeeLeft = '╞', TeeRight = '╡' };
	public static BorderDefinition DoubleVertical => Light with { VerticalSeparator = '║', Tee = '╫', TeeTop = '╥', TeeBottom = '╨' };
	public static BorderDefinition DoubleOuter => Double with { VerticalSeparator = '│', HorizontalSeparator = '─', TeeLeft = '╟', TeeRight = '╢', Tee = '┼', TeeTop = '╤', TeeBottom = '╧' };
	public static BorderDefinition DoubleInner => Light with { VerticalSeparator = '║', HorizontalSeparator = '═', TeeLeft = '╞', TeeRight = '╡', Tee = '╬', TeeTop = '╥', TeeBottom = '╨' };

	private record TestObject(string ColA, string ColB, string ColC);
}

public record ColumnDef<T>(string ColumnName, Func<T, string?> ValueGetter);

public static class TextTableExtention
{
	/// <summary>
	/// Draw a text-based table to the provided TextWriter.
	/// </summary>
	/// <typeparam name="T">Data row type</typeparam>
	/// <param name="textWriter"></param>
	/// <param name="rows">Data rows to be drawn</param>
	/// <param name="options">Table drawing options</param>
	/// <param name="columnSelectors">Data cell selector. Header name is based on member name</param>
	public static void DrawTable<T>(this TextWriter textWriter, IEnumerable<T> rows, TextTableOptions options, params Expression<Func<T, string>>[] columnSelectors)
	{
		//Convert MemberExpression to ColumnDef<T>
		var columnDefs = new ColumnDef<T>[columnSelectors.Length];
		for (int i = 0; i < columnDefs.Length; i++)
		{
			var exp = columnSelectors[i].Body as MemberExpression
				   ?? throw new ArgumentException($"Expression at index {i} is not a member access expression", nameof(columnSelectors));

			columnDefs[i] = new ColumnDef<T>(exp.Member.Name, columnSelectors[i].Compile());
		}

		textWriter.DrawTable(rows, options, columnDefs);
	}

	/// <summary>
	/// Draw a text-based table to the provided TextWriter.
	/// </summary>
	/// <typeparam name="T">Data row type</typeparam>
	/// <param name="textWriter"></param>
	/// <param name="rows">Data rows to be drawn</param>
	/// <param name="options">Table drawing options</param>
	/// <param name="columnSelectors">Column header name and cell value selector.</param>
	public static void DrawTable<T>(this TextWriter textWriter, IEnumerable<T> rows, TextTableOptions options, params ColumnDef<T>[] columnSelectors)
	{
		var rowsArray = rows.ToArray();
		var colNames = columnSelectors.Select(c => c.ColumnName).ToArray();

		var colWidths = new int[columnSelectors.Length];
		for (int i = 0; i < columnSelectors.Length; i++)
		{
			var nameWidth = options.DrawHeader ? StrLen(colNames[i]) : 0;
			var maxValueWidth = rowsArray.Length == 0 ? 0 : rows.Max(o => StrLen(columnSelectors[i].ValueGetter(o)));
			colWidths[i] = Math.Max(nameWidth, maxValueWidth);
		}

		textWriter.DrawTop(colWidths, options);
		textWriter.DrawHeader(colNames, colWidths, options);
		foreach (var row in rowsArray)
		{
			textWriter.DrawLeft(options, options.Border.Vertical, options.PaddingCharacter);

			var cellValues = columnSelectors.Select((def, j) => def.ValueGetter(row).PadText(colWidths[j], options));
			textWriter.DrawRow(options, options.Border.VerticalSeparator, options.PaddingCharacter, cellValues);

			textWriter.DrawRight(options, options.Border.Vertical, options.PaddingCharacter);
		}
		textWriter.DrawBottom(colWidths, options);
	}

	private static void DrawHeader(this TextWriter textWriter, string[] colNames, int[] colWidths, TextTableOptions options)
	{
		if (!options.DrawHeader)
			return;
		//Draw column header names
		textWriter.DrawLeft(options, options.Border.Vertical, options.PaddingCharacter);

		var cellValues = colNames.Select((n, i) => n.PadText(colWidths[i], options));
		textWriter.DrawRow(options, options.Border.VerticalSeparator, options.PaddingCharacter, cellValues);

		textWriter.DrawRight(options, options.Border.Vertical, options.PaddingCharacter);

		//Draw header separator
		textWriter.DrawLeft(options, options.Border.TeeLeft, options.Border.HorizontalSeparator);

		cellValues = colWidths.Select(w => new string(options.Border.HorizontalSeparator, w));
		textWriter.DrawRow(options, options.Border.Tee, options.Border.HorizontalSeparator, cellValues);

		textWriter.DrawRight(options, options.Border.TeeRight, options.Border.HorizontalSeparator);
	}

	private static void DrawTop(this TextWriter textWriter, int[] colWidths, TextTableOptions options)
	{
		if (!options.DrawBorder)
			return;
		textWriter.DrawLeft(options, options.Border.CornerTopLeft, options.Border.Horizontal);

		var cellValues = colWidths.Select(w => new string(options.Border.Horizontal, w));
		textWriter.DrawRow(options, options.Border.TeeTop, options.Border.Horizontal, cellValues);

		textWriter.DrawRight(options, options.Border.CornerTopRight, options.Border.Horizontal);
	}

	private static void DrawBottom(this TextWriter textWriter, int[] colWidths, TextTableOptions options)
	{
		if (!options.DrawBorder)
			return;
		textWriter.DrawLeft(options, options.Border.CornerBottomLeft, options.Border.Horizontal);

		var cellValues = colWidths.Select(w => new string(options.Border.Horizontal, w));
		textWriter.DrawRow(options, options.Border.TeeBottom, options.Border.Horizontal, cellValues);

		textWriter.DrawRight(options, options.Border.CornerBottomRight, options.Border.Horizontal);
	}

	private static void DrawLeft(this TextWriter textWriter, TextTableOptions options, char borderChar, char cellPadChar)
	{
		if (!options.DrawBorder)
			return;
		textWriter.Write(borderChar);
		textWriter.Write(new string(cellPadChar, options.SideBorderPadding));
	}

	private static void DrawRight(this TextWriter textWriter, TextTableOptions options, char borderChar, char cellPadChar)
	{
		if (options.DrawBorder)
		{
			textWriter.Write(new string(cellPadChar, options.SideBorderPadding));
			textWriter.WriteLine(borderChar);
		}
		else
		{
			textWriter.WriteLine();
		}
	}

	private static void DrawRow(this TextWriter textWriter, TextTableOptions options, char colSeparator, char cellPadChar, IEnumerable<string> cellValues)
	{
		var cellPadding = new string(cellPadChar, options.IntercellPadding);
		var separator = cellPadding + colSeparator + cellPadding;
		textWriter.Write(string.Join(separator, cellValues));
	}

	private static string PadText(this string? text, int totalWidth, TextTableOptions options)
	{
		if (string.IsNullOrEmpty(text))
			return new string(options.PaddingCharacter, totalWidth);
		else if (StrLen(text) >= totalWidth)
			return text;

		return options.Justify switch
		{
			Justify.Right => PadLeft(text),
			Justify.Center => PadCenter(text),
			_ or Justify.Left => PadRight(text),
		};

		string PadCenter(string text)
		{
			var half = (totalWidth - StrLen(text)) / 2;

			text = options.CenterTiebreak == Justify.Right
				? new string(options.PaddingCharacter, half) + text
				: text + new string(options.PaddingCharacter, half);

			return options.CenterTiebreak == Justify.Right
				? text.PadRight(totalWidth, options.PaddingCharacter)
				: text.PadLeft(totalWidth, options.PaddingCharacter);
		}

		string PadLeft(string text)
		{
			var padSize = totalWidth - StrLen(text);
			return new string(options.PaddingCharacter, padSize) + text;
		}

		string PadRight(string text)
		{
			var padSize = totalWidth - StrLen(text);
			return text + new string(options.PaddingCharacter, padSize);
		}
	}

	/// <summary>
	/// Determine the width of the string in console characters, accounting for wide unicode characters.
	/// </summary>
	private static int StrLen(string? str)
		=> string.IsNullOrEmpty(str) ? 0 : str.Sum(c => CharIsWide(c) ? 2 : 1);

	/// <summary>
	/// Determines if the character is a unicode "Full Width" character which takes up two spaces in the console.
	/// </summary>
	static bool CharIsWide(char c)
		=> (c >= '\uFF01' && c <= '\uFF61') || (c >= '\uFFE0' && c <= '\uFFE6');
}
