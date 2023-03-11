using Avalonia.Media;
using Avalonia.Media.Imaging;
using DataLayer;
using LibationUiBase.GridView;
using System;

namespace LibationAvalonia.ViewModels
{
    public class AvaloniaEntryStatus : EntryStatus, IEntryStatus, IComparable
	{
		private static Bitmap _defaultImage;
		public override IBrush BackgroundBrush => IsEpisode ? App.SeriesEntryGridBackgroundBrush : Brushes.Transparent;

		private AvaloniaEntryStatus(LibraryBook libraryBook) : base(libraryBook) { }
		public static EntryStatus Create(LibraryBook libraryBook) => new AvaloniaEntryStatus(libraryBook);

		protected override Bitmap LoadImage(byte[] picture)
		{
			try
			{
				using var ms = new System.IO.MemoryStream(picture);
				return new Bitmap(ms);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error loading cover art for {Book}", Book);
				return _defaultImage ??= new Bitmap(App.OpenAsset("img-coverart-prod-unavailable_80x80.jpg"));
			}
		}

		protected override Bitmap GetResourceImage(string rescName)
		{
			using var stream = App.OpenAsset(rescName + ".png");
			return new Bitmap(stream);
		}
	}
}
