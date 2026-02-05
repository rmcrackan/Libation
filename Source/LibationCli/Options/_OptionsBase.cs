using CommandLine;
using Dinah.Core;
using FileManager;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LibationCli
{
	public abstract class OptionsBase
	{
		[Option(longName: "libationFiles", HelpText = "Path to Libation Files directory")]
		public DirectoryInfo? LibationFiles { get; set; }

		[Option('o', "override",  HelpText = "Configuration setting override [SettingName]=\"Setting_Value\"")]
		public IEnumerable<OptionOverride>? SettingOverrides { get; set; }

		public async Task Run()
		{
			if (LibationFiles?.Exists is true)
			{
				Environment.SetEnvironmentVariable(LibationFileManager.LibationFiles.LIBATION_FILES_DIR, LibationFiles.FullName);
			}

			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			Setup.Initialize();

			if (SettingOverrides is not null)
				ProcessSettingsOverrides();

			try
			{
				await ProcessAsync();
			}
			catch (Exception ex)
			{
				Environment.ExitCode = (int)ExitCode.RunTimeError;
				PrintVerbUsage(
					"ERROR",
					"=====",
					ex.Message,
					"",
					ex.StackTrace);
			}
		}

		private static bool TryParseEnum(Type enumType, string? value, out object? result)
		{
			var values = Enum.GetNames(enumType);

			if (values.Select(n => n.ToLowerInvariant()).Distinct().Count() != values.Length)
			{
				//Enum names must be case sensitive.
				return Enum.TryParse(enumType, value, out result);
			}

			for (int i = 0; i < values.Length; i++)
			{
				if (values[i].Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return Enum.TryParse(enumType, values[i], out result);
				}
			}
			result = null;
			return false;
		}

		protected void PrintVerbUsage(params string?[] linesBeforeUsage)
		{
			var verb = GetType().GetCustomAttribute<VerbAttribute>()?.Name;
			var helpText = new HelpVerb { HelpType = verb }.GetHelpText();
			helpText.AddPreOptionsLines(linesBeforeUsage);
			helpText.AddPreOptionsLine("");
			helpText.AddPreOptionsLine($"{verb} Usage:");
			Console.Error.WriteLine(helpText);
		}

		protected static void ReplaceConsoleText(TextWriter writer, int previousLength, string newText)
		{
			writer.Write(new string('\b', previousLength));
			writer.Write(newText);
			writer.Write(new string(' ', int.Max(0, previousLength - newText.Length)));
		}

		protected abstract Task ProcessAsync();

		protected IOrderedEnumerable<PropertyInfo> GetConfigurationProperties()
			=> typeof(Configuration).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DescriptionAttribute)))
			.Where(p => !p.Name.In(ExcludedSettings))
			.OrderBy(p => p.PropertyType.IsEnum)
			.ThenBy(p => p.PropertyType.Name)
			.ThenBy(p => p.Name);

		private readonly string[] ExcludedSettings = [
			nameof(Configuration.LibationFiles),
			nameof(Configuration.GridScaleFactor),
			nameof(Configuration.GridFontScaleFactor),
			nameof(Configuration.GridColumnsVisibilities),
			nameof(Configuration.GridColumnsDisplayIndices),
			nameof(Configuration.GridColumnsWidths)];

		private void ProcessSettingsOverrides()
		{
			var configProperties = GetConfigurationProperties().ToArray();
			foreach (var option in SettingOverrides?.Where(p => p.Property is not null && p.Value is not null) ?? [])
			{
				if (option.Property?.StartsWithInsensitive(ReplacePrefix) is true)
				{
					OverrideReplacement(option);
				}
				else if (configProperties.FirstOrDefault(p => p.Name.EqualsInsensitive(option.Property)) is not PropertyInfo property)
				{
					Console.Error.WriteLine($"Unknown configuration property '{option.Property}'");
				}
				else if (property.PropertyType == typeof(string))
				{
					property.SetValue(Configuration.Instance, option.Value?.Trim());
				}
				else if (property.PropertyType == typeof(bool) && bool.TryParse(option.Value?.Trim(), out var bVal))
				{
					property.SetValue(Configuration.Instance, bVal);
				}
				else if (property.PropertyType == typeof(int) && int.TryParse(option.Value?.Trim(), out var intVal))
				{
					property.SetValue(Configuration.Instance, intVal);
				}
				else if (property.PropertyType == typeof(long) && long.TryParse(option.Value?.Trim(), out var longVal))
				{
					property.SetValue(Configuration.Instance, longVal);
				}
				else if (property.PropertyType == typeof(LongPath))
				{
					var value = option.Value is null ? null : (LongPath)option.Value.Trim();
					property.SetValue(Configuration.Instance, value);
				}
				else if (property.PropertyType.IsEnum && TryParseEnum(property.PropertyType, option.Value?.Trim(), out var enumVal))
				{
					property.SetValue(Configuration.Instance, enumVal);
				}
				else
				{
					Console.Error.WriteLine($"Cannot set configuration property '{property.Name}' of type '{property.PropertyType}' with value '{option.Value}'");
				}
			}
		}

		private static void OverrideReplacement(OptionOverride option)
		{
			List<Replacement> newReplacements = [];

			bool addedToList = false;
			foreach (var r in Configuration.Instance.ReplacementCharacters.Replacements)
			{
				if (GetReplacementName(r).EqualsInsensitive(option.Property))
				{
					var newReplacement = new Replacement(r.CharacterToReplace, option.Value ?? string.Empty, r.Description)
					{
						Mandatory = r.Mandatory
					};
					newReplacements.Add(newReplacement);
					addedToList = true;
				}
				else
				{
					newReplacements.Add(r);
				}
			}

			if (!addedToList)
			{
				var charToReplace = option.Property!.Substring(ReplacePrefix.Length);
				if (charToReplace.Length != 1)
				{
					Console.Error.WriteLine($"Invalid character to replace: '{charToReplace}'");
				}
				else
				{
					newReplacements.Add(new(charToReplace[0], option.Value ?? string.Empty, ""));
				}
			}
			Configuration.Instance.ReplacementCharacters = new ReplacementCharacters { Replacements = newReplacements };
		}

		const string ReplacePrefix = "Replace_";
		protected static string GetReplacementName(Replacement r)
			=> !r.Mandatory ? ReplacePrefix + r.CharacterToReplace
			: r.CharacterToReplace == '\0' ? ReplacePrefix + "OtherInvalid"
			: r.CharacterToReplace == '/' ? ReplacePrefix + "Slash"
			: r.CharacterToReplace == '\\' ? ReplacePrefix + "BackSlash"
			: r.Description == "Open Quote" ? ReplacePrefix + "OpenQuote"
			: r.Description == "Close Quote" ? ReplacePrefix + "CloseQuote"
			: r.Description == "Other Quote" ? ReplacePrefix + "OtherQuote"
			: ReplacePrefix + r.Description.Replace(" ", "");

		public class OptionOverride
		{
			public string? Property { get; }
			public string? Value { get; }

			public OptionOverride(string value)
			{
				if (value is null)
					return;
				
				//Special case of Replace_=  settings
				var start
					= value.StartsWithInsensitive(ReplacePrefix + "=")
					? value.IndexOf('=', ReplacePrefix.Length + 1)
					: value.IndexOf('=');

				if (start < 1)
					return;
				Property = value[..start];

				//Don't trim here. Trim before parsing the value if needed, otherwise
				//preserve for settings which utilize white space (e.g. Replacements)
				Value = value[(start + 1)..];

				if (Value.StartsWith('"') && Value.EndsWith('"'))
				{
					Value = Value[1..];
				}

				if (Value.EndsWith('"'))
				{
					Value = Value[..^1];
				}
			}
		}
	}
}
