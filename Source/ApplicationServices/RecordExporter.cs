using AudibleApi.Common;
using CsvHelper;
using DataLayer;
using Newtonsoft.Json.Linq;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices
{
	public static class RecordExporter
	{
		public static void ToXlsx(string saveFilePath, IEnumerable<IRecord> records)
		{
			if (!records.Any())
				return;

			using var workbook = new XSSFWorkbook();
			var sheet = workbook.CreateSheet("Records");

			var detailSubtotalFont = workbook.CreateFont();
			detailSubtotalFont.IsBold = true;

			var detailSubtotalCellStyle = workbook.CreateCellStyle();
			detailSubtotalCellStyle.SetFont(detailSubtotalFont);

			// headers
			var rowIndex = 0;
			var row = sheet.CreateRow(rowIndex);

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

			var col = 0;
			foreach (var c in columns)
			{
				var cell = row.CreateCell(col++);
				cell.SetCellValue(c);
				cell.CellStyle = detailSubtotalCellStyle;
			}

			var dateFormat = workbook.CreateDataFormat();
			var dateStyle = workbook.CreateCellStyle();
			dateStyle.DataFormat = dateFormat.GetFormat("MM/dd/yyyy HH:mm:ss");

			// Add data rows
			foreach (var record in records)
			{
				col = 0;

				row = sheet.CreateRow(++rowIndex);

				row.CreateCell(col++).SetCellValue(record.GetType().Name);

				var dateCreatedCell = row.CreateCell(col++);
				dateCreatedCell.CellStyle = dateStyle;
				dateCreatedCell.SetCellValue(record.Created.DateTime);

				row.CreateCell(col++).SetCellValue(record.Start.TotalMilliseconds);

				if (record is IAnnotation annotation)
				{
					row.CreateCell(col++).SetCellValue(annotation.AnnotationId);

					var lastModifiedCell = row.CreateCell(col++);
					lastModifiedCell.CellStyle = dateStyle;
					lastModifiedCell.SetCellValue(annotation.LastModified.DateTime);

					if (annotation is IRangeAnnotation rangeAnnotation)
					{
						row.CreateCell(col++).SetCellValue(rangeAnnotation.End.TotalMilliseconds);
						row.CreateCell(col++).SetCellValue(rangeAnnotation.Text);

						if (rangeAnnotation is Clip clip)
							row.CreateCell(col++).SetCellValue(clip.Title);
					}
				}
			}

			using var fileData = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
			workbook.Write(fileData);
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
}
