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
	}
}
