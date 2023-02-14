using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool CanUpdate => true;
		public void InstallUpdate(string updateBundle)
		{
			var thisExe = Environment.ProcessPath;
			var thisDir = Path.GetDirectoryName(thisExe);
			var zipExtractor = Path.Combine(Path.GetTempPath(), "ZipExtractor.exe");

			File.Copy("ZipExtractor.exe", zipExtractor, overwrite: true);

			RunAsRoot(zipExtractor, $"--input \"{updateBundle}\" --output \"{thisDir}\" --executable \"{thisExe}\"");
		}

		public Process RunAsRoot(string exe, string args)
        {
			var psi = new ProcessStartInfo()
			{
				FileName = exe,
				UseShellExecute = true,
				Verb = "runas",
				WindowStyle = ProcessWindowStyle.Normal,
				CreateNoWindow = true,
				Arguments = args
			};

            return Process.Start(psi);
		}
    }
}
