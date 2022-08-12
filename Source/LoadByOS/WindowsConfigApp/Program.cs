using AppScaffolding.OSInterop;

namespace WindowsConfigApp
{
    class Program : OSConfigBase
    {
        public override Type InteropFunctionsType => typeof(WinInterop);
        public override Type[] ReferencedTypes => new Type[]
        {
            typeof(Form1),
            typeof(Bitmap),
            typeof(Accessibility.IAccIdentity),
            typeof(Microsoft.Win32.SystemEvents)
        };

        static void Main() => new Program().Run();
    }
}