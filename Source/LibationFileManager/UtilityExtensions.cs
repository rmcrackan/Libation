using System;
using System.Collections.Generic;
using System.Linq;
using FileManager;

namespace LibationFileManager
{
	public static class UtilityExtensions
	{
		public static void AddParameterReplacement(this FileNamingTemplate fileNamingTemplate, TemplateTags templateTags, object value)
			=> fileNamingTemplate.AddParameterReplacement(templateTags.TagName, value);
	}
}
