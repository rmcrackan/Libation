using System;
using System.Diagnostics;

namespace LibationFileManager
{
    public class NullInteropFunctions : IInteropFunctions
    {

		public NullInteropFunctions() { }
        public NullInteropFunctions(params object[] values) { }

        public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
        public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();
		public bool CanUpdate => throw new PlatformNotSupportedException();
		public Process RunAsRoot(string exe, string args) => throw new PlatformNotSupportedException();
		public void InstallUpdate(string updateBundle) => throw new PlatformNotSupportedException();
	}
}
