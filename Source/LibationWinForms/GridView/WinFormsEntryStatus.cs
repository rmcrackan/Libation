using DataLayer;
using Dinah.Core.WindowsDesktop.Drawing;
using LibationUiBase.GridView;
using System;
using System.Drawing;

namespace LibationWinForms.GridView
{
	public class WinFormsEntryStatus : EntryStatus, IEntryStatus
	{
		private static readonly Color SERIES_BG_COLOR = Color.FromArgb(230, 255, 230);
		public override object BackgroundBrush => IsEpisode ? SERIES_BG_COLOR : SystemColors.ControlLightLight;

		private WinFormsEntryStatus(LibraryBook libraryBook) : base(libraryBook) { }
		public static EntryStatus Create(LibraryBook libraryBook) => new WinFormsEntryStatus(libraryBook);

		protected override object LoadImage(byte[] picture)
		{
			try
			{
				return ImageReader.ToImage(picture);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error loading cover art for {Book}", Book);
				return Properties.Resources.default_cover_80x80;
			}
		}

		protected override Image GetResourceImage(string rescName)
		{
			var image = Properties.Resources.ResourceManager.GetObject(rescName);

			return image as Bitmap;
		}
	}
}
