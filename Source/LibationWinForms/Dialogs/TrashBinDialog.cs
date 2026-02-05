using ApplicationServices;
using DataLayer;
using Dinah.Core.Collections.Generic;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class TrashBinDialog : Form
{
	private string lastGoodFilter = "";
	private TempSearchEngine SearchEngine { get; } = new TempSearchEngine();
	public TrashBinDialog()
	{
		InitializeComponent();

		this.SetLibationIcon();
		this.RestoreSizeAndLocation(Configuration.Instance);
		this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);

		deletedCheckedLbl.Text = "";
		plusBookcSheckedLbl.Text = "";
		productsGrid1.SearchEngine = SearchEngine;
		productsGrid1.RemovableCountChanged += (_, _) => UpdateCounts();
		productsGrid1.VisibleCountChanged += (_, _) => UpdateCounts();
		Load += TrashBinDialog_Load;
	}

	private IEnumerable<LibraryBook> GetCheckedBooks() => productsGrid1.GetVisibleGridEntries().Where(i => i.Remove is true).Select(i => i.LibraryBook);

	private async void TrashBinDialog_Load(object? sender, EventArgs e)
	{
		productsGrid1.RemoveColumnVisible = true;
		await InitAsync();
	}

	private void UpdateCounts()
	{
		var visible = productsGrid1.GetVisibleGridEntries().ToArray();
		var plusVisibleCount = visible.Count(e => e.LibraryBook.IsAudiblePlus);

		var checkedCount = visible.Count(e => e.Remove is true);
		var plusCheckedCount = visible.Count(e => e.LibraryBook.IsAudiblePlus && e.Remove is true);

		deletedCheckedLbl.Text = $"Checked: {checkedCount} of {visible.Length}";
		plusBookcSheckedLbl.Text = $"Checked: {plusCheckedCount} of {plusVisibleCount}";

		everythingCb.CheckStateChanged -= everythingCb_CheckStateChanged;
		everythingCb.CheckState = checkedCount == 0 || visible.Length == 0 ? CheckState.Unchecked
			: checkedCount == visible.Length ? CheckState.Checked
			: CheckState.Indeterminate;
		everythingCb.CheckStateChanged += everythingCb_CheckStateChanged;

		audiblePlusCb.CheckStateChanged -= audiblePlusCb_CheckStateChanged;
		audiblePlusCb.CheckState = plusCheckedCount == 0 || plusVisibleCount == 0 ? CheckState.Unchecked
			: plusCheckedCount == plusVisibleCount ? CheckState.Checked
			: CheckState.Indeterminate;
		audiblePlusCb.CheckStateChanged += audiblePlusCb_CheckStateChanged;
	}

	private async Task InitAsync()
	{
		var deletedBooks = DbContexts.GetDeletedLibraryBooks();
		SearchEngine.ReindexSearchEngine(deletedBooks);
		await productsGrid1.BindToGridAsync(deletedBooks);
	}

	private void Reload()
	{
		var deletedBooks = DbContexts.GetDeletedLibraryBooks();
		SearchEngine.ReindexSearchEngine(deletedBooks);
		productsGrid1.UpdateGrid(deletedBooks);
	}

	private async void permanentlyDeleteBtn_Click(object sender, EventArgs e)
	{
		setControlsEnabled(false);

		var qtyChanges = await GetCheckedBooks().PermanentlyDeleteBooksAsync();
		if (qtyChanges > 0)
			Reload();

		setControlsEnabled(true);
	}

	private async void restoreBtn_Click(object sender, EventArgs e)
	{
		setControlsEnabled(false);

		var qtyChanges = await GetCheckedBooks().RestoreBooksAsync();
		if (qtyChanges > 0)
			Reload();

		setControlsEnabled(true);
	}

	private void setControlsEnabled(bool enabled)
		=> Invoke(() => productsGrid1.Enabled = restoreBtn.Enabled = permanentlyDeleteBtn.Enabled = everythingCb.Enabled = enabled);

	private void textBox1_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Enter)
			searchBtn_Click(sender, e);
	}

	private void searchBtn_Click(object sender, EventArgs e)
	{
		try
		{
			productsGrid1.Filter(textBox1.Text);
			lastGoodFilter = textBox1.Text;
		}
		catch
		{
			productsGrid1.Filter(lastGoodFilter);
		}
	}

	private void audiblePlusCb_CheckStateChanged(object? sender, EventArgs e)
	{
		switch (audiblePlusCb.CheckState)
		{
			case CheckState.Checked:
				SetVisibleChecked(e => e.IsAudiblePlus, isChecked: true);
				break;
			case CheckState.Unchecked:
				SetVisibleChecked(e => e.IsAudiblePlus, isChecked: false);
				break;
			default:
				audiblePlusCb.CheckState = CheckState.Unchecked;
				break;
		}
	}
	private void everythingCb_CheckStateChanged(object? sender, EventArgs e)
	{
		switch (everythingCb.CheckState)
		{
			case CheckState.Checked:
				SetVisibleChecked(_ => true, isChecked: true);
				break;
			case CheckState.Unchecked:
				SetVisibleChecked(_ => true, isChecked: false);
				break;
			default:
				everythingCb.CheckState = CheckState.Unchecked;
				break;
		}
	}

	public void SetVisibleChecked(Func<LibraryBook, bool> predicate, bool isChecked)
	{
		productsGrid1.GetVisibleGridEntries().Where(e => predicate(e.LibraryBook)).ForEach(i => i.Remove = isChecked);
		UpdateCounts();
	}
}
