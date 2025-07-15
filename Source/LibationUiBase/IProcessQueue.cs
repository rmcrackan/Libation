using DataLayer;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase;

public interface IProcessQueue
{
	bool RemoveCompleted(LibraryBook libraryBook);
	void AddDownloadPdf(IEnumerable<LibraryBook> entries);	
	void AddConvertMp3(IEnumerable<LibraryBook> entries);	
	void AddDownloadDecrypt(IEnumerable<LibraryBook> entries);	
}

public static class ProcessQueueExtensions
{

	public static bool QueueDownloadPdf(this IProcessQueue queue, IList<LibraryBook> libraryBooks)
	{
		var needsPdf = libraryBooks.Where(lb => !lb.AbsentFromLastScan && lb.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated).ToArray();
		if (needsPdf.Length > 0)
		{
			Serilog.Log.Logger.Information("Begin download {count} pdfs", needsPdf.Length);
			queue.AddDownloadPdf(needsPdf);
			return true;
		}
		return false;
	}

	public static bool QueueConvertToMp3(this IProcessQueue queue, IList<LibraryBook> libraryBooks)
	{
		//Only Queue Liberated books for conversion.  This isn't a perfect filter, but it's better than nothing.
		var preLiberated = libraryBooks.Where(lb => !lb.AbsentFromLastScan && lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated && lb.Book.ContentType is DataLayer.ContentType.Product).ToArray();
		if (preLiberated.Length > 0)
		{
			Serilog.Log.Logger.Information("Begin convert {count} books to mp3", preLiberated.Length);
			queue.AddConvertMp3(preLiberated);
			return true;
		}
		return false;
	}

	public static bool QueueDownloadDecrypt(this IProcessQueue queue, IList<LibraryBook> libraryBooks)
	{
		if (libraryBooks.Count == 1)
		{
			var item = libraryBooks[0];

			if (item.AbsentFromLastScan)
				return false;
			else if(item.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload)
			{
				queue.RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single library book backup of {libraryBook}", item);
				queue.AddDownloadDecrypt([item]);
				return true;
			}
			else if (item.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
			{
				queue.RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", item);
				queue.AddDownloadPdf([item]);
				return true;
			}
		}
		else
		{
			var toLiberate
				= libraryBooks
				.Where(x => !x.AbsentFromLastScan && x.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload || x.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
				.ToArray();

			if (toLiberate.Length > 0)
			{
				Serilog.Log.Logger.Information("Begin backup of {count} library books", toLiberate.Length);
				queue.AddDownloadDecrypt(toLiberate);
				return true;
			}
		}
		return false;
	}
}
