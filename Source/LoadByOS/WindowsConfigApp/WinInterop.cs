using SixLabors.ImageSharp;
using System.Diagnostics;
using LibationFileManager;
using System.IO;
using System;
using Dinah.Core;

namespace WindowsConfigApp
{
    internal class WinInterop : IInteropFunctions
    {
        public WinInterop() { }
        public WinInterop(params object[] values) { }

#nullable enable
		public IWebViewAdapter? CreateWebViewAdapter() => new WindowsWebView2Adapter();
#nullable disable
		public void SetFolderIcon(string image, string directory)
        {
			var icon = Image.Load(image).ToIcon();
			new DirectoryInfo(directory)?.SetIcon(icon, "Music");
		}

        public void DeleteFolderIcon(string directory)
            => new DirectoryInfo(directory)?.DeleteIcon();

        public bool CanUpgrade => true;
		public void InstallUpgrade(string upgradeBundle)
		{
			var thisExe = Environment.ProcessPath;
			var thisDir = Path.GetDirectoryName(thisExe);
			var zipExtractor = Path.Combine(Path.GetTempPath(), "ZipExtractor.exe");

			File.Copy("ZipExtractor.exe", zipExtractor, overwrite: true);

			RunAsRoot(zipExtractor,
                $"--input {upgradeBundle.SurroundWithQuotes()} " +
                $"--output {thisDir.SurroundWithQuotes()} " +
                $"--executable {thisExe.SurroundWithQuotes()}");
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
