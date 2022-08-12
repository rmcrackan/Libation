using AppScaffolding.OSInterop;

namespace MacOSConfigApp
{
    class Program : OSConfigBase
    {
        public override Type InteropFunctionsType => typeof(MacOSInterop);

        static void Main() => new Program().Run();
    }
}
