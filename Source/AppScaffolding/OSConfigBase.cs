using System;

namespace AppScaffolding
{
    public abstract class OSConfigBase
    {
        public abstract Type InteropFunctionsType { get; }
        public virtual Type[] ReferencedTypes { get; } = new Type[0];

        public void Run()
        {
            //Each of these types belongs to a different windows-only assembly that's needed by
            //the WinInterop methods. By referencing these types in main we force the runtime to
            //load their assemblies before execution reaches inside main. This allows the calling
            //process to find these assemblies in its module list.
            _ = ReferencedTypes;
            _ = InteropFunctionsType;

            //Wait for the calling process to be ready to read the WriteLine()
            Console.ReadLine();

            // Signal the calling process that execution has reached inside main, and that all referenced assemblies have been loaded.
            Console.WriteLine();

            // Wait for the calling process to finish reading the process module list, then exit.
            Console.ReadLine();
        }
    }
}
