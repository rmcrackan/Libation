using SixLabors.ImageSharp;
using System.Diagnostics;
using LibationFileManager;
using System.IO;
using System;

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
				var icon = Image.Load(File.ReadAllBytes(image)).ToIcon();
                iconPath = Path.Combine(directory, $"{Guid.NewGuid()}.ico");
                File.WriteAllBytes(iconPath, icon);

				new DirectoryInfo(directory)?.SetIcon(iconPath, "Music");
            }
            finally
            {
                if (File.Exists(iconPath))
                    File.Delete(iconPath);
            }
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

			RunAsRoot(zipExtractor, $"--input \"{upgradeBundle}\" --output \"{thisDir}\" --executable \"{thisExe}\"");
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
