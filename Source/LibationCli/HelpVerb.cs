using AppScaffolding;
using CommandLine;
using CommandLine.Text;

namespace LibationCli;

[Verb("help", HelpText = "Display more information on a specific command.")]
internal class HelpVerb
{
	/// <summary>
	/// Name of the verb to get help about
	/// </summary>
	[Value(0, Default = "")]
	public string HelpType { get; set; }

	/// <summary>
	/// Create a base <see cref="HelpText"/> for <see cref="LibationCli"/>
	/// </summary>
	public static HelpText CreateHelpText() => new HelpText
	{
		AutoVersion = false,
		AutoHelp = false,
		Heading = $"LibationCli v{LibationScaffolding.BuildVersion.ToString(3)}",
		AdditionalNewLineAfterOption = true,
		MaximumDisplayWidth = 80
	};

	/// <summary>
	/// Get the <see cref="HelpType"/>'s <see cref="HelpText"/>
	/// </summary>
	public HelpText GetHelpText()
	{
		var helpText = CreateHelpText();
		var result = new Parser().ParseArguments(new string[] { HelpType }, Program.VerbTypes);
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
