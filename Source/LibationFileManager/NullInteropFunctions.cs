using System;
using System.Diagnostics;

#nullable enable

namespace LibationFileManager
{
    public class NullInteropFunctions : IInteropFunctions
    {

		public NullInteropFunctions() { }
        public NullInteropFunctions(params object[] values) { }

		public IWebViewAdapter? CreateWebViewAdapter() => throw new PlatformNotSupportedException();
		public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
        public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();
		public bool CanUpgrade => throw new PlatformNotSupportedException();
		public string ReleaseIdentifier => throw new PlatformNotSupportedException();
		public Process RunAsRoot(string exe, string args) => throw new PlatformNotSupportedException();
		public void InstallUpgrade(string updateBundle) => throw new PlatformNotSupportedException();
	}
}
