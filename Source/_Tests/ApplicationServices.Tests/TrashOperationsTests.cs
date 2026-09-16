using ApplicationServices;
using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationServices.Tests;

[TestClass]
[DoNotParallelize]
public class TrashOperationsTests
{
	private string directory = string.Empty;
	private string? previousDirectory;

	[TestInitialize]
	public void Initialize()
	{
		directory = Path.Combine(Path.GetTempPath(), $"libation-trash-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(directory);
		previousDirectory = Environment.GetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR);
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, directory);
		Configuration.CreateMockInstance();
		using var context = DbContexts.GetContext();
		var book = new Book(new AudibleProductId("B0TRASHTEST"), "Trash test", "", "Description", 600,
			ContentType.Product, [new Contributor("Author")], [new Contributor("Narrator")], "us");
		context.LibraryBooks.Add(new LibraryBook(book, DateTime.UtcNow, "original-account"));
		context.SaveChanges();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, previousDirectory);
		Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
		try { Directory.Delete(directory, recursive: true); }
		catch (IOException) { }
	}

	private static LibraryBook Read()
	{
		using var context = DbContexts.GetContext();
		return context.GetLibraryBook_Flat_NoTracking("B0TRASHTEST")!;
	}

	[TestMethod]
	public async Task Remove_and_restore_duplicate_instances_preserve_newer_database_fields()
	{
		var first = Read();
		var second = Read();
		Assert.AreNotSame(first, second);
		using (var context = DbContexts.GetContext())
		{
			var current = context.LibraryBooks.Single();
			current.SetAccount("updated-account");
			current.AbsentFromLastScan = true;
			context.SaveChanges();
		}

		LibraryBook[] selection = [first, second];
		Assert.IsTrue(await selection.RemoveBooksAsync() > 0);
		Assert.IsFalse(first.IsDeleted, "Do not mutate detached UI objects before refreshing the library.");
		using (var context = DbContexts.GetContext())
		{
			var current = context.LibraryBooks.Single();
			Assert.IsTrue(current.IsDeleted);
			Assert.AreEqual("updated-account", current.Account);
			Assert.IsTrue(current.AbsentFromLastScan);
		}

		Assert.IsTrue(await selection.RestoreBooksAsync() > 0);
		var restored = Read();
		Assert.IsFalse(restored.IsDeleted);
		Assert.AreEqual("updated-account", restored.Account);
		Assert.IsTrue(restored.AbsentFromLastScan);
	}

	[TestMethod]
	public async Task Permanent_delete_accepts_duplicate_instances_and_deletes_both_rows()
	{
		LibraryBook[] selection = [Read(), Read()];
		Assert.AreNotSame(selection[0], selection[1]);
		Assert.IsTrue(await selection.PermanentlyDeleteBooksAsync() > 0);
		using var context = DbContexts.GetContext();
		Assert.AreEqual(0, context.LibraryBooks.Count());
		Assert.AreEqual(0, context.Books.Count());
	}
}
