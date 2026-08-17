using ApplicationServices;
using AudibleApi;
using AppScaffolding;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using AudibleUtilities;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LibationUiBase.Tests;

/// <summary>
/// A guard against reintroducing the leak fixed in #1960: an AuthenticationRequiredException carried a live
/// <see cref="Account"/>, and Serilog.Exceptions writes every public property of a logged exception - following
/// nested objects to a depth of 10 - into the log file people attach to public issue reports. So no exception
/// anywhere may be able to reach an account or an identity that way.
/// <para>
/// This lives here rather than beside the exception it guards because a test only sees the assemblies its own
/// project references, and this one reaches the most of them.
/// </para>
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

	/// <summary>
	/// One public type per assembly, used only to reach the assembly. The AudibleApi packages are in here
	/// deliberately: <see cref="Identity"/> is theirs, so an upstream exception that started carrying one would
	/// leak through Libation, and this is where we would want to find that out - at the version bump, rather
	/// than in someone's log.
	/// </summary>
	private static readonly Assembly[] Assemblies =
	[
		typeof(AutoScanRunner).Assembly,           // LibationUiBase
		typeof(LibraryCommands).Assembly,          // ApplicationServices
		typeof(LibationScaffolding).Assembly,      // AppScaffolding
		typeof(Processable).Assembly,              // FileLiberator
		typeof(Account).Assembly,                  // AudibleUtilities
		typeof(Configuration).Assembly,            // LibationFileManager
		typeof(FileManager.LongPath).Assembly,     // FileManager
		typeof(LibraryBook).Assembly,              // DataLayer
		typeof(Identity).Assembly,                 // AudibleApi
		typeof(NonJsonResponseException).Assembly  // AudibleApi.Common
	];

	[TestMethod]
	public void no_exception_type_exposes_one_through_its_public_properties()
	{
		var exceptionTypes = Assemblies
			.Distinct()
			.SelectMany(a => a.GetTypes())
			.Where(t => typeof(Exception).IsAssignableFrom(t))
			.ToArray();

		// if a rename or a project split ever empties this, the test must fail rather than pass silently
		Assert.IsTrue(exceptionTypes.Length > 0, "found no exception types to check");

		foreach (var exceptionType in exceptionTypes)
		{
			var path = FindForbidden(exceptionType, depth: 0, [], []);
			Assert.IsNull(path, $"{exceptionType.Name} can reach a secret-bearing type through public properties: {path}");
		}
	}

	/// <summary>
	/// Walks public instance properties the way Serilog.Exceptions' ReflectionBasedDestructurer does, to the same
	/// default depth, and returns the path to the first forbidden type it can reach.
	/// </summary>
	private static string? FindForbidden(Type type, int depth, HashSet<Type> visited, List<string> trail)
	{
		if (depth > 10 || !visited.Add(type))
			return null;

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.GetIndexParameters().Length > 0)
				continue;

			var propertyType = Unwrap(property.PropertyType);
			var step = trail.Append($"{type.Name}.{property.Name}").ToList();

			if (Forbidden.Contains(propertyType))
				return string.Join(" -> ", step);

			// only follow types from the assemblies at issue: the framework's own graphs are vast and cannot
			// reach an Audible account
			if (Assemblies.Contains(propertyType.Assembly))
			{
				var found = FindForbidden(propertyType, depth + 1, visited, step);
				if (found is not null)
					return found;
			}
		}

		return null;
	}

	/// <summary>Collections and nullables hide the interesting type one level down.</summary>
	private static Type Unwrap(Type type)
	{
		if (type.IsArray)
			return Unwrap(type.GetElementType()!);

		if (!type.IsGenericType)
			return type;

		var arguments = type.GetGenericArguments();
		return arguments.Length == 1 ? Unwrap(arguments[0]) : type;
	}
}
