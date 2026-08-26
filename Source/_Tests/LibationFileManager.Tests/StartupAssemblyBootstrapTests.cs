using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationFileManager.Tests;

/// <summary>
/// Issue #2001: a startup crash reported nothing more than "Libation encountered a fatal error", because
/// the assembly that failed to load was Serilog and the classification only knew a handful of names.
/// </summary>
[TestClass]
public class StartupAssemblyBootstrapTests
{
	// Serilog is in this test project's own output folder, so the file exists and its version can be read.
	// Asking for an impossible version is the reporter's situation: an overlay upgrade that left the old
	// file in place, where the loader reports the same "cannot find the file specified" as for a deletion.
	private static FileNotFoundException StaleAssemblyFailure(string assembly = "Serilog", string version = "99.0.0.0")
	{
		var identity = $"{assembly}, Version={version}, Culture=neutral, PublicKeyToken=24c2f752a8e58a10";
		return new FileNotFoundException(
			$"Could not load file or assembly '{identity}'. The system cannot find the file specified.",
			identity);
	}

	[TestMethod]
	public void A_stale_install_file_is_recognised_and_both_versions_are_read()
	{
		Assert.IsTrue(StartupAssemblyBootstrap.TryGetInstallAssemblyFailure(StaleAssemblyFailure(), out var failure));

		Assert.IsNotNull(failure);
		Assert.AreEqual("Serilog", failure!.AssemblyName);
		Assert.AreEqual(new Version("99.0.0.0"), failure.RequestedVersion);
		Assert.IsNotNull(failure.InstalledVersion, "Serilog.dll is in this test's own folder, so its version should be readable");
		Assert.IsTrue(failure.InstalledVersion < failure.RequestedVersion);
		Assert.AreEqual("Serilog.dll", failure.FileName);
	}

	[TestMethod]
	public void A_missing_install_file_is_recognised_with_no_installed_version()
	{
		Assert.IsTrue(StartupAssemblyBootstrap.TryGetInstallAssemblyFailure(
			StaleAssemblyFailure("NoSuchLibationDependency", "1.2.3.4"),
			out var failure));

		Assert.IsNotNull(failure);
		Assert.IsNull(failure!.InstalledVersion);
		StringAssert.Contains(StartupAssemblyBootstrap.DescribeInstallAssemblyFailure(failure), "is missing from the install folder");
	}

	[TestMethod]
	public void An_assembly_that_is_present_and_new_enough_is_left_to_someone_else()
	{
		// Serilog.dll is right there and satisfies this reference, so whatever went wrong is not a broken
		// install folder and must not be reported as one.
		Assert.IsFalse(StartupAssemblyBootstrap.TryGetInstallAssemblyFailure(
			StaleAssemblyFailure("Serilog", "1.0.0.0"),
			out var failure));

		Assert.IsNull(failure);
	}

	// The loader is the only thing that raises a FileNotFoundException carrying an assembly identity.
	// Libation's own "EF Core is missing" check carries a plain path, and must keep its own message.
	[TestMethod]
	public void A_plain_file_path_is_not_mistaken_for_an_assembly_reference()
	{
		var ex = new FileNotFoundException(
			"Required file 'Microsoft.EntityFrameworkCore.Sqlite.dll' was not found in the Libation install folder.",
			Path.Combine("C:", "Libation", "Microsoft.EntityFrameworkCore.Sqlite.dll"));

		Assert.IsFalse(StartupAssemblyBootstrap.TryGetInstallAssemblyFailure(ex, out _));
		Assert.AreEqual("Library load failed", StartupAssemblyBootstrap.GetStartupFailureMessage(ex)?.Title);
	}

	[TestMethod]
	public void The_message_names_the_file_the_version_it_has_and_the_version_it_needs()
	{
		var message = StartupAssemblyBootstrap.GetStartupFailureMessage(StaleAssemblyFailure());

		Assert.IsNotNull(message);
		Assert.AreEqual("Libation could not load a required file", message!.Title);
		StringAssert.Contains(message.Body, "Serilog.dll");
		StringAssert.Contains(message.Body, "99.0.0.0");
		StringAssert.Contains(message.Body, "troubleshoot#windows-incomplete-in-app-upgrade");
	}

	// Before this, a Serilog failure matched no predicate, so GetFatalStartupMessage never even looked for
	// a backup to restore and fell through to the generic crash text.
	[TestMethod]
	public void A_stale_install_file_no_longer_falls_through_to_the_generic_crash_message()
	{
		var generic = new FatalStartupMessage("Libation Crash", "Libation encountered a fatal error and must close.");

		var message = StartupAssemblyBootstrap.GetFatalStartupMessage(StaleAssemblyFailure(), generic);

		Assert.AreNotEqual(generic.Title, message.Title);
		Assert.AreNotEqual(generic.Body, message.Body);
	}

	[TestMethod]
	public void An_assembly_failure_nested_in_an_AggregateException_is_still_found()
	{
		var ex = new AggregateException(new InvalidOperationException("unrelated"), StaleAssemblyFailure());

		Assert.IsTrue(StartupAssemblyBootstrap.TryGetInstallAssemblyFailure(ex, out var failure));
		Assert.AreEqual("Serilog", failure!.AssemblyName);
	}

	[TestMethod]
	public void An_unrelated_failure_is_still_no_ones_problem()
	{
		Assert.IsFalse(StartupAssemblyBootstrap.TryGetInstallAssemblyFailure(new InvalidOperationException("nothing to do with files"), out _));
		Assert.IsNull(StartupAssemblyBootstrap.GetStartupFailureMessage(new InvalidOperationException("nothing to do with files")));
	}
}
