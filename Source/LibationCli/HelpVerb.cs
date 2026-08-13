using AppScaffolding;
using CommandLine;
using CommandLine.Text;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibationCli;

[Verb("help", HelpText = "Display more information on a specific command.")]
internal class HelpVerb
{
	/// <summary>
	/// Name of the verb to get help about
	/// </summary>
	[Value(0, Default = "")]
	public string? HelpType { get; set; }

	/// <summary>
	/// Create a base <see cref="HelpText"/> for <see cref="LibationCli"/>
	/// </summary>
	public static HelpText CreateHelpText() => new HelpText
	{
		AutoVersion = false,
		AutoHelp = false,
		Heading = $"LibationCli v{LibationScaffolding.BuildVersion.ToVersionString()}",
		AdditionalNewLineAfterOption = true,
		MaximumDisplayWidth = 80
	};

	public static HelpText GetGroupHelpText(CliCommandGroup group)
	{
		var helpText = CreateHelpText();
		helpText.AddPreOptionsLine(group.HelpText);
		helpText.AddPreOptionsLine($"{group.Name} commands:");
		foreach (var subcommand in group.Subcommands)
			helpText.AddPreOptionsLine($"  {group.Name} {subcommand.Name}  {subcommand.HelpText}");
		return helpText;
	}

	/// <summary>
	/// Write the global verb list plus nested command groups (e.g. <c>abs</c>).
	/// Groups are appended after <see cref="HelpText"/> because CommandLineParser has no post-options slot.
	/// </summary>
	public static void WriteGlobalVerbList(
		TextWriter error,
		Type[] verbTypes,
		IReadOnlyList<CliCommandGroup> groups,
		string? preOptionsLine = null)
	{
		var helpText = CreateHelpText();
		if (preOptionsLine is not null)
			helpText.AddPreOptionsLine(preOptionsLine);
		helpText.AddVerbs(verbTypes);
		error.WriteLine(helpText);

		foreach (var group in groups)
			error.WriteLine($"  {group.Name,-21}{group.HelpText}");
	}

	/// <summary>
	/// Get the <see cref="HelpType"/>'s <see cref="HelpText"/>
	/// </summary>
	public HelpText GetHelpText()
	{
		var helpText = CreateHelpText();
		var result = new Parser().ParseArguments([HelpType], Program.VerbTypes);
		if (result.TypeInfo.Current == typeof(NullInstance))
		{
			//HelpType is not a defined verb so get LibationCli usage
			helpText.AddVerbs(Program.VerbTypes);
		}
		else
		{
			helpText.AutoHelp = true;
			helpText.AddDashesToOption = true;
			helpText.AddOptions(result);
		}
		return helpText;
	}
}
