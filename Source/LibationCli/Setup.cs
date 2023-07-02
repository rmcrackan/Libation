using AppScaffolding;
using CommandLine;
using System;
using System.Linq;
using System.Reflection;

namespace LibationCli
{
	public static class Setup
	{
		public static void Initialize()
		{
			//Determine variety by the dlls present in the current directory.
			//Necessary to be able to check for upgrades.
			var variety = System.IO.File.Exists("System.Windows.Forms.dll") ? Variety.Classic : Variety.Chardonnay;
			LibationScaffolding.SetReleaseIdentifier(variety);

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			var config = LibationScaffolding.RunPreConfigMigrations();


			LibationScaffolding.RunPostConfigMigrations(config);
			LibationScaffolding.RunPostMigrationScaffolding(config);
		}

		public static Type[] LoadVerbs() => Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => t.GetCustomAttribute<VerbAttribute>() is not null)
			.ToArray();
	}
}
