using Avalonia.Controls;
using DataLayer;
using Dinah.Core.ErrorHandling;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationUiBase.ProcessQueue;
using System.Collections.Generic;
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
			MainVM.Configure_NonUI();
		}

		QueuedBook = new ProcessBookViewModel(sampleEntries[0], Configuration.Instance) { Status = ProcessBookStatus.Queued };
		WorkingBook = new ProcessBookViewModel(sampleEntries[0], Configuration.Instance) { Status = ProcessBookStatus.Working };
		CompletedBook = new ProcessBookViewModel(sampleEntries[0], Configuration.Instance) { Status = ProcessBookStatus.Completed };
		CancelledBook = new ProcessBookViewModel(sampleEntries[0], Configuration.Instance) { Status = ProcessBookStatus.Cancelled };
		FailedBook = new ProcessBookViewModel(sampleEntries[0], Configuration.Instance) { Status = ProcessBookStatus.Failed };

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
		yield return MockLibraryBook.CreateBook(title: "Some Book 1", subtitle: "The Theming", dateAdded: System.DateTime.Now.AddDays(4)).WithBookStatus(LiberatedStatus.Liberated);
		yield return MockLibraryBook.CreateBook(title: "Some Book 2", dateAdded: System.DateTime.Now.AddDays(3)).WithBookStatus(LiberatedStatus.PartialDownload);
		yield return MockLibraryBook.CreateBook(title: "Some Book 3", dateAdded: System.DateTime.Now.AddDays(2), absetFromLastScan: true).WithPdfStatus(LiberatedStatus.NotLiberated);
		yield return MockLibraryBook.CreateBook(title: "Some Book 4", dateAdded: System.DateTime.Now.AddDays(1)).WithBookStatus(LiberatedStatus.Error);
		yield return MockLibraryBook.CreateBook(title: "Some Series", subtitle: "", contentType: ContentType.Parent).AddSeries("Some Series", 0);
		yield return MockLibraryBook.CreateBook(title: "Some Episode", subtitle: "Episode 1", contentType: ContentType.Episode).AddSeries("Some Series", 1);
	}

	private class MockProcessable : FileLiberator.Processable
	{
		public override string Name => nameof(MockProcessable);
		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook) => Task.FromResult(new StatusHandler());
		public override bool Validate(LibraryBook libraryBook) => false;
	}
}