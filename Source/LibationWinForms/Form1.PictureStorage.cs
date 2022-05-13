using Dinah.Core.Drawing;
using LibationFileManager;

namespace LibationWinForms
{
    public partial class Form1
    {
        private void Configure_PictureStorage()
        {
            // init default/placeholder cover art
            var format = System.Drawing.Imaging.ImageFormat.Jpeg;
            PictureStorage.SetDefaultImage(PictureSize._80x80, Properties.Resources.default_cover_80x80.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize._300x300, Properties.Resources.default_cover_300x300.ToBytes(format));
            PictureStorage.SetDefaultImage(PictureSize._500x500, Properties.Resources.default_cover_500x500.ToBytes(format));
        }
    }
}
