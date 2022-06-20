using System;
using System.Linq;

namespace FileManager
{
	public class MetadataNamingTemplate : NamingTemplate
	{
		public MetadataNamingTemplate(string template) : base(template) { }

		public string GetTagContents()
		{
			var tagValue = Template;

			foreach (var r in ParameterReplacements)
				tagValue = tagValue.Replace($"<{formatKey(r.Key)}>", r.Value?.ToString() ?? "");

			return tagValue;
		}
	}
}
