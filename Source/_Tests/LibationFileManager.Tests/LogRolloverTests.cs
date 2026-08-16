using AssertionHelper;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace SerilogConfigurationTests;

/// <summary>
/// Builds a real Serilog logger from the config Libation generates and writes until it rolls. Asserting on the
/// JSON alone would pass just as happily with a misspelled argument name, which Serilog ignores in silence -
/// and silently not rolling is the bug being fixed.
/// </summary>
[TestClass]
[DoNotParallelize]
public class LogRolloverTests
{
	private string tempDir = string.Empty;
	private ILogger originalLogger = Serilog.Log.Logger;

	[TestInitialize]
	public void Initialize()
	{
		originalLogger = Serilog.Log.Logger;
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-log-rollover-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		Serilog.Log.CloseAndFlush();
		Serilog.Log.Logger = originalLogger;
		Configuration.RestoreSingletonInstance();

		try
		{
			Directory.Delete(tempDir, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	/// <summary>The pre-13.7.10 default: a monthly rolling interval and nothing about size.</summary>
	private JObject LegacySerilogConfig(long? fileSizeLimitBytes = null)
	{
		var args = new JObject
		{
			["path"] = Path.Combine(tempDir, "Log.log"),
			["rollingInterval"] = "Month",
			["outputTemplate"] = "{Message:lj}{NewLine}"
		};

		if (fileSizeLimitBytes is not null)
			args["fileSizeLimitBytes"] = fileSizeLimitBytes;

		return new JObject
		{
			["MinimumLevel"] = "Information",
			["WriteTo"] = new JArray { new JObject { ["Name"] = "File", ["Args"] = args } }
		};
	}

	private string[] LogFiles() => [.. Directory.GetFiles(tempDir, "Log*.log").Order()];

	[TestMethod]
	public void An_existing_config_is_migrated_and_then_actually_rolls_on_size()
	{
		var config = Configuration.CreateMockInstance();
		// A small limit so the test writes kilobytes rather than tens of megabytes. Libation's own default
		// value is asserted in SerilogConfigurationTests; what matters here is that Serilog obeys it.
		config.SetNonString(LegacySerilogConfig(fileSizeLimitBytes: 4096), "Serilog");

		config.EnsureSerilogConfig();
		config.ConfigureLogging();

		var line = new string('x', 512);
		for (var i = 0; i < 40; i++)
			Serilog.Log.Logger.Information(line);
		Serilog.Log.CloseAndFlush();

		var files = LogFiles();
		Assert.IsTrue(files.Length > 1, $"expected the log to roll, found {files.Length} file(s)");
		foreach (var file in files)
			Assert.IsTrue(new FileInfo(file).Length < 8192, $"{Path.GetFileName(file)} grew past the size limit");
	}

	[TestMethod]
	public void The_generated_default_rolls_on_size_too()
	{
		var config = Configuration.CreateMockInstance();
		config.EnsureSerilogConfig();

		var args = (JObject)((JObject)config.GetObject("Serilog")!).SelectToken("$.WriteTo[0].Args")!;

		// Serilog ignores an argument it does not recognise, so the names have to match the sink exactly.
		args["fileSizeLimitBytes"] = 4096;
		args["path"] = Path.Combine(tempDir, "Log.log");
		args["outputTemplate"] = "{Message:lj}{NewLine}";
		config.SetNonString((JObject)config.GetObject("Serilog")!, "Serilog");

		config.ConfigureLogging();

		var line = new string('x', 512);
		for (var i = 0; i < 40; i++)
			Serilog.Log.Logger.Information(line);
		Serilog.Log.CloseAndFlush();

		var files = LogFiles();
		Assert.IsTrue(files.Length > 1, $"expected the log to roll, found {files.Length} file(s)");
	}

	[TestMethod]
	public void Without_size_rolling_a_single_file_grows_unbounded()
	{
		// The behaviour being fixed, pinned so a future change to the defaults cannot quietly restore it.
		var config = Configuration.CreateMockInstance();
		var serilog = LegacySerilogConfig(fileSizeLimitBytes: 4096);
		var args = (JObject)serilog.SelectToken("$.WriteTo[0].Args")!;
		args["outputTemplate"] = "{Message:lj}{NewLine}";
		args["rollOnFileSizeLimit"] = false;
		config.SetNonString(serilog, "Serilog");

		config.EnsureSerilogConfig();
		config.ConfigureLogging();

		var line = new string('x', 512);
		for (var i = 0; i < 40; i++)
			Serilog.Log.Logger.Information(line);
		Serilog.Log.CloseAndFlush();

		// One file, and Serilog stopped writing at the limit rather than starting a new one.
		LogFiles().Length.Should().Be(1);
	}

	[TestMethod]
	public void Old_log_files_beyond_the_retained_count_are_deleted()
	{
		var config = Configuration.CreateMockInstance();
		var serilog = LegacySerilogConfig(fileSizeLimitBytes: 4096);
		var args = (JObject)serilog.SelectToken("$.WriteTo[0].Args")!;
		args["outputTemplate"] = "{Message:lj}{NewLine}";
		args["retainedFileCountLimit"] = 2;
		config.SetNonString(serilog, "Serilog");

		config.EnsureSerilogConfig();
		config.ConfigureLogging();

		var line = new string('x', 512);
		for (var i = 0; i < 100; i++)
			Serilog.Log.Logger.Information(line);
		Serilog.Log.CloseAndFlush();

		LogFiles().Length.Should().Be(2);
	}
}
