using System;
using System.Collections.Generic;
using System.Drawing;
using Dinah.Core.Drawing;
using FileManager;

namespace WindowsDesktopUtilities
{
	public static class WinAudibleImageServer
	{
		private static Dictionary<PictureDefinition, Image> cache { get; } = new Dictionary<PictureDefinition, Image>();

		public static Image GetImage(string pictureId, PictureSize size)
		{
			var def = new PictureDefinition(pictureId, size);
			if (!cache.ContainsKey(def))
			{
				(var isDefault, var bytes) = PictureStorage.GetPicture(def);

				var image = ImageReader.ToImage(bytes);
				if (isDefault)
					return image;
				cache[def] = image;
			}
			return cache[def];
		}
	}
}
