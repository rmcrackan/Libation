using DataLayer;
using LibationUiBase.GridView;
using System.Drawing;

namespace LibationWinForms.GridView
{
	public class WinFormsEntryStatus : EntryStatus, IEntryStatus
	{
		private static readonly Color SERIES_BG_COLOR = Color.FromArgb(230, 255, 230);
		public Color BackgroundBrush => IsEpisode ? SERIES_BG_COLOR : SystemColors.ControlLightLight;

		private WinFormsEntryStatus(LibraryBook libraryBook) : base(libraryBook) { }
		public static EntryStatus Create(LibraryBook libraryBook) => new WinFormsEntryStatus(libraryBook);

		protected override Image LoadImage(byte[] picture)
			=> WinFormsUtil.TryLoadImageOrDefault(picture, LibationFileManager.PictureSize._80x80);

		protected override Image GetResourceImage(string rescName)
		{
			var image = Properties.Resources.ResourceManager.GetObject(rescName);
			return image as Bitmap;
		}
	}
}
