using System;
using System.Collections.Generic;
using System.Linq;
using FileManager;

namespace LibationFileManager
{
	public static class UtilityExtensions
	{
		public static void AddParameterReplacement(this NamingTemplate fileNamingTemplate, TemplateTags templateTags, object value)
			=> fileNamingTemplate.AddParameterReplacement(templateTags.TagName, value);

		public static void AddUniqueParameterReplacement(this NamingTemplate namingTemplate, string key, object value)
			=> namingTemplate.ParameterReplacements[key] = value;
	}
}
