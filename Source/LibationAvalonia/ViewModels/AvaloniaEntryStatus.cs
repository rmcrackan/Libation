using Avalonia.Media;
using Avalonia.Media.Imaging;
using DataLayer;
using LibationUiBase.GridView;
using System;

namespace LibationAvalonia.ViewModels
{
    public class AvaloniaEntryStatus : EntryStatus, IEntryStatus, IComparable
	{
		public override IBrush BackgroundBrush => IsEpisode ? App.SeriesEntryGridBackgroundBrush : Brushes.Transparent;

		private AvaloniaEntryStatus(LibraryBook libraryBook) : base(libraryBook) { }
		public static EntryStatus Create(LibraryBook libraryBook) => new AvaloniaEntryStatus(libraryBook);

		protected override Bitmap LoadImage(byte[] picture)
			=> AvaloniaUtils.TryLoadImageOrDefault(picture, LibationFileManager.PictureSize._80x80);

		//Button icons are handled by LiberateStatusButton
		protected override Bitmap GetResourceImage(string rescName) => null;
	}
}
