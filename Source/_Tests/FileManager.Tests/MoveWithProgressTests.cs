using FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MoveWithProgressTests;

/// <summary>
/// The destination folder is named by the folder template, so it holds book metadata such as the title.
/// Before a move, the device check runs stat on that folder.
/// </summary>
[TestClass]
[DoNotParallelize]
public class WhenTheDestinationFolderNameContainsShellSyntax
{
	[TestMethod]
	public async Task the_file_is_moved_and_nothing_in_the_name_is_executed()
	{
		if (OperatingSystem.IsWindows())
		{
			Assert.Inconclusive("Skipped because Windows reads the drive serial number instead of running stat.");
			return;
		}

		var root = Directory.CreateTempSubdirectory("MoveWithProgressTests").FullName;
		try
		{
			var marker = Path.Combine(root, "marker");
			var bookFolder = Directory.CreateDirectory(Path.Combine(root, "Title $(touch marker) `touch marker` [B0TEST1234]"));
			var source = Path.Combine(root, "book.m4b");
			var destination = Path.Combine(bookFolder.FullName, "book.m4b");
			File.WriteAllText(source, "audio");

			var previousDirectory = Environment.CurrentDirectory;
			Environment.CurrentDirectory = root;
			try
			{
				Assert.IsTrue(await new MoveWithProgress().MoveAsync(source, destination));
			}
			finally
			{
				Environment.CurrentDirectory = previousDirectory;
			}

			Assert.IsFalse(File.Exists(marker), "the folder name was run as a shell command");
			Assert.AreEqual("audio", File.ReadAllText(destination));
			Assert.IsFalse(File.Exists(source));
		}
		finally
		{
			Directory.Delete(root, true);
		}
	}
}
