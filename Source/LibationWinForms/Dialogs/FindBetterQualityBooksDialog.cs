using ApplicationServices;
using LibationUiBase;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class FindBetterQualityBooksDialog : Form
{
	private FindBetterQualityBooksViewModel VM { get; }

	private Task? scanTask;
	public FindBetterQualityBooksDialog()
	{
		InitializeComponent();

		dataGridView1.EnableHeadersVisualStyles = !Application.IsDarkModeEnabled;
		lblScanCount.Visible = btnMarkBooks.Visible = false;
		DataContext = VM = new FindBetterQualityBooksViewModel();
		VM.PropertyChanged += VM_PropertyChanged;
		VM.BookScanned += VM_BookScanned;

		cboxUseWidevine.Text = FindBetterQualityBooksViewModel.UseWidevineSboxText;
		cboxUseWidevine.DataBindings.Add(new Binding(nameof(CheckBox.Checked), VM, nameof(FindBetterQualityBooksViewModel.ScanWidevine)));
		btnScan.DataBindings.Add(new Binding(nameof(Button.Text), VM, nameof(FindBetterQualityBooksViewModel.ScanButtonText)));
		btnScan.Enabled = false;

		this.RestoreSizeAndLocation(LibationFileManager.Configuration.Instance);
		this.SetLibationIcon();
		Shown += Shown_LoadLibrary;
		Shown += Shown_ShowInitialMessage;
		FormClosing += FindBetterQualityBooksDialog_FormClosing;
		SetDoubleBuffer(dataGridView1, true);
	}

	static void SetDoubleBuffer(Control control, bool DoubleBuffered)
	{
		typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, control, [DoubleBuffered]);
	}

	private void Shown_ShowInitialMessage(object? sender, EventArgs e)
	{
		if (!VM.ShowFindBetterQualityBooksHelp)
			return;
		var result = MessageBox.Show(this, FindBetterQualityBooksViewModel.InitialMessage, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
		if (result == DialogResult.No)
		{
			VM.ShowFindBetterQualityBooksHelp = false;
		}
	}

	private async void Shown_LoadLibrary(object? sender, EventArgs e)
	{
		var library = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking());
		var list = library.Where(FindBetterQualityBooksViewModel.ShouldScan).Select(lb => new BookDataViewModel(lb)).ToList();
		VM.Books = new SortBindingList<BookDataViewModel>(list);

		Invoke(() =>
		{
			bookDataViewModelBindingSource.DataSource = VM.Books;
			foreach (DataGridViewRow r in dataGridView1.Rows)
			{
				//Force creation of DefaultCellStyle to speed up later coloring
				//_ = r.DefaultCellStyle;
			}
			btnScan.Enabled = true;
			dataGridView1.CellFormatting += dataGridView1_CellFormatting;
		});
	}

	private void VM_BookScanned(object? sender, BookDataViewModel e)
	{
		Invoke(() => dataGridView1.CurrentCell = dataGridView1.Rows
			.Cast<DataGridViewRow>()
			.FirstOrDefault(r => r.DataBoundItem == e)?
			.Cells[foundFileDataGridViewTextBoxColumn.Index]);
	}

	private void VM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(FindBetterQualityBooksViewModel.IsScanning):
				btnScan.Enabled = true;
				break;
			case nameof(FindBetterQualityBooksViewModel.ScanCount):
				lblScanCount.Visible = !string.IsNullOrEmpty(VM.ScanCount);
				lblScanCount.Text = VM.ScanCount;
				break;
			case nameof(FindBetterQualityBooksViewModel.MarkBooksButtonText):
				btnMarkBooks.Visible = !string.IsNullOrEmpty(VM.MarkBooksButtonText);
				btnMarkBooks.Text = VM.MarkBooksButtonText;
				break;
		}
	}

	private async void FindBetterQualityBooksDialog_FormClosing(object? sender, FormClosingEventArgs e)
	{
		if (scanTask is not null)
		{
			await scanTask;
			scanTask = null;
			//give the UI a moment to update after cancelling the first close
			await Task.Delay(100);
			Invoke(Close);
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (scanTask is not null)
		{
			this.SaveSizeAndLocation(LibationFileManager.Configuration.Instance);
			e.Cancel = true;
			VM.StopScan();
		}
		base.OnFormClosing(e);
	}

	private void btnScan_Click(object sender, EventArgs e)
	{
		btnScan.Enabled = false;
		scanTask = Task.Run(async () =>
		{
			try
			{
				if (VM.IsScanning)
					VM.StopScan();
				else
					await VM.ScanAsync();
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Failed to scan for better quality books");
				MessageBox.Show(this, "An error occurred while scanning for better quality books. Please see the logs for more information.", "Error Scanning Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Invoke(() =>
				{
					VM.IsScanning = false;
					btnScan.Enabled = true;
				});
			}
		});
	}

	private async void btnMarkBooks_Click(object sender, EventArgs e)
	{
		btnMarkBooks.Enabled = false;
		try
		{
			await VM.MarkBooksAsync();
		}
		catch (Exception ex)
		{
			Serilog.Log.Error(ex, "Failed to mark books as Not Liberated");
			MessageBox.Show(this, "An error occurred while marking books as Not Liberated. Please see the logs for more information.", "Error Marking Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			Invoke(() => btnMarkBooks.Enabled = true);
		}
	}

	private void dataGridView1_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
			return;

		var row = dataGridView1.Rows[e.RowIndex];
		if (row.DataBoundItem is BookDataViewModel bvm)
		{
			row.DefaultCellStyle.BackColor = bvm.ScanStatus.GetColor();
		}
	}
}
