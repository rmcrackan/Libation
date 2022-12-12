using LibationFileManager;

namespace LinuxConfigApp
{
    internal class LinuxInterop : IInteropFunctions
    {
        public LinuxInterop() { }
        public LinuxInterop(params object[] values) { }

        public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
        public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();
        public void CopyTextToClipboard(string text) => throw new PlatformNotSupportedException();
    }
}
