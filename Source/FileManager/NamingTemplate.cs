using Dinah.Core;
using System;
using System.Collections.Generic;

namespace FileManager
{
	public class NamingTemplate
	{
		/// <summary>Proposed full name. May contain optional html-styled template tags. Eg: &lt;name&gt;</summary>
		public string Template { get; }

		/// <param name="template">Proposed file name with optional html-styled template tags.</param>
		public NamingTemplate(string template) => Template = ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));

		/// <summary>Optional step 1: Replace html-styled template tags with parameters. Eg {"name", "Bill Gates"} => /&lt;name&gt;/ => /Bill Gates/</summary>
		public Dictionary<string, object> ParameterReplacements { get; } = new Dictionary<string, object>();

		/// <summary>Convenience method</summary>
		public void AddParameterReplacement(string key, object value)
			// using .Add() instead of "[key] = value" will make unintended overwriting throw exception
			=> ParameterReplacements.Add(key, value);

		protected static string formatKey(string key)
			=> key
			.Replace("<", "")
			.Replace(">", "");
	}
}
