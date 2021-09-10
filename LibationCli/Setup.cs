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


			LibationScaffolding.RunPostConfigMigrations();
			LibationScaffolding.RunPostMigrationScaffolding();

#if !DEBUG
			checkForUpdate();
#endif
		}

		private static void checkForUpdate()
		{
			var (hasUpgrade, zipUrl, htmlUrl, zipName) = LibationScaffolding.GetLatestRelease();
			if (!hasUpgrade)
				return;

			var origColor = Console.ForegroundColor;
			try
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"UPDATE AVAILABLE @ {zipUrl}");
			}
			finally
			{
				Console.ForegroundColor = origColor;
			}
		}

		public static void SubscribeToDatabaseEvents()
		{
			DataLayer.UserDefinedItem.ItemChanged += (sender, e) => ApplicationServices.LibraryCommands.UpdateUserDefinedItem(((DataLayer.UserDefinedItem)sender).Book);
		}

		public static Type[] LoadVerbs() => Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => t.GetCustomAttribute<VerbAttribute>() is not null)
			.ToArray();
	}
}
