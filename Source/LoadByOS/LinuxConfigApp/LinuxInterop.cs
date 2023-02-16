using LibationFileManager;
using System.Diagnostics;

namespace LinuxConfigApp
{
    internal class LinuxInterop : IInteropFunctions
    {
		//Different terminal apps possibly installed on a linux system
		// [0] console executable
		// [1] argument to set the concole's title
		// [2] argument to pass a command to be executed to the terminal
		static readonly string[][] consoleCommands =
        {
	        new[] {"konsole", "--title", "-e"},
	        new[] {"gnome-terminal", "--title", "--"},
	        new[] {"mate-terminal", "--title", "-x"},
	        new[] {"xterm", "-T", "-e"},
        };

		public LinuxInterop() { }
        public LinuxInterop(params object[] values) { }

        public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
        public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();

        //only run the audo updater is the current app was installed from the
        //.deb package. Try to detect this by checking if the symlink exists.
        public bool CanUpdate => Directory.Exists("/usr/lib/libation");
        public void InstallUpdate(string updateBundle)
		{
			RunAsRoot("apt", $"install '{updateBundle}'");
		}

		public Process RunAsRoot(string exe, string args)
        {
			//cribbed this script from VirtualBox's guest additions installer.
			//It's designed to launch the system's gui superuser password
			//prompt across multiple distributions and desktop environments.
			const string runasroot = "/tmp/runasroot.sh";
			File.WriteAllBytes(runasroot, Properties.Resources.runasroot);

			string command = $"{exe ?? ""} {args ?? ""}".Trim();

			foreach (var console in consoleCommands)
			{
				ProcessStartInfo psi = new()
				{
					FileName = console[0],
					UseShellExecute = false,
					ArgumentList =
					{
						console[1],
						$"Running '{exe}' as root",
						console[2],
						"/bin/sh",
						runasroot,
						"Installing libation.deb",
						command,
						$"Please run '{command}' manually"
					}
				};


				try
				{
					return Process.Start(psi);
				}
				catch { }
			}
			return null;
		}
	}
}
