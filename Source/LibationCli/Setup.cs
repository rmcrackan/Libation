﻿using AppScaffolding;
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
			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			var config = LibationScaffolding.RunPreConfigMigrations();

			LibationScaffolding.RunPostConfigMigrations(config);

#if classic
			LibationScaffolding.RunPostMigrationScaffolding(Variety.Classic, config);
#else
			LibationScaffolding.RunPostMigrationScaffolding(Variety.Chardonnay, config);
#endif
		}

		public static Type[] LoadVerbs() => Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => t.GetCustomAttribute<VerbAttribute>() is not null)
			.ToArray();
	}
}
