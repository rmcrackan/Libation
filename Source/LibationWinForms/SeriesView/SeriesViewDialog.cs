using AudibleApi.Common;
using DataLayer;
using Dinah.Core;
using System.ComponentModel;
using System.Windows.Forms;
using System;
using Dinah.Core.WindowsDesktop.Forms;
using LibationWinForms.GridView;
using LibationFileManager;
using LibationUiBase.SeriesView;
using System.Drawing;

namespace LibationWinForms.SeriesView
{
	public partial class SeriesViewDialog : Form
	{
		private readonly LibraryBook LibraryBook;

		public SeriesViewDialog()
		{
			InitializeComponent();
			this.RestoreSizeAndLocation(Configuration.Instance);
			this.SetLibationIcon();

			Load += SeriesViewDialog_Load;
			FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);
		}

		public SeriesViewDialog(LibraryBook libraryBook) : this()
		{
			LibraryBook = ArgumentValidator.EnsureNotNull(libraryBook, "libraryBook");
		}

		private async void SeriesViewDialog_Load(object sender, EventArgs e)
		{
			try
			{
				var seriesEntries = await SeriesItem.GetAllSeriesItemsAsync(LibraryBook);

				//Create a DataGridView for each series and add all children of that series to it.
				foreach (var series in seriesEntries.Keys)
				{
					var dgv = createNewSeriesGrid();
					dgv.CellContentClick += Dgv_CellContentClick;
					dgv.DataSource = new SeriesEntryBindingList(seriesEntries[series]);
					dgv.BindingContextChanged += (_, _) => dgv.Sort(dgv.Columns["Order"], ListSortDirection.Ascending);
					dgv.EnableHeadersVisualStyles = !Application.IsDarkModeEnabled;

					var tab = new TabPage { Text = series.Title };
					tab.Controls.Add(dgv);
					tab.VisibleChanged += (_, _) => dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
					tabControl1.Controls.Add(tab);
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error loading searies info");

				var tab = new TabPage { Text = "ERROR" };
				tab.Controls.Add(new Label { Text = "ERROR LOADING SERIES INFO\r\n\r\n" + ex.Message, ForeColor = Color.Red, Dock = DockStyle.Fill });
				tabControl1.Controls.Add(tab);
			}
		}

		private ImageDisplay imageDisplay;

		private async void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			var dgv = (DataGridView)sender;
			var sentry = dgv.GetBoundItem<SeriesItem>(e.RowIndex);

			if (dgv.Columns[e.ColumnIndex].DataPropertyName == nameof(SeriesItem.Cover))
			{
				coverClicked(sentry.Item);
				return;
			}
			else if (dgv.Columns[e.ColumnIndex].DataPropertyName == nameof(SeriesItem.Title))
			{
				sentry.ViewOnAudible(LibraryBook.Book.Locale);
				return;
			}
			else if (dgv.Columns[e.ColumnIndex].DataPropertyName == nameof(SeriesItem.Button) && sentry.Button.HasButtonAction)
			{
				await sentry.Button.PerformClickAsync(LibraryBook);
			}
		}

		private void coverClicked(Item libraryBook)
		{
			var picDef = new PictureDefinition(libraryBook.PictureLarge ?? libraryBook.PictureId, PictureSize.Native);

			void PictureCached(object sender, PictureCachedEventArgs e)
			{
				if (e.Definition.PictureId == picDef.PictureId)
					imageDisplay.SetCoverArt(e.Picture);

				PictureStorage.PictureCached -= PictureCached;
			}

			PictureStorage.PictureCached += PictureCached;
			(bool isDefault, byte[] initialImageBts) = PictureStorage.GetPicture(picDef);

			var windowTitle = $"{libraryBook.Title} - Cover";

			if (imageDisplay is null || imageDisplay.IsDisposed || !imageDisplay.Visible)
			{
				imageDisplay = new ImageDisplay();
				imageDisplay.RestoreSizeAndLocation(Configuration.Instance);
				imageDisplay.FormClosed += (_, _) => imageDisplay.SaveSizeAndLocation(Configuration.Instance);
			}

			imageDisplay.Text = windowTitle;
			imageDisplay.SetCoverArt(initialImageBts);
			if (!isDefault)
				PictureStorage.PictureCached -= PictureCached;

			if (!imageDisplay.Visible)
				imageDisplay.Show();
		}

		private static DataGridView createNewSeriesGrid()
		{
			var dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				RowHeadersVisible = false,
				ReadOnly = false,
				ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				AllowUserToResizeRows = false,
				AutoGenerateColumns = false
			};

			dgv.RowTemplate.Height = 80;

			dgv.Columns.Add(new DataGridViewImageColumn
			{
				DataPropertyName = nameof(SeriesItem.Cover),
				HeaderText = "Cover",
				Name = "Cover",
				ReadOnly = true,
				Resizable = DataGridViewTriState.False,
				Width = 80
			});
			dgv.Columns.Add(new DataGridViewTextBoxColumn
			{
				DataPropertyName = nameof(SeriesItem.Order),
				HeaderText = "Series\r\nOrder",
				Name = "Order",
				ReadOnly = true,
				SortMode = DataGridViewColumnSortMode.Automatic,
				Width = 50
			});
			dgv.Columns.Add(new DownloadButtonColumn
			{
				DataPropertyName = nameof(SeriesItem.Button),
				HeaderText = "Availability",
				Name = "DownloadButton",
				ReadOnly = true,
				SortMode = DataGridViewColumnSortMode.Automatic,
				Width = 50
			});
			dgv.Columns.Add(new DataGridViewLinkColumn
			{
				DataPropertyName = nameof(SeriesItem.Title),
				HeaderText = "Title",
				Name = "Title",
				ReadOnly = true,
				TrackVisitedState = true,
				SortMode = DataGridViewColumnSortMode.Automatic,
				Width = 200,
				LinkColor = ThemeExtensions.LinkColor,
				VisitedLinkColor = ThemeExtensions.VisitedLinkColor,
			});

			dgv.CellToolTipTextNeeded += Dgv_CellToolTipTextNeeded;

			return dgv;
		}

		private static void Dgv_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
		{
			if (sender is not DataGridView dgv || e.ColumnIndex < 0) return;

			e.ToolTipText = dgv.Columns[e.ColumnIndex].DataPropertyName switch
			{
				nameof(SeriesItem.Cover) => "Click to see full size",
				nameof(SeriesItem.Title) => "Open Audible product page",
				_ => string.Empty
			};
		}
	}
}
