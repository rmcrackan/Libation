using Avalonia.Controls;
using Dinah.Core.Drawing;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class MainWindow
	{
		private void Configure_NonUI()
		{
			// init default/placeholder cover art
			var format = System.Drawing.Imaging.ImageFormat.Jpeg;
			PictureStorage.SetDefaultImage(PictureSize._80x80, Properties.Resources.default_cover_80x80.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize._300x300, Properties.Resources.default_cover_300x300.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize._500x500, Properties.Resources.default_cover_500x500.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize.Native, Properties.Resources.default_cover_500x500.ToBytes(format));
		}
	}
}
