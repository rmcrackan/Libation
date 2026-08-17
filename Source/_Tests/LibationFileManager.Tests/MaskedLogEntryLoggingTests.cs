using Dinah.Core.Security;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace SerilogConfigurationTests;

/// <summary>
/// Builds the logger the way Libation does and writes through it, rather than testing the destructuring policy
/// on its own: the policy only protects anything if <see cref="Configuration.ConfigureLogging"/> actually
/// registers it, and a missing registration is silent.
/// </summary>
[TestClass]
[DoNotParallelize]
public class MaskedLogEntryLoggingTests
{
	private const string Secret = "jade@example.com";
	private const string Masked = "AccountId=j[...]e|Locale=us";

	private string tempDir = string.Empty;
	private ILogger originalLogger = Serilog.Log.Logger;

	private class MaskedThing : ILogMasked
	{
		public string MaskedLogEntry => Masked;
		public string Address => Secret;
		public override string ToString() => Masked;
	}

	[TestInitialize]
	public void Initialize()
	{
		originalLogger = Serilog.Log.Logger;
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-masked-log-tests-{Guid.NewGuid():N}");
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

	private string LogThrough(Action<ILogger> write)
	{
		var config = Configuration.CreateMockInstance();
		config.EnsureSerilogConfig();

		var args = (JObject)((JObject)config.GetObject("Serilog")!).SelectToken("$.WriteTo[0].Args")!;
		args["path"] = Path.Combine(tempDir, "Log.log");
		args["outputTemplate"] = "{Message:lj} {Properties:j}{NewLine}";
		config.SetNonString((JObject)config.GetObject("Serilog")!, "Serilog");

		config.ConfigureLogging();

		write(Serilog.Log.Logger);
		Serilog.Log.CloseAndFlush();

		return string.Join("\n", Directory.GetFiles(tempDir, "Log*.log").Select(File.ReadAllText));
	}

	/// <summary>Destructured, so the policy is what has to catch it: without one, Address would be written.</summary>
	[TestMethod]
	public void a_destructured_masked_type_is_reduced_to_its_masked_entry()
	{
		var written = LogThrough(logger => logger.Information("scanning {@Account}", new MaskedThing()));

		StringAssert.Contains(written, Masked);
		Assert.IsFalse(written.Contains(Secret, StringComparison.Ordinal), "the log contained the unmasked value");
	}

	/// <summary>The shape most of Libation's logging uses: an anonymous object holding the thing.</summary>
	[TestMethod]
	public void a_masked_type_nested_in_a_debug_object_is_reduced_too()
	{
		var written = LogThrough(logger => logger.Information("scanning {@DebugInfo}", new { Account = new MaskedThing(), Attempt = 2 }));

		StringAssert.Contains(written, Masked);
		Assert.IsFalse(written.Contains(Secret, StringComparison.Ordinal), "the log contained the unmasked value");
	}

	/// <summary>
	/// Destructured on purpose, and with nothing registered for it: SecretString reports its own redaction from a
	/// property, so the shape survives being taken apart.
	/// </summary>
	[TestMethod]
	public void a_destructured_secret_is_written_as_its_shape()
	{
		var written = LogThrough(logger => logger.Information("key {@Key}", new SecretString(Secret)));

		StringAssert.Contains(written, $"[REDACTED length={Secret.Length}]");
		Assert.IsFalse(written.Contains(Secret, StringComparison.Ordinal), "the log contained the secret");
	}
}
