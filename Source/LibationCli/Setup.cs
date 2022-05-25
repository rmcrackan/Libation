using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AppScaffolding;
using CommandLine;
using CommandLine.Text;
using Dinah.Core;
using Dinah.Core.Collections;
using Dinah.Core.Collections.Generic;

namespace LibationCli
{
	public static class Setup
	{
		public static void Initialize()
		{
			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			var config = LibationScaffolding.RunPreConfigMigrations();


			LibationScaffolding.RunPostConfigMigrations(config);
			LibationScaffolding.RunPostMigrationScaffolding(config);

#if !DEBUG
			checkForUpdate();
#endif
		}

		private static void checkForUpdate()
		{
			var upgradeProperties = LibationScaffolding.GetLatestRelease();
			if (upgradeProperties is null)
				return;

			var origColor = Console.ForegroundColor;
			try
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"UPDATE AVAILABLE @ {upgradeProperties.ZipUrl}");
			}
			finally
			{
				Console.ForegroundColor = origColor;
			}
		}

		public static Type[] LoadVerbs() => Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => t.GetCustomAttribute<VerbAttribute>() is not null)
			.ToArray();
	}
}
