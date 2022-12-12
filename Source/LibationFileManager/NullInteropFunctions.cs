using System;

namespace LibationFileManager
{
    public class NullInteropFunctions : IInteropFunctions
    {
        public NullInteropFunctions() { }
        public NullInteropFunctions(params object[] values) { }

        public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
        public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();
        public void CopyTextToClipboard(string text) => throw new PlatformNotSupportedException();
    }
}
