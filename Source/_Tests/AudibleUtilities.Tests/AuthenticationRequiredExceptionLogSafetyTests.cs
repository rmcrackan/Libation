using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using Dinah.Core.Security;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace AudibleUtilities.Tests;

/// <summary>
/// The reported bug: an account's address reached a log file because the exception carried the live account and
/// Serilog.Exceptions reflects over every public property of whatever it is given. These tests run the real
/// pipeline - the same enricher Libation configures - and assert nothing private comes out the other end.
/// </summary>
[TestClass]
public class AuthenticationRequiredExceptionLogSafety
{
	private const string AccountId = "jade@example.com";
	private const string AccountName = "Jade";
	private const string ActivationBytes = "1a2b3c4d";
	private const string RefreshTokenValue = "Atnr|_CHAR_REFRESH_";
	private const string AccessTokenValue = "Atna|_CHAR_ACCESS_";
	private const string AdpTokenValue = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
	private const string CookieValue = "cookie-secret-value";
	private const string StoreAuthCookie = "store-auth-cookie-value";

	private static Account registeredAccount()
	{
		var privateKey = RSA.Create(2048).ExportRSAPrivateKeyPem();
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(privateKey),
			new AdpToken(AdpTokenValue),
			new AccessToken(AccessTokenValue, new DateTime(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc)),
			new RefreshToken(RefreshTokenValue),
			new List<KeyValuePair<string, SecretString>> { new("session-id", CookieValue) },
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: StoreAuthCookie);

		return new Account(AccountId)
		{
			AccountName = AccountName,
			DecryptKey = ActivationBytes,
			IdentityTokens = identity
		};
	}

	[TestMethod]
	public void logging_the_exception_writes_nothing_private()
	{
		var account = registeredAccount();
		var privateKeyHeader = account.IdentityTokens!.PrivateKey!.Reveal()[..40];
		var ex = new AuthenticationRequiredException(
			account,
			message: $"Stored credentials for {account.MaskedLogEntry} are missing or incomplete.");

		var written = renderThroughSerilog(ex);

		// the masked form is present, so the entry is still useful for telling accounts apart
		StringAssert.Contains(written, account.MaskedLogEntry);

		foreach (var secret in new[] { AccountId, AccountName, ActivationBytes, RefreshTokenValue, AccessTokenValue, AdpTokenValue, CookieValue, StoreAuthCookie, privateKeyHeader })
			Assert.IsFalse(written.Contains(secret, StringComparison.Ordinal), $"log contained a secret: {secret}");
	}

	/// <summary>
	/// The inner exception is logged too, so a failure that names an account on its way up the chain has the same
	/// problem. This is the shape AutoScanRunner logs: an outer exception wrapping the authentication failure.
	/// </summary>
	[TestMethod]
	public void logging_it_as_an_inner_exception_writes_nothing_private()
	{
		var ex = new InvalidOperationException(
			"Error scanning library",
			new AuthenticationRequiredException(registeredAccount()));

		var written = renderThroughSerilog(ex);

		Assert.IsFalse(written.Contains(AccountId, StringComparison.Ordinal), "log contained the account id");
		Assert.IsFalse(written.Contains(ActivationBytes, StringComparison.Ordinal), "log contained the activation bytes");
	}

	[TestMethod]
	public void the_dialog_still_gets_the_full_label()
	{
		var summary = AccountSummary.From(registeredAccount())!;

		StringAssert.Contains(summary.RevealOwnerFacingLabel(), AccountId);
		Assert.IsFalse(summary.MaskedLogEntry.Contains(AccountId, StringComparison.Ordinal));
	}

	/// <summary>
	/// Renders a log event the way Libation's file sink does: the message, the exception, and the expanded
	/// {Properties:j} that WithExceptionDetails fills in.
	/// </summary>
	private static string renderThroughSerilog(Exception ex)
	{
		var sink = new CollectingSink();
		using var logger = new LoggerConfiguration()
			.Enrich.WithExceptionDetails()
			.WriteTo.Sink(sink)
			.CreateLogger();

		logger.Warning(ex, "Auto-scan paused: Audible login is required.");

		var logEvent = sink.Events.Single();
		return logEvent.RenderMessage()
			+ logEvent.Exception
			+ string.Join("|", logEvent.Properties.Select(p => $"{p.Key}={p.Value}"));
	}

	private class CollectingSink : ILogEventSink
	{
		public List<LogEvent> Events { get; } = [];
		public void Emit(LogEvent logEvent) => Events.Add(logEvent);
	}
}

/// <summary>
/// The paths that do not involve an exception at all: interpolating an account, and persisting its activation
/// bytes.
/// </summary>
[TestClass]
public class AccountMasking
{
	[TestMethod]
	public void interpolating_an_account_is_masked()
	{
		var account = new Account("jade@example.com") { AccountName = "Jade" };

		var interpolated = $"{account}";

		Assert.AreEqual(account.MaskedLogEntry, interpolated);
		Assert.IsFalse(interpolated.Contains("jade@example.com", StringComparison.Ordinal));
		Assert.IsFalse(interpolated.Contains("Jade", StringComparison.Ordinal));
	}

	[TestMethod]
	public void an_account_declares_itself_maskable_for_serilog()
		=> Assert.IsInstanceOfType<ILogMasked>(new Account("jade@example.com"));

	/// <summary>
	/// DecryptKey became a SecretString, which would serialize as an object and lose the value if the converter
	/// were not doing its job. Existing settings files have to keep loading, and keep the shape they had.
	/// </summary>
	[TestMethod]
	public void the_activation_bytes_still_persist_as_a_bare_string()
	{
		var json = JsonConvert.SerializeObject(new Account("jade@example.com") { DecryptKey = "1a2b3c4d" });

		var decryptKey = JObject.Parse(json)["DecryptKey"]!;
		Assert.AreEqual(JTokenType.String, decryptKey.Type);
		Assert.AreEqual("1a2b3c4d", decryptKey.ToObject<string>());
	}
}

// The guard that no exception type can reach an Account lives in LibationUiBase.Tests
// (ExceptionsCannotReachAnAccount): a test only sees the assemblies its own project references, and that one
// reaches ApplicationServices, FileLiberator, and DataLayer as well as these.
