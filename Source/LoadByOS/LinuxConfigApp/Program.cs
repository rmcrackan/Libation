using AppScaffolding.OSInterop;

namespace LinuxConfigApp
{
    class Program : OSConfigBase
    {
        public override Type InteropFunctionsType => typeof(LinuxInterop);

        static void Main() => new Program().Run();
    }
}
