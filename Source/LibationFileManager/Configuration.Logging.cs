using Dinah.Core.Logging;
using Dinah.Core.Security;
using FileManager;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Settings.Configuration;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LibationFileManager;

public partial class Configuration
{
	private IConfigurationRoot? configuration;

	public bool SerilogInitialized { get; private set; }

	/// <summary>
	/// Size at which the log rolls to a new file. Deliberately well under GitHub's 25 MB attachment limit so
	/// the current log can always be attached to a bug report.
	/// </summary>
	public const long LogFileSizeLimitBytes = 10 * 1024 * 1024;

	/// <summary>
	/// How many log files to keep. With <see cref="LogFileSizeLimitBytes"/> this caps the logs at about 200 MB.
	/// </summary>
	public const int LogRetainedFileCountLimit = 20;

	/// <summary>
	/// Create default Serilog config if missing, and bring an existing one up to date: migrate the legacy
	/// ZipFile sink to File, attach <see cref="FileSinkHook"/>, and add size-based rolling.
	/// Must run before <see cref="ValidateSerilogConfiguration"/> / <see cref="ConfigureLogging"/>.
	/// </summary>
	public void EnsureSerilogConfig()
	{
		if (GetObject("Serilog") is JObject serilog)
		{
			bool fileChanged = false;
			foreach (var zipFileSink in serilog.SelectTokens("$.WriteTo[?(@.Name == 'ZipFile')]", false).OfType<JObject>())
			{
				zipFileSink["Name"] = "File";
				fileChanged = true;
			}
			var hooks = typeof(FileSinkHook).AssemblyQualifiedName;
			foreach (var fileSinkArgs in serilog.SelectTokens("$.WriteTo[?(@.Name == 'File')].Args", false).OfType<JObject>())
			{
				if (fileSinkArgs["hooks"]?.Value<string>() != hooks)
				{
					fileSinkArgs["hooks"] = hooks;
					fileChanged = true;
				}

				fileChanged |= AddSizeRollingArgs(fileSinkArgs);
			}

			if (fileChanged)
				SetNonString(serilog.DeepClone(), "Serilog");
			return;
		}

		var serilogObj = new JObject
		{
			{ "MinimumLevel", "Information" },
			{ "WriteTo", new JArray
				{
					// ABOUT SINKS
					// Only File sink is currently used. By user request (June 2024) others packages are included for experimental use.

					// new JObject { {"Name", "Console" } }, // this has caused more problems than it's solved
					new JObject
					{
						{ "Name", "File" },
						{ "Args", CreateDefaultFileSinkArgs() }
					}
				}
			},
			// better exception logging with: Serilog.Exceptions library -- WithExceptionDetails
			{ "Using", new JArray{ "Dinah.Core", "Serilog.Exceptions" } }, // dll's name, NOT namespace
			{ "Enrich", new JArray{ "WithCaller", "WithExceptionDetails" } },
		};
		SetNonString(serilogObj, "Serilog");
	}

	private JObject CreateDefaultFileSinkArgs()
	{
		var args = new JObject
		{
			// for this sink to work, a path must be provided. we override this below
			{ "path", Path.Combine(LibationFiles.Location, "Log.log") },
			{ "rollingInterval", "Month" },
			// Serilog template formatting examples
			// - default:                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
			//   output example:             2019-11-26 08:48:40.224 -05:00 [DBG] Begin Libation
			// - with class and method info: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}";
			//   output example:             2019-11-26 08:48:40.224 -05:00 [DBG] (at LibationWinForms.Program.init()) Begin Libation
			// {Properties:j} needed for expanded exception logging
			{ "outputTemplate", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception} {Properties:j}" },
			{ "hooks", typeof(FileSinkHook).AssemblyQualifiedName }, // for FileSinkHook
		};

		AddSizeRollingArgs(args);
		return args;
	}

