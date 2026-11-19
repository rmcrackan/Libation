using CommandLine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationCli.Options;

[Verb("set-setting", HelpText = "Set configuration values in Settings.json.")]
internal class SetSettingOptions : OptionsBase
{
	protected override bool EphemeralSettings => false;

	[Value(0, MetaName = "settings", Required = true, HelpText = "Settings to update in [SettingName]=\"Setting_Value\" format.")]
	public IEnumerable<OptionOverride> Settings { get; set; } = [];

	protected override Task ProcessAsync()
	{
		if (!ApplySettings(Settings))
			Environment.ExitCode = (int)ExitCode.RunTimeError;

		return Task.CompletedTask;
	}
}
