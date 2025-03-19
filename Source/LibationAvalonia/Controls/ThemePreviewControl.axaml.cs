using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DataLayer;
using Dinah.Core.ErrorHandling;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using NPOI.Util.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls;

public partial class ThemePreviewControl : UserControl
{
	public ProductsDisplayViewModel ProductsDisplay { get; set; }
	public string[] ComboBoxItems { get; } = Enumerable.Range(1, 9).Select(n => $"Combo box item {n}").ToArray();
	public int ComboBoxSelectedIndex { get; set; }

	public ProcessBookViewModel QueuedBook { get; }
	public ProcessBookViewModel WorkingBook { get; }
	public ProcessBookViewModel CompletedBook { get; }
	public ProcessBookViewModel CancelledBook { get; }
	public ProcessBookViewModel FailedBook { get; }
	public ThemePreviewControl()
    {
		InitializeComponent();
		List<LibraryBook> sampleEntries;
		sampleEntries = CreateMockBooks().ToList();

		if (Design.IsDesignMode)
		{
			using var ms1 = new MemoryStream();
			App.OpenAsset("img-coverart-prod-unavailable_80x80.jpg").CopyTo(ms1);
			PictureStorage.SetDefaultImage(PictureSize._80x80, ms1.ToArray());
		}

		QueuedBook = new ProcessBookViewModel(sampleEntries[0], null) { Status = ProcessBookStatus.Queued };
		WorkingBook = new ProcessBookViewModel(sampleEntries[0], null) { Status = ProcessBookStatus.Working };
		CompletedBook = new ProcessBookViewModel(sampleEntries[0], null) { Status = ProcessBookStatus.Completed };
		CancelledBook = new ProcessBookViewModel(sampleEntries[0], null) { Status = ProcessBookStatus.Cancelled };
		FailedBook = new ProcessBookViewModel(sampleEntries[0], null) { Status = ProcessBookStatus.Failed };

		//Set the current processable so that the empty queue doesn't try to advance.
		QueuedBook.AddDownloadPdf();
		WorkingBook.AddDownloadPdf();

		typeof(ProcessBookViewModel).GetProperty(nameof(ProcessBookViewModel.Progress)).SetValue(WorkingBook, 50);

		ProductsDisplay = new ProductsDisplayViewModel();
		_ = ProductsDisplay.BindToGridAsync(sampleEntries);
		DataContext = this;
    }

	private IEnumerable<LibraryBook> CreateMockBooks()
	{
		var author = new Contributor("Some Author", "asin_contributor");
		var narrator = new Contributor("Some Narrator", "asin_narrator");

		var book1 = new Book(new AudibleProductId("asin_book1"), "Some Book 1", "The Theming", "Demo Book Entry", 525600, ContentType.Product, [author], [narrator], "us");
		var book2 = new Book(new AudibleProductId("asin_book2"), "Some Book 2", "The Theming", "Demo Book Entry", 525600, ContentType.Product, [author], [narrator], "us");
		var book3 = new Book(new AudibleProductId("asin_book3"), "Some Book 3", "The Theming", "Demo Book Entry", 525600, ContentType.Product, [author], [narrator], "us");
		var book4 = new Book(new AudibleProductId("asin_book4"), "Some Book 4", "The Theming", "Demo Book Entry", 525600, ContentType.Product, [author], [narrator], "us");
		var seriesParent = new Book(new AudibleProductId("asin_series"), "Some Series", "", "Demo Series Entry", 0, ContentType.Parent, [author], [narrator], "us");
		var episode = new Book(new AudibleProductId("asin_episode"), "Some Episode", "Episode 1", "Demo Episode Entry", 56, ContentType.Episode, [author], [narrator], "us");

		var series = new Series(new AudibleSeriesId(seriesParent.AudibleProductId), seriesParent.Title);

		seriesParent.UpsertSeries(series, "");
		episode.UpsertSeries(series, "1");

		book1.UserDefinedItem.BookStatus = LiberatedStatus.Liberated;
		book4.UserDefinedItem.BookStatus = LiberatedStatus.Error;
		//Set the backing field directly to preserve LiberatedStatus.PartialDownload
		typeof(UserDefinedItem).GetField("_bookStatus", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(book2.UserDefinedItem, LiberatedStatus.PartialDownload);

		yield return new LibraryBook(book1, System.DateTime.Now.AddDays(4), "someone@email.co");
		yield return new LibraryBook(book2, System.DateTime.Now.AddDays(3), "someone@email.co");
		yield return new LibraryBook(book3, System.DateTime.Now.AddDays(2), "someone@email.co") { AbsentFromLastScan = true };
		yield return new LibraryBook(book4, System.DateTime.Now.AddDays(1), "someone@email.co");
		yield return new LibraryBook(seriesParent, System.DateTime.Now, "someone@email.co");
		yield return new LibraryBook(episode, System.DateTime.Now, "someone@email.co");
	}

	private class MockProcessable : FileLiberator.Processable
	{
		public override string Name => nameof(MockProcessable);
		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook) => Task.FromResult(new StatusHandler());
		public override bool Validate(LibraryBook libraryBook) => false;
	}
}