	/// <summary>
	/// Adds the size-based rolling arguments a monthly rolling interval does not provide on its own.
	/// <para>
	/// Without these, Serilog's own defaults apply: one file per month, no size roll, and a 1 GB ceiling
	/// after which the sink silently stops writing. A busy install (many accounts, a scan every hour) can
	/// reach tens of MB in a month, past the point where the log can be attached to a bug report.
	/// </para>
	/// <para>
	/// Only absent keys are filled in, so a hand-tuned config is left alone.
	/// </para>
	/// </summary>
	/// <returns>True when something was added.</returns>
	private static bool AddSizeRollingArgs(JObject fileSinkArgs)
	{
		var changed = false;

		changed |= AddIfMissing("fileSizeLimitBytes", LogFileSizeLimitBytes);
		changed |= AddIfMissing("rollOnFileSizeLimit", true);
		changed |= AddIfMissing("retainedFileCountLimit", LogRetainedFileCountLimit);

		return changed;

		bool AddIfMissing(string name, JToken value)
		{
			if (fileSinkArgs[name] is not null)
				return false;

			fileSinkArgs[name] = value;
			return true;
		}
	}

	public void ConfigureLogging()
	{
		ValidateSerilogConfiguration();

		// Pass explicit assemblies to ConfigurationReaderOptions.
		// Workaround: Serilog otherwise loads all "Serilog*" assemblies from the app folder and fails
		// on unreferenced leftovers (e.g. ZipFile sink after migration).
		// https://github.com/serilog/serilog-settings-configuration/issues/406
		var readerOptions = new ConfigurationReaderOptions(
			typeof(ILogger).Assembly,                                 // Serilog
			typeof(LoggerCallerEnrichmentConfiguration).Assembly,     // Dinah.Core
			typeof(LoggerEnrichmentConfigurationExtensions).Assembly, // Serilog.Exceptions
			typeof(ConsoleLoggerConfigurationExtensions).Assembly,    // Serilog.Sinks.Console
			typeof(FileLoggerConfigurationExtensions).Assembly);      // Serilog.Sinks.File

		// Build from the in-memory settings store so ZipFile->File migration (and CLI ephemeral
		// settings) are what Serilog sees, not a stale disk copy.
		configuration = CreateLoggingConfigurationRoot();
		Log.Logger = new LoggerConfiguration()
			 .ReadFrom.Configuration(configuration, readerOptions)
			 .Destructure.ByTransforming<LongPath>(lp => lp.Path)
			 .Destructure.With<LogFileFilter>()
			 // last lines of defense for structured logging: a masked identity instead of the object, and a
			 // secret that renders as its shape instead of its contents
			 .Destructure.With<MaskedLogEntryPolicy>()
			 .Destructure.ByTransforming<SecretString>(secret => secret.ToString())
			 .CreateLogger();
		SerilogInitialized = true;
	}

	private IConfigurationRoot CreateLoggingConfigurationRoot()
	{
		var json = Settings.GetJObject().ToString(Formatting.None);
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
		return new ConfigurationBuilder()
			.AddJsonStream(stream)
			.Build();
	}

	/// <summary>
	/// Fail fast on structurally broken Serilog config (missing/empty WriteTo, bad MinimumLevel).
	/// Hand-edited custom sink names are allowed; legacy ZipFile is migrated by <see cref="EnsureSerilogConfig"/>.
	/// Call after ZipFile->File migration in scaffolding.
	/// </summary>
	public void ValidateSerilogConfiguration()
	{
		var settingsPath = LibationFiles.SettingsFilePath;
		if (GetObject("Serilog") is not JObject serilog)
		{
			throw InvalidConfigurationValueException.ForPath(
				"Serilog",
				null,
				$"Settings.json ({settingsPath}) is missing a Serilog section. " +
				"Add a Serilog configuration with at least one WriteTo sink (Libation defaults to File).");
		}

		ValidateSerilogMinimumLevel(serilog, settingsPath);
		ValidateSerilogWriteTo(serilog, settingsPath);
	}

