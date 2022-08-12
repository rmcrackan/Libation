using AppScaffolding.OSInterop;

namespace WindowsConfigApp
{
    internal class WinInterop : IInteropFunctions
    {
        public WinInterop() { }
        public WinInterop(params object[] values) { }


        // examples until the real interface is filled out
        private string InitValue1 { get; }
        private int InitValue2 { get; }

        public WinInterop(string initValue1, int initValue2)
        {
            InitValue1 = initValue1;
            InitValue2 = initValue2;
        }

        public void CopyTextToClipboard(string text) => Clipboard.SetDataObject(text, true, 5, 150);

        public void ShowForm()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        public string TransformInit1() => InitValue1.ToUpper();

        public int TransformInit2() => InitValue2 * InitValue2;
    }
}
