using FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace SaferEnumerateFilesTests;

/// <summary>
/// From issue #1984: a Books folder on a USB drive started returning I/O errors partway through a library scan,
/// and Libation closed with "Libation encountered a fatal error". It then closed the same way on every launch
/// afterwards, before the window appeared.
/// <para>
/// Two guards were supposed to prevent that and neither did. <see cref="EnumerationOptions.IgnoreInaccessible"/>
/// only forgives permissions, so an I/O error still came out of the enumerator. And the try/catch the caller had
/// wrapped around the call never saw it: listing files is lazy, so the error was raised where the sequence was
/// walked - inside the caller's AddRange, long after the try/catch had exited.
/// </para>
/// </summary>
[TestClass]
public class WhenADirectoryStopsBeingReadablePartwayThrough
{
	// Relative, and built with Path.Combine, so the separators are whatever this platform uses. LongPath
	// rewrites '/' to '\' on Windows, which is not what any of these tests are about.
	private static readonly LongPath BooksDirectory = "books";
	private static readonly LongPath FirstFile = Path.Combine("books", "first.m4b");
	private static readonly LongPath SecondFile = Path.Combine("books", "second.m4b");

	private static IEnumerable<LongPath> TwoFilesThenFails(Exception failure)
	{
		yield return FirstFile;
		yield return SecondFile;
		throw failure;
	}

	/// <summary>The shape of the crash: the walk is consumed by an AddRange, exactly as the file cache does.</summary>
	[TestMethod]
	public void the_walk_ends_early_instead_of_throwing_at_whoever_is_consuming_it()
	{
		var found = new List<LongPath>();

		found.AddRange(FileUtility.IterateSafely(
			() => TwoFilesThenFails(new IOException("Input/output error")),
			BooksDirectory));

		CollectionAssert.AreEqual(
			new[] { FirstFile, SecondFile },
			found,
			"everything read before the failure is still good, and still worth returning");
	}

	[TestMethod]
	public void the_caller_can_tell_a_truncated_walk_from_an_empty_directory()
	{
		Exception? reported = null;
		var failure = new IOException("Input/output error");

		FileUtility.IterateSafely(() => TwoFilesThenFails(failure), BooksDirectory, ex => reported = ex).ToList();

		Assert.AreSame(failure, reported);
	}

	[TestMethod]
	public void nothing_is_reported_when_the_whole_directory_was_read()
	{
		var reported = false;

		FileUtility.IterateSafely(() => new[] { FirstFile }, BooksDirectory, _ => reported = true).ToList();

		Assert.IsFalse(reported);
	}

	/// <summary>Opening the directory is itself a read, and fails the same way.</summary>
	[TestMethod]
	public void a_failure_before_the_first_entry_lists_nothing_rather_than_throwing()
	{
		Exception? reported = null;

		var found = FileUtility.IterateSafely(
			() => throw new IOException("Input/output error"),
			BooksDirectory,
			ex => reported = ex).ToList();

		Assert.AreEqual(0, found.Count);
		Assert.IsInstanceOfType<IOException>(reported);
	}

	[TestMethod]
	public void a_directory_that_has_gone_is_forgiven_too()
	{
		var found = FileUtility.IterateSafely(
			() => TwoFilesThenFails(new DirectoryNotFoundException()),
			BooksDirectory).ToList();

		Assert.AreEqual(2, found.Count);
	}

	[TestMethod]
	public void so_is_a_directory_the_user_is_not_allowed_to_read()
	{
		var found = FileUtility.IterateSafely(
			() => TwoFilesThenFails(new UnauthorizedAccessException()),
			BooksDirectory).ToList();

		Assert.AreEqual(2, found.Count);
	}

	/// <summary>
	/// The guard is for a file system that will not answer, not for bugs. Widening it to everything would hide
	/// the next defect in here behind a short list of files.
	/// </summary>
	[TestMethod]
	public void a_failure_that_is_not_the_file_system_is_still_raised()
	{
		var found = FileUtility.IterateSafely(
			() => TwoFilesThenFails(new InvalidOperationException("a real bug")),
			BooksDirectory);

		Assert.ThrowsExactly<InvalidOperationException>(() => found.ToList());
	}
}

