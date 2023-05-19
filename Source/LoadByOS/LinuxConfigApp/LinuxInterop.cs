using AppScaffolding;
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

		public IWebViewAdapter CreateWebViewAdapter() => null;
		public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
		public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();

		public string ReleaseIdString => LibationScaffolding.ReleaseIdentifier.ToString() + (File.Exists("/bin/yum") ? "_RPM" : "");

		//only run the auto upgrader if the current app was installed from the
		//.deb or .rpm package. Try to detect this by checking if the symlink exists.
		public bool CanUpgrade => File.Exists("/bin/libation");
		public void InstallUpgrade(string upgradeBundle)
		{
			if (File.Exists("/bin/yum"))
				RunAsRoot("yum", $"install -y '{upgradeBundle}'");
			else
				RunAsRoot("apt", $"install '{upgradeBundle}'");
		}

		public Process RunAsRoot(string exe, string args)
		{
			//cribbed this script from VirtualBox's guest additions installer.
			//It's designed to launch the system's gui superuser password
			//prompt across multiple distributions and desktop environments.
			const string runasroot = "runasroot.sh";

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
						$"Running '{exe}' as root", // console title
						console[2],
						"/bin/sh",
						Path.Combine(Configuration.ProcessDirectory, runasroot), //script file
						"Installing libation.deb", //command title
						command, // command to execute vis /bin/sh
						$"Please run '{command}' manually" // error message to display in the terminal
					}
				};

				try
				{
					return Process.Start(psi);
				}
				catch { }
			}
			throw new PlatformNotSupportedException($"Could not start any of the supported terminals: {string.Join(", ", consoleCommands.Select(c => c[0]))}");
		}
	}
}
