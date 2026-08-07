using AssertionHelper;
using FileManager;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace SerilogConfigurationTests;

[TestClass]
[DoNotParallelize]
public class SerilogConfigurationTests
{
	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
	}

	[TestMethod]
	public void Validate_accepts_File_sink()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(CreateSerilog("File"), "Serilog");

		config.ValidateSerilogConfiguration();
	}

	[TestMethod]
	public void Validate_accepts_Console_sink()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(CreateSerilog("Console"), "Serilog");

		config.ValidateSerilogConfiguration();
	}

	[TestMethod]
	public void Validate_accepts_hand_edited_custom_sink_name()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(CreateSerilog("Seq"), "Serilog");

		config.ValidateSerilogConfiguration();
	}

	[TestMethod]
	public void EnsureSerilogConfig_migrates_ZipFile_to_File()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(CreateSerilog("ZipFile"), "Serilog");

		config.EnsureSerilogConfig();

		var serilog = (JObject)config.GetObject("Serilog")!;
		var name = serilog.SelectToken("$.WriteTo[0].Name")?.Value<string>();
		Assert.AreEqual("File", name);

		config.ValidateSerilogConfiguration();
	}

	[TestMethod]
	public void EnsureSerilogConfig_migrates_all_ZipFile_sinks()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(new JObject
		{
			["MinimumLevel"] = "Information",
			["WriteTo"] = new JArray
			{
				new JObject { ["Name"] = "ZipFile", ["Args"] = new JObject { ["path"] = "a.log" } },
				new JObject { ["Name"] = "Console" },
				new JObject { ["Name"] = "ZipFile", ["Args"] = new JObject { ["path"] = "b.log" } },
			}
		}, "Serilog");

		config.EnsureSerilogConfig();

		var serilog = (JObject)config.GetObject("Serilog")!;
		var names = serilog.SelectTokens("$.WriteTo[*].Name").Select(t => t.Value<string>()).ToList();
		CollectionAssert.AreEqual(new[] { "File", "Console", "File" }, names);

		config.ValidateSerilogConfiguration();
	}

	[TestMethod]
	public void Validate_rejects_invalid_MinimumLevel()
	{
		var config = Configuration.CreateMockInstance();
		var serilog = CreateSerilog("File");
		serilog["MinimumLevel"] = "Loud";
		config.SetNonString(serilog, "Serilog");

		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(config.ValidateSerilogConfiguration);
		StringAssert.Contains(ex.Message, "MinimumLevel");
		StringAssert.Contains(ex.Message, "Loud");
	}

	[TestMethod]
	public void Validate_rejects_malformed_WriteTo()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(new JObject
		{
			["MinimumLevel"] = "Information",
			["WriteTo"] = "File"
		}, "Serilog");

		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(config.ValidateSerilogConfiguration);
		StringAssert.Contains(ex.Message, "WriteTo");
	}

	[TestMethod]
	public void Validate_rejects_empty_WriteTo()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(new JObject
		{
			["MinimumLevel"] = "Information",
			["WriteTo"] = new JArray()
		}, "Serilog");

		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(config.ValidateSerilogConfiguration);
		StringAssert.Contains(ex.Message, "empty");
	}

	[TestMethod]
	public void Validate_rejects_missing_sink_Name()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString(new JObject
		{
			["MinimumLevel"] = "Information",
			["WriteTo"] = new JArray { new JObject { ["Args"] = new JObject() } }
		}, "Serilog");

		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(config.ValidateSerilogConfiguration);
		StringAssert.Contains(ex.Message, "Name");
	}

	[TestMethod]
	public void Ephemeral_ZipFile_migration_is_visible_to_Validate_without_disk_write()
	{
		// Mirrors CLI ephemeral startup: migrate in memory, then validate from the same store ConfigureLogging reads.
		var config = Configuration.CreateMockInstance();
		config.IsEphemeralInstance.Should().BeTrue();
		config.SetNonString(CreateSerilog("ZipFile"), "Serilog");

		config.EnsureSerilogConfig();
		config.ValidateSerilogConfiguration();

		var serilog = (JObject)config.GetObject("Serilog")!;
		Assert.AreEqual("File", serilog.SelectToken("$.WriteTo[0].Name")?.Value<string>());
	}

	[TestMethod]
	public void Fatal_startup_message_uses_invalid_configuration_body()
	{
		var ex = InvalidConfigurationValueException.ForEnum(
			"TokenStorageMethod",
			"Nope",
			typeof(AudibleApi.Authorization.TokenStorageMethod));

		var message = StartupAssemblyBootstrap.GetStartupFailureMessage(ex);
		Assert.IsNotNull(message);
		Assert.AreEqual("Invalid Settings.json", message!.Title);
		StringAssert.Contains(message.Body, "TokenStorageMethod");
		StringAssert.Contains(message.Body, "Nope");
	}

	private static JObject CreateSerilog(string sinkName) => new()
	{
		["MinimumLevel"] = "Information",
		["WriteTo"] = new JArray
		{
			new JObject
			{
				["Name"] = sinkName,
				["Args"] = new JObject
				{
					["path"] = Path.Combine(Path.GetTempPath(), "LibationTestLog.log"),
					["rollingInterval"] = "Month"
				}
			}
		},
		["Using"] = new JArray { "Dinah.Core", "Serilog.Exceptions" },
		["Enrich"] = new JArray { "WithCaller", "WithExceptionDetails" }
	};
}