[TestClass]
public class SaferEnumerateFilesAgainstARealDirectory
{
	private string tempDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-enumerate-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			if (!OperatingSystem.IsWindows() && Directory.Exists(tempDir))
				File.SetUnixFileMode(tempDir, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
			Directory.Delete(tempDir, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	[TestMethod]
	public void the_files_that_are_there_are_listed()
	{
		File.WriteAllText(Path.Combine(tempDir, "book.m4b"), "audio");
		Directory.CreateDirectory(Path.Combine(tempDir, "nested"));
		File.WriteAllText(Path.Combine(tempDir, "nested", "another.m4b"), "audio");

		var topOnly = FileUtility.SaferEnumerateFiles(tempDir).Select(f => Path.GetFileName((string)f)).ToArray();
		var everything = FileUtility.SaferEnumerateFiles(tempDir, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName((string)f)).Order().ToArray();

		CollectionAssert.AreEqual(new[] { "book.m4b" }, topOnly);
		CollectionAssert.AreEqual(new[] { "another.m4b", "book.m4b" }, everything);
	}

	/// <summary>
	/// This used to throw, and lazily, so it landed on whoever walked the sequence rather than whoever asked for
	/// it. A Books folder that is not there is a settings problem to report, never a crash.
	/// </summary>
	[TestMethod]
	public void a_directory_that_is_not_there_lists_nothing_and_says_why()
	{
		Exception? reported = null;
		var missing = Path.Combine(tempDir, "no-such-folder");

		var found = FileUtility.SaferEnumerateFiles(missing, onIncomplete: ex => reported = ex).ToList();

		Assert.AreEqual(0, found.Count);
		Assert.IsInstanceOfType<DirectoryNotFoundException>(reported);
	}
}

[TestClass]
public class CanEnumerate
{
	private string tempDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-can-enumerate-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			if (!OperatingSystem.IsWindows() && Directory.Exists(tempDir))
				File.SetUnixFileMode(tempDir, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
			Directory.Delete(tempDir, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	[TestMethod]
	public void a_readable_directory_can_be_read()
	{
		Assert.IsTrue(FileUtility.CanEnumerate(tempDir), "an empty directory is readable, it is just empty");

		File.WriteAllText(Path.Combine(tempDir, "book.m4b"), "audio");

		Assert.IsTrue(FileUtility.CanEnumerate(tempDir));
	}

	[TestMethod]
	public void a_directory_that_is_not_there_cannot_be_read()
		=> Assert.IsFalse(FileUtility.CanEnumerate(Path.Combine(tempDir, "no-such-folder")));

	/// <summary>
	/// Existing is not the same as usable, which is the whole point of asking. A pulled or failing drive still
	/// answers that it is a directory, and a recursive listing of it comes back empty rather than refusing -
	/// IgnoreInaccessible sees to that - so without this check Libation reports a full library as downloading
	/// nothing instead of saying the drive is unreadable.
	/// </summary>
	[TestMethod]
	public void a_directory_that_exists_but_refuses_to_be_read_cannot_be_read()
	{
		// Assert.Inconclusive is not [DoesNotReturn], so return explicitly or the body below still
		// looks reachable on Windows to the platform compatibility analyzer
		if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
		{
			Assert.Inconclusive("Skipped because revoking directory read permission needs unix file modes.");
			return;
		}
		if (Environment.IsPrivilegedProcess)
		{
			Assert.Inconclusive("Skipped because root may read a directory with no permissions, so there is nothing to refuse.");
			return;
		}

		aDirectoryThatExistsButRefusesToBeRead(tempDir);
	}

	[SupportedOSPlatform("linux")]
	[SupportedOSPlatform("macos")]
	private static void aDirectoryThatExistsButRefusesToBeRead(string directory)
	{
		File.WriteAllText(Path.Combine(directory, "book.m4b"), "audio");
		File.SetUnixFileMode(directory, UnixFileMode.None);

		Assert.IsTrue(Directory.Exists(directory), "the directory is still there; it just cannot be read");
		Assert.AreEqual(0, FileUtility.SaferEnumerateFiles(directory, "*", SearchOption.AllDirectories).Count(), "a listing comes back empty rather than refusing");
		Assert.IsFalse(FileUtility.CanEnumerate(directory));
	}
}
