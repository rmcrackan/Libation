using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLayer;
using FileManager;

namespace ApplicationServices
{
	public static class TransitionalFileLocator
	{
		public static string Audio_GetPath(Book book)
		{
			var loc = book?.UserDefinedItem?.BookLocation ?? "";
			if (File.Exists(loc))
				return loc;

			return AudibleFileStorage.Audio.GetPath(book.AudibleProductId);
		}

		public static bool PDF_Exists(Book book)
		{
			var status = book?.UserDefinedItem?.PdfStatus;
			if (status.HasValue && status.Value == LiberatedStatus.Liberated)
				return true;

			return AudibleFileStorage.PDF.Exists(book.AudibleProductId);
		}

		public static bool Audio_Exists(Book book)
		{
			var status = book?.UserDefinedItem?.BookStatus;
			// true since Error == libhack
			if (status.HasValue && status.Value != LiberatedStatus.NotLiberated)
				return true;

			return AudibleFileStorage.Audio.Exists(book.AudibleProductId);
		}

		public static bool AAXC_Exists(Book book)
		{
			// this one will actually stay the same. centralizing helps with organization in the interim though
			return AudibleFileStorage.AAXC.Exists(book.AudibleProductId);
		}
	}
}
