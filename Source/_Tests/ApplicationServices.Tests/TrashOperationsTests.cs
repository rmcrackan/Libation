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
		foreach (var deleted in new[] { false, true })
		{
			var id = deleted ? "B0UNSELECTEDTRASH" : "B0UNSELECTED";
			var untouched = new Book(new AudibleProductId(id), id, "", "Untouched description", 600,
				ContentType.Product, [new Contributor(id + " author")], [new Contributor(id + " narrator")], "us");
			context.LibraryBooks.Add(new LibraryBook(untouched, new DateTime(2020, 1, 1), "untouched-account")
			{
				IsDeleted = deleted,
				AbsentFromLastScan = true,
				IsAudiblePlus = true
			});
		}
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

	private static void AssertUnselectedUnchanged()
	{
		using var context = DbContexts.GetContext();
		foreach (var deleted in new[] { false, true })
		{
			var id = deleted ? "B0UNSELECTEDTRASH" : "B0UNSELECTED";
			var row = context.LibraryBooks.Single(lb => lb.Book.AudibleProductId == id);
			Assert.AreEqual(deleted, row.IsDeleted);
			Assert.AreEqual("untouched-account", row.Account);
			Assert.AreEqual(new DateTime(2020, 1, 1), row.DateAdded);
			Assert.IsTrue(row.AbsentFromLastScan);
			Assert.IsTrue(row.IsAudiblePlus);
			var book = context.Books.Single(b => b.AudibleProductId == id);
			Assert.AreEqual(id, book.Title);
			Assert.AreEqual("Untouched description", book.Description);
		}
	}

	[TestMethod]
	public async Task Remove_and_restore_duplicate_instances_preserve_newer_database_fields()
	{
		var first = Read();
		var second = Read();
		Assert.AreNotSame(first, second);
		using (var context = DbContexts.GetContext())
		{
			var current = context.LibraryBooks.Single(lb => lb.Book.AudibleProductId == "B0TRASHTEST");
			current.SetAccount("updated-account");
			current.AbsentFromLastScan = true;
			context.SaveChanges();
		}

		LibraryBook[] selection = [first, second];
		Assert.IsTrue(await selection.RemoveBooksAsync() > 0);
		AssertUnselectedUnchanged();
		Assert.IsFalse(first.IsDeleted, "Do not mutate detached UI objects before refreshing the library.");
		using (var context = DbContexts.GetContext())
		{
			var current = context.LibraryBooks.Single(lb => lb.Book.AudibleProductId == "B0TRASHTEST");
			Assert.IsTrue(current.IsDeleted);
			Assert.AreEqual("updated-account", current.Account);
			Assert.IsTrue(current.AbsentFromLastScan);
		}

		Assert.IsTrue(await selection.RestoreBooksAsync() > 0);
		AssertUnselectedUnchanged();
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
		AssertUnselectedUnchanged();
		using var context = DbContexts.GetContext();
		Assert.AreEqual(2, context.LibraryBooks.Count());
		Assert.AreEqual(2, context.Books.Count());
		Assert.IsFalse(context.LibraryBooks.Any(lb => lb.Book.AudibleProductId == "B0TRASHTEST"));
		Assert.IsFalse(context.Books.Any(b => b.AudibleProductId == "B0TRASHTEST"));
	}

	[TestMethod]
	public async Task Repeated_operations_and_missing_rows_are_no_ops()
	{
		LibraryBook[] selection = [Read(), Read()];
		Assert.AreEqual(0, await selection.RestoreBooksAsync());
		Assert.IsTrue(await selection.RemoveBooksAsync() > 0);
		Assert.AreEqual(0, await selection.RemoveBooksAsync());
		Assert.IsTrue(await selection.RestoreBooksAsync() > 0);
		Assert.AreEqual(0, await selection.RestoreBooksAsync());
		Assert.IsTrue(await selection.PermanentlyDeleteBooksAsync() > 0);
		Assert.AreEqual(0, await selection.PermanentlyDeleteBooksAsync());
		Assert.AreEqual(0, await selection.RemoveBooksAsync());
		Assert.AreEqual(0, await selection.RestoreBooksAsync());
		AssertUnselectedUnchanged();
	}
}
