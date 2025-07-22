using ApplicationServices;
using AudibleApi.Common;
using DataLayer;
using FileLiberator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class BookRecordsDialog : Form
	{
		private readonly Func<ScrollBar> VScrollBar;
		private readonly LibraryBook libraryBook;
		private BookRecordBindingList bookRecordEntries;

		public BookRecordsDialog()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				//Prevent the designer from auto-generating columns
				dataGridView1.AutoGenerateColumns = false;
				dataGridView1.DataSource = syncBindingSource;
			}

			this.SetLibationIcon();

			VScrollBar =
				typeof(DataGridView)
				.GetProperty("VerticalScrollBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.GetMethod
				.CreateDelegate<Func<ScrollBar>>(dataGridView1);

			this.RestoreSizeAndLocation(LibationFileManager.Configuration.Instance);
			FormClosing += (_, _) => this.SaveSizeAndLocation(LibationFileManager.Configuration.Instance);
		}

		public BookRecordsDialog(LibraryBook libraryBook) : this()
		{
			this.libraryBook = libraryBook;

			Text = $"{libraryBook.Book.TitleWithSubtitle} - Clips and Bookmarks";
		}

		private async void BookRecordsDialog_Shown(object sender, EventArgs e)
		{
			try
			{
				var api = await libraryBook.GetApiAsync();
				var records = await api.GetRecordsAsync(libraryBook.Book.AudibleProductId);
				
				bookRecordEntries = new BookRecordBindingList(records.Select(r => new BookRecordEntry(r)));
			}
			catch(Exception ex)
			{
				Serilog.Log.Error(ex, "Failed to retrieve records for {libraryBook}", libraryBook);
				bookRecordEntries = new();
			}
			finally
			{
				syncBindingSource.DataSource = bookRecordEntries;

				//Autosize columns and resize form to column width so no horizontal scroll bar is necessary.
				dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
				var columnWidth = dataGridView1.Columns.OfType<DataGridViewColumn>().Sum(c => c.Width);
				Width = Width - dataGridView1.Width + columnWidth + dataGridView1.Margin.Right + (VScrollBar().Visible? VScrollBar().ClientSize.Width : 0);
			}
		}

		#region Buttons

		private void setControlEnabled(object control, bool enabled)
		{
			if (control is Control c)
			{
				if (c.InvokeRequired)
					c.Invoke(new MethodInvoker(() =>
					{
						c.Enabled = enabled;
						c.Focus();
					}));
				else
				{
					c.Enabled = enabled;
					c.Focus();
				}
			}
		}

		private async void exportCheckedBtn_Click(object sender, EventArgs e)
		{
			setControlEnabled(sender, false);
			await saveRecords(bookRecordEntries.Where(r => r.IsChecked).Select(r => r.Record));
			setControlEnabled(sender, true);
		}

		private async void exportAllBtn_Click(object sender, EventArgs e)
		{
			setControlEnabled(sender, false);
			await saveRecords(bookRecordEntries.Select(r => r.Record));
			setControlEnabled(sender, true);
		}

		private void uncheckAllBtn_Click(object sender, EventArgs e)
		{
			foreach (var record in bookRecordEntries)
				record.IsChecked = false;
		}

		private void checkAllBtn_Click(object sender, EventArgs e)
		{
			foreach (var record in bookRecordEntries)
				record.IsChecked = true;
		}

		private async void deleteCheckedBtn_Click(object sender, EventArgs e)
		{
			var records = bookRecordEntries.Where(r => r.IsChecked).Select(r => r.Record).ToList();

			if (!records.Any()) return;

			setControlEnabled(sender, false);

			bool success = false;
			try
			{
				var api = await libraryBook.GetApiAsync();
				success = await api.DeleteRecordsAsync(libraryBook.Book.AudibleProductId, records);
				records = await api.GetRecordsAsync(libraryBook.Book.AudibleProductId);

				var removed = bookRecordEntries.ExceptBy(records, r => r.Record).ToList();

				foreach (var r in removed)
					bookRecordEntries.Remove(r);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, ex.Message);
			}
			finally { setControlEnabled(sender, true); }

			if (!success)
				MessageBox.Show(this, $"Libation was unable to delete the {records.Count} selected records", "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private async void reloadAllBtn_Click(object sender, EventArgs e)
		{
			setControlEnabled(sender, false);

			try
			{
				var api = await libraryBook.GetApiAsync();
				var records = await api.GetRecordsAsync(libraryBook.Book.AudibleProductId);

				bookRecordEntries = new BookRecordBindingList(records.Select(r => new BookRecordEntry(r)));
				syncBindingSource.DataSource = bookRecordEntries;
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, ex.Message);
				MessageBox.Show(this, $"Libation was unable to to reload records", "Reload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { setControlEnabled(sender, true); }
		}

		#endregion

		private async Task saveRecords(IEnumerable<IRecord> records)
		{
			if (!records.Any()) return;

			try
			{
				var saveFileDialog = 
					Invoke(() => new SaveFileDialog
					{
						Title = "Where to export records",
						AddExtension = true,
						FileName = $"{libraryBook.Book.TitleWithSubtitle} - Records",
						DefaultExt = "xlsx",
						Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv|JSON files (*.json)|*.json" // + "|All files (*.*)|*.*"
					});

				if (Invoke(saveFileDialog.ShowDialog) != DialogResult.OK)
					return;

				// FilterIndex is 1-based, NOT 0-based
				switch (saveFileDialog.FilterIndex)
				{
					case 1: // xlsx
					default:
						await Task.Run(() => RecordExporter.ToXlsx(saveFileDialog.FileName, records));
						break;
					case 2: // csv
						await Task.Run(() => RecordExporter.ToCsv(saveFileDialog.FileName, records));
						break;
					case 3: // json
						await Task.Run(() => RecordExporter.ToJson(saveFileDialog.FileName, libraryBook, records));
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(this, "Error attempting to export your library.", "Error exporting", ex);
			}
		}
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape) Close();
			base.OnKeyDown(e);
		}

		#region DataGridView Bindings

		private class BookRecordBindingList : BindingList<BookRecordEntry>
		{
			private PropertyDescriptor _propertyDescriptor;
			private ListSortDirection _listSortDirection;
			private bool _isSortedCore;

			protected override PropertyDescriptor SortPropertyCore => _propertyDescriptor;
			protected override ListSortDirection SortDirectionCore => _listSortDirection;
			protected override bool IsSortedCore => _isSortedCore;
			protected override bool SupportsSortingCore => true;
			public BookRecordBindingList() : base(new List<BookRecordEntry>()) { }
			public BookRecordBindingList(IEnumerable<BookRecordEntry> records) : base(records.ToList()) { }
			protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
			{
				var itemsList = (List<BookRecordEntry>)Items;

				var sorted =
					direction is ListSortDirection.Ascending ? itemsList.OrderBy(prop.GetValue).ToList()
					: itemsList.OrderByDescending(prop.GetValue).ToList();

				itemsList.Clear();
				itemsList.AddRange(sorted);

				_propertyDescriptor = prop;
				_listSortDirection = direction;
				_isSortedCore = true;

				OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			}
		}

		private class BookRecordEntry : LibationUiBase.ReactiveObject
		{
			private const string DateFormat = "yyyy-MM-dd HH\\:mm";
			private bool _ischecked;
			public IRecord Record { get; }
			public bool IsChecked { get => _ischecked; set => RaiseAndSetIfChanged(ref _ischecked, value); }
			public string Type => Record.GetType().Name;
			public string Start => formatTimeSpan(Record.Start);
			public string Created => Record.Created.ToString(DateFormat);
			public string Modified => Record is IAnnotation annotation ? annotation.Created.ToString(DateFormat) : string.Empty;
			public string End => Record is IRangeAnnotation range ? formatTimeSpan(range.End) : string.Empty;
			public string Note => Record is IRangeAnnotation range ? range.Text : string.Empty;
			public string Title => Record is Clip range ? range.Title : string.Empty;
			public BookRecordEntry(IRecord record) => Record = record;

			private static string formatTimeSpan(TimeSpan timeSpan)
			{
				int h = (int)timeSpan.TotalHours;
				int m = timeSpan.Minutes;
				int s = timeSpan.Seconds;
				int ms = timeSpan.Milliseconds;

				return ms == 0 ? $"{h:d2}:{m:d2}:{s:d2}" : $"{h:d2}:{m:d2}:{s:d2}.{ms:d3}";
			}
		}

		#endregion
	}
}
