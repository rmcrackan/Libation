using DataLayer;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationServices;

public class BulkSetDownloadStatus
{
	private List<(string message, LiberatedStatus newStatus, IEnumerable<LibraryBook> LibraryBooks, bool isPdf)> actionSets { get; } = new();

	public int Count => actionSets.Count;

	public IEnumerable<string> Messages => actionSets.Select(a => a.message);
	public string AggregateMessage => $"Are you sure you want to set {Messages.Aggregate((a, b) => $"{a} and {b}")}?";

	private readonly List<LibraryBook> _libraryBooks;
	private readonly bool _setDownloaded;
	private readonly bool _setNotDownloaded;
	private readonly Func<string, bool> _audioExists;
	private readonly Func<string, bool> _pdfExists;

	public BulkSetDownloadStatus(List<LibraryBook> libraryBooks, bool setDownloaded, bool setNotDownloaded)
		: this(
			libraryBooks,
			setDownloaded,
			setNotDownloaded,
			productId => AudibleFileStorage.Audio.GetPath(productId) is not null,
			productId => AudibleFileStorage.Supplement.GetPath(productId) is not null)
	{ }

	/// <summary>Takes the file lookups as arguments, so tests need no Books directory.</summary>
	internal BulkSetDownloadStatus(List<LibraryBook> libraryBooks, bool setDownloaded, bool setNotDownloaded, Func<string, bool> audioExists, Func<string, bool> pdfExists)
	{
		_libraryBooks = libraryBooks;
		_setDownloaded = setDownloaded;
		_setNotDownloaded = setNotDownloaded;
		_audioExists = audioExists;
		_pdfExists = pdfExists;
	}

	public int Discover()
	{
		var bookExistsList = _libraryBooks
			.Select(libraryBook => new
			{
				LibraryBook = libraryBook,
				FileExists = _audioExists(libraryBook.Book.AudibleProductId),
				// A book without a supplement has no PDF status to set, so its folder is not searched for one.
				PdfExists = libraryBook.Book.UserDefinedItem.PdfStatus is not null && _pdfExists(libraryBook.Book.AudibleProductId)
			})
			.ToList();

		if (_setDownloaded)
		{
			var books2change = bookExistsList
				.Where(a => a.FileExists && a.LibraryBook.Book.UserDefinedItem.BookStatus != LiberatedStatus.Liberated)
				.Select(a => a.LibraryBook)
				.ToList();

			if (books2change.Any())
				actionSets.Add((
					$"{"book".PluralizeWithCount(books2change.Count)} to 'Downloaded'",
					LiberatedStatus.Liberated,
					books2change,
					false));

			// The PDF download goes by status alone, so a library rebuilt from the files on disk would otherwise
			// fetch every PDF it already has.
			var pdfs2change = bookExistsList
				.Where(a => a.PdfExists && a.LibraryBook.Book.UserDefinedItem.PdfStatus != LiberatedStatus.Liberated)
				.Select(a => a.LibraryBook)
				.ToList();

			if (pdfs2change.Any())
				actionSets.Add((
					$"{"PDF".PluralizeWithCount(pdfs2change.Count)} to 'Downloaded'",
					LiberatedStatus.Liberated,
					pdfs2change,
					true));
		}

		if (_setNotDownloaded)
		{
			var books2change = bookExistsList
				.Where(a => !a.FileExists && a.LibraryBook.Book.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated)
				.Select(a => a.LibraryBook)
				.ToList();

			if (books2change.Any())
				actionSets.Add((
					$"{"book".PluralizeWithCount(books2change.Count)} to 'Download Pending'",
					LiberatedStatus.NotLiberated,
					books2change,
					false));

			// Only a PDF marked downloaded is set back. Error records that Audible has no PDF to give, which a
			// missing file does nothing to contradict, and setting it pending would have the next liberate ask
			// Audible for it again (issue #1973).
			var pdfs2change = bookExistsList
				.Where(a => !a.PdfExists && a.LibraryBook.Book.UserDefinedItem.PdfStatus == LiberatedStatus.Liberated)
				.Select(a => a.LibraryBook)
				.ToList();

			if (pdfs2change.Any())
				actionSets.Add((
					$"{"PDF".PluralizeWithCount(pdfs2change.Count)} to 'Download Pending'",
					LiberatedStatus.NotLiberated,
					pdfs2change,
					true));
		}

		return Count;
	}

	public async Task ExecuteAsync()
	{
		foreach (var a in actionSets)
		{
			if (a.isPdf)
				await a.LibraryBooks.UpdatePdfStatusAsync(a.newStatus);
			else
				await a.LibraryBooks.UpdateBookStatusAsync(a.newStatus);
		}
	}
}
