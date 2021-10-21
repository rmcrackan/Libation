using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using FileLiberator;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioFileStorageExtTests
{
	[TestClass]
	public class GetValidFilename
	{
		private DataLayer.LibraryBook GetLibraryBook(string asin)
		{
			var book = new DataLayer.Book(new DataLayer.AudibleProductId(asin), "title", "desc", 1, DataLayer.ContentType.Product, new List<DataLayer.Contributor> { new DataLayer.Contributor("author") }, new List<DataLayer.Contributor> { new DataLayer.Contributor("narrator") }, new DataLayer.Category(new DataLayer.AudibleCategoryId("seriesId") , "name"), "us");
			var libraryBook = new DataLayer.LibraryBook(book, DateTime.Now, "my us");
			return libraryBook;
		}

		[TestMethod]
		[DataRow(null, "name", "ext", "suffix")]
		[DataRow(@"C:\", null, "ext", "suffix")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void arg_null_exception(string dirFullPath, string filename, string extension, string metadataSuffix)
			=> AudioFileStorageExt.GetValidFilename(dirFullPath, filename, extension, GetLibraryBook(metadataSuffix));

		[TestMethod]
		[DataRow("", "name", "ext", "suffix")]
		[DataRow("   ", "name", "ext", "suffix")]
		[DataRow(@"C:\", "", "ext", "suffix")]
		[DataRow(@"C:\", "   ", "ext", "suffix")]
		[ExpectedException(typeof(ArgumentException))]
		public void arg_exception(string dirFullPath, string filename, string extension, string metadataSuffix)
			=> AudioFileStorageExt.GetValidFilename(dirFullPath, filename, extension, GetLibraryBook(metadataSuffix));

		[TestMethod]
		public void null_extension() => Tests(@"C:\foo\bar", "my file", null, "meta", @"C:\foo\bar\my file [meta]");

		[TestMethod]
		[DataRow(@"C:\foo\bar", "my file", "txt", "my id", @"C:\foo\bar\my file [my id].txt")]
		public void Tests(string dirFullPath, string filename, string extension, string metadataSuffix, string expected)
			=> AudioFileStorageExt.GetValidFilename(dirFullPath, filename, extension, GetLibraryBook(metadataSuffix)).Should().Be(expected);
	}
}
