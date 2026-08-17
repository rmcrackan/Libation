using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using Dinah.Core.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
/// A guard against the next exception type reintroducing this. Serilog.Exceptions walks the public property
/// graph of whatever exception it is handed, to a default depth of 10, so no exception may be able to reach an
/// account or an identity that way.
/// </summary>
[TestClass]
public class ExceptionsCannotReachAnAccount
{
	private static readonly Type[] Forbidden =
	[
		typeof(Account),
		typeof(AccountsSettings),
		typeof(Identity),
		typeof(AccessToken),
		typeof(RefreshToken),
		typeof(AdpToken),
		typeof(PrivateKey)
	];

	[TestMethod]
	public void no_exception_type_exposes_one_through_its_public_properties()
	{
		// the assemblies this test project can see. Account and the authentication exception live in the first.
		var assemblies = new[]
		{
			typeof(Account).Assembly,
			typeof(LibationFileManager.Configuration).Assembly,
			typeof(FileManager.LongPath).Assembly
		};

		var exceptionTypes = assemblies
			.SelectMany(a => a.GetTypes())
			.Where(t => typeof(Exception).IsAssignableFrom(t))
			.ToArray();

		Assert.IsTrue(exceptionTypes.Length > 0, "found no exception types to check");

		foreach (var exceptionType in exceptionTypes)
		{
			var path = findForbidden(exceptionType, depth: 0, [], []);
			Assert.IsNull(path, $"{exceptionType.Name} can reach a secret-bearing type through public properties: {path}");
		}
	}

	private static string? findForbidden(Type type, int depth, HashSet<Type> visited, List<string> trail)
	{
		if (depth > 10 || !visited.Add(type))
			return null;

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.GetIndexParameters().Length > 0)
				continue;

			var propertyType = unwrap(property.PropertyType);
			var step = trail.Append($"{type.Name}.{property.Name}").ToList();

			if (Forbidden.Contains(propertyType))
				return string.Join(" -> ", step);

			if (propertyType.Assembly == typeof(Account).Assembly || propertyType.Assembly == typeof(Identity).Assembly)
			{
				var found = findForbidden(propertyType, depth + 1, visited, step);
				if (found is not null)
					return found;
			}
		}

		return null;
	}

	/// <summary>Collections and nullables hide the interesting type one level down.</summary>
	private static Type unwrap(Type type)
	{
		if (type.IsArray)
			return unwrap(type.GetElementType()!);

		if (!type.IsGenericType)
			return type;

		var arguments = type.GetGenericArguments();
		return arguments.Length == 1 ? unwrap(arguments[0]) : type;
	}
}
