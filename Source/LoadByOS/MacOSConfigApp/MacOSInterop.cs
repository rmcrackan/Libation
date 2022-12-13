using LibationFileManager;

namespace MacOSConfigApp
{
    internal class MacOSInterop : IInteropFunctions
    {
        public MacOSInterop() { }
        public MacOSInterop(params object[] values) { }

        public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
        public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();
        public void CopyTextToClipboard(string text) => throw new PlatformNotSupportedException();
    }
}
