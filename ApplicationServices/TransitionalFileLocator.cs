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
		public static string Audio_GetPath(string productId)
		{
			var book = DbContexts.GetContext().GetBook_Flat_NoTracking(productId);
			var loc = book?.UserDefinedItem?.BookLocation ?? "";
			if (File.Exists(loc))
				return loc;

			return AudibleFileStorage.Audio.GetPath(productId);
		}

		public static bool PDF_Exists(string productId)
		{
			var book = DbContexts.GetContext().GetBook_Flat_NoTracking(productId);
			var status = book?.UserDefinedItem?.PdfStatus;
			if (status.HasValue && status.Value == LiberatedStatus.Liberated)
				return true;

			return AudibleFileStorage.PDF.Exists(productId);
		}

		public static bool Audio_Exists(string productId)
		{
			var book = DbContexts.GetContext().GetBook_Flat_NoTracking(productId);
			var status = book?.UserDefinedItem?.BookStatus;
			// true since Error == libhack
			if (status != LiberatedStatus.NotLiberated)
				return true;

			return AudibleFileStorage.Audio.Exists(productId);
		}

		public static bool AAXC_Exists(string productId)
		{
			// this one will actually stay the same. centralizing helps with organization in the interim though
			return AudibleFileStorage.AAXC.Exists(productId);
		}
	}
}
