using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core.WindowsDesktop;
using Dinah.Core.WindowsDesktop.Drawing;
using LibationFileManager;

namespace WindowsConfigApp
{
    internal class WinInterop : IInteropFunctions
    {
        public WinInterop() { }
        public WinInterop(params object[] values) { }

        public void SetFolderIcon(string image, string directory)
        {
            string iconPath = null;

            try
            {
                var icon = ImageReader.ToIcon(image);
                iconPath = Path.Combine(directory, $"{Guid.NewGuid()}.ico");
                icon.Save(iconPath);

                new DirectoryInfo(directory).SetIcon(iconPath, Directories.FolderTypes.Music);
            }
            finally
            {
                if (File.Exists(iconPath))
                    File.Delete(iconPath);
            }
        }

        public void DeleteFolderIcon(string directory)
            => new DirectoryInfo(directory)?.DeleteIcon();

        public void CopyTextToClipboard(string text)
            => Clipboard.SetText(text);
    }
}
