using Dinah.Core.Logging;
using FileManager;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Settings.Configuration;
using System;
using System.ComponentModel;

namespace LibationFileManager;

public partial class Configuration
{
	private IConfigurationRoot? configuration;

	public bool SerilogInitialized { get; private set; }

	public void ConfigureLogging()
	{
		//pass explicit assemblies to the ConfigurationReaderOptions
		//This is a workaround for the issue where serilog will try to load all
		//Assemblies starting with "serilog" from the app folder, but it will fail
		//if those assemblies are unreferenced.
		//This was a problem when migrating from the ZipFile sink to the File sink.
		//Upgrading users would still have the ZipFile sink dll in the program
		//folder and serilog would try to load it, unsuccessfully.
		//https://github.com/serilog/serilog-settings-configuration/issues/406
		var readerOptions = new ConfigurationReaderOptions(
			typeof(ILogger).Assembly,                                 // Serilog
			typeof(LoggerCallerEnrichmentConfiguration).Assembly,     // Dinah.Core
			typeof(LoggerEnrichmentConfigurationExtensions).Assembly, // Serilog.Exceptions
			typeof(ConsoleLoggerConfigurationExtensions).Assembly,    // Serilog.Sinks.Console
			typeof(FileLoggerConfigurationExtensions).Assembly);      // Serilog.Sinks.File

		configuration = new ConfigurationBuilder()
			.AddJsonFile(Instance.LibationFiles.SettingsFilePath, optional: false, reloadOnChange: true)
			.Build();
		Log.Logger = new LoggerConfiguration()
			 .ReadFrom.Configuration(configuration, readerOptions)
			 .Destructure.ByTransforming<LongPath>(lp => lp.Path)
			 .Destructure.With<LogFileFilter>()
			 .CreateLogger();
		SerilogInitialized = true;
	}

	[Description("The importance of a log event")]
	public LogEventLevel LogLevel
	{
		get
		{
			var logLevelStr = Settings.GetStringFromJsonPath("Serilog", "MinimumLevel");
			return Enum.TryParse<LogEventLevel>(logLevelStr, out var logLevelEnum) ? logLevelEnum : LogEventLevel.Information;
		}
		set
		{
			OnPropertyChanging(nameof(LogLevel), LogLevel, value);
			var valueWasChanged = Settings.SetWithJsonPath("Serilog", "MinimumLevel", value.ToString());
			if (!valueWasChanged)
			{
				Log.Logger.Debug("LogLevel.set attempt. No change");
				return;
			}

			configuration?.Reload();

			OnPropertyChanged(nameof(LogLevel), value);

			Log.Logger.Information("Updated LogLevel MinimumLevel. {@DebugInfo}", new
			{
				LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
				LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
				LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
				LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
				LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
				LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled()
			});
		}
	}
}