	private static void ValidateSerilogMinimumLevel(JObject serilog, string settingsPath)
	{
		var minLevelToken = serilog["MinimumLevel"];
		if (minLevelToken is null)
			return;

		string? levelText = minLevelToken.Type switch
		{
			JTokenType.String => minLevelToken.Value<string>(),
			JTokenType.Object => minLevelToken["Default"]?.Value<string>(),
			_ => minLevelToken.ToString()
		};

		if (levelText is null)
			return;

		if (!Enum.TryParse<LogEventLevel>(levelText, ignoreCase: true, out _))
		{
			var allowed = string.Join(", ", Enum.GetNames<LogEventLevel>());
			throw InvalidConfigurationValueException.ForPath(
				"Serilog.MinimumLevel",
				levelText,
				$"Invalid value for 'Serilog.MinimumLevel' in Settings.json ({settingsPath}): " +
				$"{InvalidConfigurationValueException.FormatValue(levelText)}. Expected one of: {allowed}.");
		}
	}

	private static void ValidateSerilogWriteTo(JObject serilog, string settingsPath)
	{
		var writeToToken = serilog["WriteTo"];
		if (writeToToken is null)
		{
			throw InvalidConfigurationValueException.ForPath(
				"Serilog.WriteTo",
				null,
				$"Settings.json ({settingsPath}) Serilog section has no WriteTo sinks. " +
				"Add at least one WriteTo sink (Libation defaults to File).");
		}

		if (writeToToken is not JArray writeTo)
		{
			throw InvalidConfigurationValueException.ForPath(
				"Serilog.WriteTo",
				writeToToken.Type.ToString(),
				$"Settings.json ({settingsPath}) 'Serilog.WriteTo' must be a JSON array of sink objects.");
		}

		if (writeTo.Count == 0)
		{
			throw InvalidConfigurationValueException.ForPath(
				"Serilog.WriteTo",
				"[]",
				$"Settings.json ({settingsPath}) Serilog.WriteTo is empty. " +
				"Add at least one WriteTo sink (Libation defaults to File).");
		}

		for (var i = 0; i < writeTo.Count; i++)
		{
			var path = $"Serilog.WriteTo[{i}]";
			if (writeTo[i] is not JObject sink)
			{
				throw InvalidConfigurationValueException.ForPath(
					path,
					writeTo[i]?.Type.ToString(),
					$"Settings.json ({settingsPath}) '{path}' must be a JSON object with a Name property.");
			}

			var name = sink["Name"]?.Value<string>();
			var namePath = $"{path}.Name";
			if (string.IsNullOrWhiteSpace(name))
			{
				throw InvalidConfigurationValueException.ForPath(
					namePath,
					name,
					$"Settings.json ({settingsPath}) '{namePath}' is missing or empty.");
			}
		}
	}

	/// <summary>
	/// Force-read enum-backed settings so invalid values fail at startup instead of later.
	/// </summary>
	public void ValidateEnumSettings()
	{
		try
		{
			_ = ThemeVariant;
			_ = MaxSampleRate;
			_ = LameEncoderQuality;
			_ = ClipsBookmarksFileFormat;
			_ = TokenStorageMethod;
			_ = SpatialAudioCodec;
			_ = FileDownloadQuality;
			_ = CreationTime;
			_ = LastWriteTime;
			_ = BadBook;
			_ = DailyDownloadLimit;
			_ = DailyDownloadLimitUnit;
		}
		catch (InvalidConfigurationValueException ex)
		{
			throw EnhanceWithSettingsPath(ex);
		}
	}

	private InvalidConfigurationValueException EnhanceWithSettingsPath(InvalidConfigurationValueException ex)
	{
		var path = LibationFiles.SettingsFilePath;
		if (ex.Message.Contains(path, StringComparison.OrdinalIgnoreCase))
			return ex;

		return new InvalidConfigurationValueException(
			ex.PropertyPath,
			ex.InvalidValue,
			ex.ExpectedType,
			$"Settings.json ({path}): {ex.Message}");
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

			if (SerilogInitialized)
			{
				// Rebuild from current settings (in-memory or disk) so MinimumLevel applies.
				ConfigureLogging();
			}

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
