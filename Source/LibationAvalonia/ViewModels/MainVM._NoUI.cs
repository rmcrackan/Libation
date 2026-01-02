using LibationFileManager;
using LibationUiBase;
using System;
using System.IO;

namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		public  static void Configure_NonUI()
		{
			using var ms1 = new MemoryStream();
			App.OpenAsset("img-coverart-prod-unavailable_80x80.jpg").CopyTo(ms1);
			PictureStorage.SetDefaultImage(PictureSize._80x80, ms1.ToArray());

			using var ms2 = new MemoryStream();
			App.OpenAsset("img-coverart-prod-unavailable_300x300.jpg").CopyTo(ms2);
			PictureStorage.SetDefaultImage(PictureSize._300x300, ms2.ToArray());

			using var ms3 = new MemoryStream();
			App.OpenAsset("img-coverart-prod-unavailable_500x500.jpg").CopyTo(ms3);
			PictureStorage.SetDefaultImage(PictureSize._500x500, ms3.ToArray());
			PictureStorage.SetDefaultImage(PictureSize.Native, ms3.ToArray());

			BaseUtil.SetLoadImageDelegate(AvaloniaUtils.TryLoadImageOrDefault);
			BaseUtil.SetLoadResourceImageDelegate(LoadResourceImage);
		}
		private static Avalonia.Media.Imaging.Bitmap? LoadResourceImage(string resourceName)
		{
			try
			{
				using var stream = App.OpenAsset(resourceName);
				return new Avalonia.Media.Imaging.Bitmap(stream);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Failed to load resource image: {ResourceName}", resourceName);
				return null;
			}
		}
	}
}
