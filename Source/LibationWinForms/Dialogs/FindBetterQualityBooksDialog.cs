using ApplicationServices;
using LibationUiBase;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable enable
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
	}


	private void Shown_ShowInitialMessage(object? sender, EventArgs e)
	{
		MessageBox.Show(this, FindBetterQualityBooksViewModel.InitialMessage, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private async void Shown_LoadLibrary(object? sender, EventArgs e)
	{
		var library = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking());
		var list = library.Where(FindBetterQualityBooksViewModel.ShouldScan).Select(lb => new BookDataViewModel(lb)).ToList();
		VM.Books = new SortBindingList<BookDataViewModel>(list);

		Invoke(() =>
		{
			bookDataViewModelBindingSource.DataSource = VM.Books;
			btnScan.Enabled = true;
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

	private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
			return;

		var row = dataGridView1.Rows[e.RowIndex];
		if (row.DataBoundItem is BookDataViewModel bvm)
		{
			///yes, this is tight coupling and bad practice.
			///If we ever need tese colors in a third place,
			///consider moving them to a shared location like
			///App.axaml in LibationAvalonia
			var color = bvm.ScanStatus switch
			{
				BookScanStatus.Completed => ProcessQueue.ProcessBookControl.SuccessColor,
				BookScanStatus.Cancelled => ProcessQueue.ProcessBookControl.CancelledColor,
				BookScanStatus.Error => ProcessQueue.ProcessBookControl.FailedColor,
				_ => ProcessQueue.ProcessBookControl.QueuedColor,
			};
			row.DefaultCellStyle.BackColor = color;
		}
	}
}
