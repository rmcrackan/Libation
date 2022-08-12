using System;

namespace AppScaffolding.OSInterop
{
    internal class NullInteropFunctions : IInteropFunctions
    {
        public NullInteropFunctions() { }
        public NullInteropFunctions(params object[] values) { }

        // examples until the real interface is filled out
        public string TransformInit1() => throw new PlatformNotSupportedException();
        public int TransformInit2() => throw new PlatformNotSupportedException();
        public void CopyTextToClipboard(string text) => throw new PlatformNotSupportedException();
        public void ShowForm() => throw new PlatformNotSupportedException();
    }
}
