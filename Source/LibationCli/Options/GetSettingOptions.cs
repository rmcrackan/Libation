using CommandLine;
using Dinah.Core;
using FileManager;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LibationCli.Options;

[Verb("get-setting", HelpText = "List current settings files and their locations.")]
internal class GetSettingOptions : OptionsBase
{
	[Option('l', "listEnumValues", HelpText = "List all value possibilities of enum types")]
	public bool ListEnumValues { get; set; }

	[Option('b', "bare", HelpText = "Print bare list without table decoration")]
	public bool Bare { get; set; }

	[Value(0, MetaName = "[setting names]", HelpText = "Optional names of settings to get.")]
	public IEnumerable<string>? SettingNames { get; set; }

	protected override Task ProcessAsync()
	{
		var configs = GetConfigOptions();
		if (SettingNames?.Any() is true)
		{
			//Operate over listed settings
			foreach (var item in SettingNames.ExceptBy(configs.Select(c => c.Name), c => c, StringComparer.OrdinalIgnoreCase))
			{
				Console.Error.WriteLine($"Unknown Setting Name: {item}");
			}

			var validSettings = configs.IntersectBy(SettingNames, a => a.Name, StringComparer.OrdinalIgnoreCase);
			if (ListEnumValues)
			{
				foreach (var item in validSettings.Where(s => !s.SettingType.IsEnum))
				{
					Console.Error.WriteLine($"Setting '{item.Name}' is not an enum type");
				}

				PrintEnumValues(validSettings.Where(s => s.SettingType.IsEnum));
			}
			else
			{
				PrintConfigOption(validSettings);
			}
		}
		else
		{
			//Operate over all settings
			if (ListEnumValues)
			{
				PrintEnumValues(configs);
			}
			else
			{
				PrintConfigOption(configs);
			}
		}
		return Task.CompletedTask;
	}

	private void PrintConfigOption(IEnumerable<ConfigOption> options)
	{
		if (Bare)
		{
			foreach (var option in options)
			{
				Console.WriteLine($"{option.Name}={option.Value}");
			}
		}
		else
		{
			Console.Out.DrawTable(options, new(), o => o.Name, o => o.Value, o => o.Type);
		}
	}

	private void PrintEnumValues(IEnumerable<ConfigOption> options)
	{
		foreach (var item in options.Where(s => s.SettingType.IsEnum))
		{
			var enumValues = Enum.GetNames(item.SettingType);
			if (Bare)
			{
				Console.WriteLine(string.Join(Environment.NewLine, enumValues.Select(e => $"{item.Name}.{e}")));
			}
			else
			{
				Console.Out.DrawTable(enumValues, new TextTableOptions(), new ColumnDef<string>(item.Name, t => t));
			}
		}
	}

	private ConfigOption[] GetConfigOptions()
	{
		var configs = GetConfigurationProperties().Where(o => o.PropertyType != typeof(ReplacementCharacters)).Select(p => new ConfigOption(p));
		var replacements = GetConfigurationProperties().SingleOrDefault(o => o.PropertyType == typeof(ReplacementCharacters))?.GetValue(Configuration.Instance) as ReplacementCharacters;

		if (replacements is not null)
		{
			//Don't reorder after concat to keep replacements grouped together at the bottom
			configs = configs.Concat(replacements.Replacements.Select(r => new ConfigOption(r)));
		}

		return configs.ToArray();
	}

	private record EnumOption(string EnumOptionValue);
	private record ConfigOption
	{
		public string Name { get; }
		public string Type { get; }
		public Type SettingType { get; }
		public string Value { get; }
		public ConfigOption(PropertyInfo propertyInfo)
		{
			Name = propertyInfo.Name;
			SettingType = propertyInfo.PropertyType;
			Type = GetTypeString(SettingType);
			Value = propertyInfo.GetValue(Configuration.Instance)?.ToString() is not string value ? "[null]"
				: SettingType == typeof(string) || SettingType == typeof(LongPath) ? value.SurroundWithQuotes()
				: value;
		}

		public ConfigOption(Replacement replacement)
		{
			Name = GetReplacementName(replacement);
			SettingType = typeof(string);
			Type = GetTypeString(SettingType);
			Value = replacement.ReplacementString.SurroundWithQuotes();
		}

		private static string GetTypeString(Type type)
		=> type.IsEnum ? $"{type.Name} (enum)" : type.Name;
	}
}
