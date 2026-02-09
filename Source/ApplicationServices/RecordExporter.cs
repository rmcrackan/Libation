using AudibleApi.Common;
using ClosedXML.Excel;
using CsvHelper;
using DataLayer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApplicationServices;

public static class RecordExporter
{
	public static void ToXlsx(string saveFilePath, IEnumerable<IRecord> records)
	{
		if (!records.Any())
			return;

		using var workbook = new XLWorkbook();
		var worksheet = workbook.AddWorksheet("Records");

		// headers
		var columns = new List<string>
		{
			nameof(Type.Name),
			nameof(IRecord.Created),
			nameof(IRecord.Start) + "_ms",
		};

		if (records.OfType<IAnnotation>().Any())
		{
			columns.Add(nameof(IAnnotation.AnnotationId));
			columns.Add(nameof(IAnnotation.LastModified));
		}
		if (records.OfType<IRangeAnnotation>().Any())
		{
			columns.Add(nameof(IRangeAnnotation.End) + "_ms");
			columns.Add(nameof(IRangeAnnotation.Text));
		}
		if (records.OfType<Clip>().Any())
			columns.Add(nameof(Clip.Title));

		int rowIndex = 1, col = 1;
		var headerRow = worksheet.Row(rowIndex++);
		foreach (var c in columns)
		{
			var headerCell = headerRow.Cell(col++);
			headerCell.Value = c;
			headerCell.Style.Font.Bold = true;
		}

		var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " HH:mm:ss";

		// Add data rows
		foreach (var record in records)
		{
			col = 1;
			var row = worksheet.Row(rowIndex++);

			row.Cell(col++).Value = record.GetType().Name;
			row.Cell(col++).SetDate(record.Created.DateTime, dateFormat);
			row.Cell(col++).Value = record.Start.TotalMilliseconds;

			if (record is IAnnotation annotation)
			{

				row.Cell(col++).Value = annotation.AnnotationId;
				row.Cell(col++).SetDate(annotation.LastModified.DateTime, dateFormat);

				if (annotation is IRangeAnnotation rangeAnnotation)
				{
					row.Cell(col++).Value = rangeAnnotation.End.TotalMilliseconds;
					row.Cell(col++).Value = rangeAnnotation.Text;

					if (rangeAnnotation is Clip clip)
						row.Cell(col++).Value = clip.Title;
				}
			}
		}

		workbook.SaveAs(saveFilePath);
	}

	private static void SetDate(this IXLCell cell, DateTime? value, string dateFormat)
	{
		cell.Value = value;
		cell.Style.DateFormat.Format = dateFormat;
	}
	public static void ToJson(string saveFilePath, LibraryBook libraryBook, IEnumerable<IRecord> records)
	{
		if (!records.Any())
			return;

		var recordsEx = extendRecords(records);

		var recordsObj = new JObject
		{
			{ "title", libraryBook.Book.TitleWithSubtitle},
			{ "asin", libraryBook.Book.AudibleProductId},
			{ "exportTime", DateTime.Now},
			{ "records", JArray.FromObject(recordsEx) }
		};

		System.IO.File.WriteAllText(saveFilePath, recordsObj.ToString(Newtonsoft.Json.Formatting.Indented));
	}

	public static void ToCsv(string saveFilePath, IEnumerable<IRecord> records)
	{
		if (!records.Any())
			return;

		using var writer = new System.IO.StreamWriter(saveFilePath);
		using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture);

		//Write headers for the present record type that has the most properties
		if (records.OfType<Clip>().Any())
			csv.WriteHeader(typeof(ClipEx));
		else if (records.OfType<Note>().Any())
			csv.WriteHeader(typeof(NoteEx));
		else if (records.OfType<Bookmark>().Any())
			csv.WriteHeader(typeof(BookmarkEx));
		else
			csv.WriteHeader(typeof(LastHeardEx));

		var recordsEx = extendRecords(records);

		csv.NextRecord();
		csv.WriteRecords(recordsEx.OfType<ClipEx>());
		csv.WriteRecords(recordsEx.OfType<NoteEx>());
		csv.WriteRecords(recordsEx.OfType<BookmarkEx>());
		csv.WriteRecords(recordsEx.OfType<LastHeardEx>());
	}

	private static IEnumerable<IRecordEx> extendRecords(IEnumerable<IRecord> records)
		=> records
		.Select<IRecord, IRecordEx>(
			r => r switch
			{
				Clip c => new ClipEx(nameof(Clip), c),
				Note n => new NoteEx(nameof(Note), n),
				Bookmark b => new BookmarkEx(nameof(Bookmark), b),
				LastHeard l => new LastHeardEx(nameof(LastHeard), l),
				_ => throw new InvalidOperationException(),
			});


	private interface IRecordEx { string Type { get; } }

	private record LastHeardEx : LastHeard, IRecordEx
	{
		public string Type { get; }
		public LastHeardEx(string type, LastHeard original) : base(original)
		{
			Type = type;
		}
	}

	private record BookmarkEx : Bookmark, IRecordEx
	{
		public string Type { get; }
		public BookmarkEx(string type, Bookmark original) : base(original)
		{
			Type = type;
		}
	}

	private record NoteEx : Note, IRecordEx
	{
		public string Type { get; }
		public NoteEx(string type, Note original) : base(original)
		{
			Type = type;
		}
	}

	private record ClipEx : Clip, IRecordEx
	{
		public string Type { get; }
		public ClipEx(string type, Clip original) : base(original)
		{
			Type = type;
		}
	}
}
