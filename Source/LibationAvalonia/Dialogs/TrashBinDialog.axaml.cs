using ApplicationServices;
using DataLayer;
using Dinah.Core.Collections.Generic;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class TrashBinDialog : DialogWindow
{
	private TrashBinViewModel VM { get; }
	public TrashBinDialog()
	{
		InitializeComponent();
		SaveOnEnter = false;
		ControlToFocusOnShow = searchTb;
		DataContext = VM = new TrashBinViewModel();

		Closing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);
		Loaded += async (_, _) => await VM.InitAsync();
	}
}

public class TrashBinViewModel : ViewModelBase
{
	private TempSearchEngine SearchEngine { get; } = new();
	public ProductsDisplayViewModel ProductsDisplay { get; }
	public string? CheckedCountText { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public string? AudiblePlusCheckedCountText { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool ControlsEnabled { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public string? FilterString { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }

	private bool? m_everythingChecked = false;
	private bool? m_audiblePlusChecked = false;
	public bool? EverythingChecked
	{
		get => m_everythingChecked;
		set
		{
			m_everythingChecked = value ?? false;
			SetVisibleChecked(_ => true, m_everythingChecked.Value);
		}
	}
	public bool? AudiblePlusChecked
	{
		get => m_audiblePlusChecked;
		set
		{
			m_audiblePlusChecked = value ?? false;
			SetVisibleChecked(e => e.IsAudiblePlus, m_audiblePlusChecked.Value);
		}
	}

	public TrashBinViewModel()
	{
		ProductsDisplay = new() { SearchEngine = SearchEngine };
		ProductsDisplay.RemovableCountChanged += (_, _) => UpdateCounts();
		ProductsDisplay.VisibleCountChanged += (_, _) => UpdateCounts();
	}

	public async Task InitAsync()
	{
		var deletedBooks = GetDeletedLibraryBooks();
		SearchEngine.ReindexSearchEngine(deletedBooks);
		await ProductsDisplay.BindToGridAsync(deletedBooks);
		await ProductsDisplay.ScanAndRemoveBooksAsync();
		ControlsEnabled = true;
	}

	private async Task ReloadAsync()
	{
		var deletedBooks = GetDeletedLibraryBooks();
		SearchEngine.ReindexSearchEngine(deletedBooks);
		await ProductsDisplay.UpdateGridAsync(deletedBooks);
	}

	public void SetVisibleChecked(Func<LibraryBook, bool> predicate, bool isChecked)
	{
		ProductsDisplay.GetVisibleGridEntries().Where(e => predicate(e.LibraryBook)).ForEach(i => i.Remove = isChecked);
	}

	private IEnumerable<LibraryBook> GetCheckedBooks() => ProductsDisplay.GetVisibleGridEntries().Where(i => i.Remove is true).Select(i => i.LibraryBook);

	private void UpdateCounts()
	{
		var visible = ProductsDisplay.GetVisibleGridEntries().ToArray();
		var plusVisibleCount = visible.Count(e => e.LibraryBook.IsAudiblePlus);

		var checkedCount = visible.Count(e => e.Remove is true);
		var plusCheckedCount = visible.Count(e => e.LibraryBook.IsAudiblePlus && e.Remove is true);

		CheckedCountText = $"Checked: {checkedCount} of {visible.Length}";
		AudiblePlusCheckedCountText = $"Checked: {plusCheckedCount} of {plusVisibleCount}";

		bool? everythingChecked = checkedCount == 0 || visible.Length == 0 ? false
			: checkedCount == visible.Length ? true
			: null;

		bool? audiblePlusChecked = plusCheckedCount == 0 || plusVisibleCount == 0 ? false
			: plusCheckedCount == plusVisibleCount ? true
			: null;

		this.RaiseAndSetIfChanged(ref m_everythingChecked, everythingChecked, nameof(EverythingChecked));
		this.RaiseAndSetIfChanged(ref m_audiblePlusChecked, audiblePlusChecked, nameof(AudiblePlusChecked));
	}

	public async Task FilterBtnAsync()
	{
		var lastGood = ProductsDisplay.FilterString;
		try
		{
			await ProductsDisplay.Filter(FilterString);
		}
		catch
		{
			await ProductsDisplay.Filter(lastGood);
			FilterString = lastGood;
		}
	}

	public async Task RestoreCheckedAsync()
	{
		ControlsEnabled = false;
		var qtyChanges = await GetCheckedBooks().RestoreBooksAsync();
		if (qtyChanges > 0)
			await ReloadAsync();
		ControlsEnabled = true;
	}

	public async Task PermanentlyDeleteCheckedAsync()
	{
		ControlsEnabled = false;
		var qtyChanges = await GetCheckedBooks().PermanentlyDeleteBooksAsync();
		if (qtyChanges > 0)
			await ReloadAsync();
		ControlsEnabled = true;
	}

	private static List<LibraryBook> GetDeletedLibraryBooks()
	{
#if DEBUG
		if (Avalonia.Controls.Design.IsDesignMode)
		{
			return [
					MockLibraryBook.CreateBook(title: "Mock Audible Plus Library Book 4", isAudiblePlus: true),
					MockLibraryBook.CreateBook(title: "Mock Audible Plus Library Book 3", isAudiblePlus: true),
					MockLibraryBook.CreateBook(title: "Mock Library Book 2"),
					MockLibraryBook.CreateBook(title: "Mock Library Book 1"),
				];
		}
#endif
		return DbContexts.GetDeletedLibraryBooks();
	}
}