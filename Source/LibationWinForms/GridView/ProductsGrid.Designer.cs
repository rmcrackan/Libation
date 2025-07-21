using LibationUiBase.GridView;

namespace LibationWinForms.GridView
{
	partial class ProductsGrid
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			gridEntryDataGridView = new System.Windows.Forms.DataGridView();
			removeGVColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			liberateGVColumn = new LiberateDataGridViewImageButtonColumn();
			coverGVColumn = new System.Windows.Forms.DataGridViewImageColumn();
			titleGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			authorsGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			narratorsGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			lengthGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			seriesGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			seriesOrderGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			descriptionGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			categoryGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			productRatingGVColumn = new MyRatingGridViewColumn();
			purchaseDateGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			myRatingGVColumn = new MyRatingGridViewColumn();
			miscGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			lastDownloadedGVColumn = new LastDownloadedGridViewColumn();
			tagAndDetailsGVColumn = new EditTagsDataGridViewImageButtonColumn();
			showHideColumnsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
			syncBindingSource = new SyncBindingSource(components);
			((System.ComponentModel.ISupportInitialize)gridEntryDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)syncBindingSource).BeginInit();
			SuspendLayout();
			// 
			// gridEntryDataGridView
			// 
			gridEntryDataGridView.AllowUserToAddRows = false;
			gridEntryDataGridView.AllowUserToDeleteRows = false;
			gridEntryDataGridView.AllowUserToOrderColumns = true;
			gridEntryDataGridView.AllowUserToResizeRows = false;
			gridEntryDataGridView.AutoGenerateColumns = false;
			gridEntryDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			gridEntryDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { removeGVColumn, liberateGVColumn, coverGVColumn, titleGVColumn, authorsGVColumn, narratorsGVColumn, lengthGVColumn, seriesGVColumn, seriesOrderGVColumn, descriptionGVColumn, categoryGVColumn, productRatingGVColumn, purchaseDateGVColumn, myRatingGVColumn, miscGVColumn, lastDownloadedGVColumn, tagAndDetailsGVColumn });
			gridEntryDataGridView.ContextMenuStrip = showHideColumnsContextMenuStrip;
			gridEntryDataGridView.DataSource = syncBindingSource;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			gridEntryDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
			gridEntryDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			gridEntryDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			gridEntryDataGridView.Location = new System.Drawing.Point(0, 0);
			gridEntryDataGridView.Margin = new System.Windows.Forms.Padding(6);
			gridEntryDataGridView.Name = "gridEntryDataGridView";
			gridEntryDataGridView.RowHeadersVisible = false;
			gridEntryDataGridView.RowHeadersWidth = 82;
			gridEntryDataGridView.RowTemplate.Height = 82;
			gridEntryDataGridView.Size = new System.Drawing.Size(3140, 760);
			gridEntryDataGridView.TabIndex = 0;
			gridEntryDataGridView.CellContentClick += DataGridView_CellContentClick;
			gridEntryDataGridView.CellToolTipTextNeeded += gridEntryDataGridView_CellToolTipTextNeeded;
			// 
			// removeGVColumn
			// 
			removeGVColumn.DataPropertyName = "Remove";
			removeGVColumn.FalseValue = "";
			removeGVColumn.Frozen = true;
			removeGVColumn.HeaderText = "Remove";
			removeGVColumn.IndeterminateValue = "";
			removeGVColumn.MinimumWidth = 60;
			removeGVColumn.Name = "removeGVColumn";
			removeGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			removeGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			removeGVColumn.ThreeState = true;
			removeGVColumn.TrueValue = "";
			removeGVColumn.Width = 60;
			// 
			// liberateGVColumn
			// 
			liberateGVColumn.DataPropertyName = "Liberate";
			liberateGVColumn.HeaderText = "Liberate";
			liberateGVColumn.MinimumWidth = 10;
			liberateGVColumn.Name = "liberateGVColumn";
			liberateGVColumn.ReadOnly = true;
			liberateGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			liberateGVColumn.ScaleFactor = 0F;
			liberateGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			liberateGVColumn.Width = 75;
			// 
			// coverGVColumn
			// 
			coverGVColumn.DataPropertyName = "Cover";
			coverGVColumn.HeaderText = "Cover";
			coverGVColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
			coverGVColumn.MinimumWidth = 10;
			coverGVColumn.Name = "coverGVColumn";
			coverGVColumn.ReadOnly = true;
			coverGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			coverGVColumn.ToolTipText = "Cover Art";
			coverGVColumn.Width = 80;
			// 
			// titleGVColumn
			// 
			titleGVColumn.DataPropertyName = "Title";
			titleGVColumn.HeaderText = "Title";
			titleGVColumn.MinimumWidth = 10;
			titleGVColumn.Name = "titleGVColumn";
			titleGVColumn.ReadOnly = true;
			titleGVColumn.Width = 200;
			// 
			// authorsGVColumn
			// 
			authorsGVColumn.DataPropertyName = "Authors";
			authorsGVColumn.HeaderText = "Authors";
			authorsGVColumn.MinimumWidth = 10;
			authorsGVColumn.Name = "authorsGVColumn";
			authorsGVColumn.ReadOnly = true;
			authorsGVColumn.Width = 100;
			// 
			// narratorsGVColumn
			// 
			narratorsGVColumn.DataPropertyName = "Narrators";
			narratorsGVColumn.HeaderText = "Narrators";
			narratorsGVColumn.MinimumWidth = 10;
			narratorsGVColumn.Name = "narratorsGVColumn";
			narratorsGVColumn.ReadOnly = true;
			narratorsGVColumn.Width = 100;
			// 
			// lengthGVColumn
			// 
			lengthGVColumn.DataPropertyName = "Length";
			lengthGVColumn.HeaderText = "Length";
			lengthGVColumn.MinimumWidth = 10;
			lengthGVColumn.Name = "lengthGVColumn";
			lengthGVColumn.ReadOnly = true;
			lengthGVColumn.ToolTipText = "Recording Length";
			lengthGVColumn.Width = 100;
			// 
			// seriesGVColumn
			// 
			seriesGVColumn.DataPropertyName = "Series";
			seriesGVColumn.HeaderText = "Series";
			seriesGVColumn.MinimumWidth = 10;
			seriesGVColumn.Name = "seriesGVColumn";
			seriesGVColumn.ReadOnly = true;
			seriesGVColumn.Width = 100;
			// 
			// seriesOrderGVColumn
			// 
			seriesOrderGVColumn.DataPropertyName = "SeriesOrder";
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			seriesOrderGVColumn.DefaultCellStyle = dataGridViewCellStyle1;
			seriesOrderGVColumn.HeaderText = "Series\r\nOrder";
			seriesOrderGVColumn.MinimumWidth = 10;
			seriesOrderGVColumn.Name = "seriesOrderGVColumn";
			seriesOrderGVColumn.ReadOnly = true;
			seriesOrderGVColumn.Width = 60;
			// 
			// descriptionGVColumn
			// 
			descriptionGVColumn.DataPropertyName = "Description";
			descriptionGVColumn.HeaderText = "Description";
			descriptionGVColumn.MinimumWidth = 10;
			descriptionGVColumn.Name = "descriptionGVColumn";
			descriptionGVColumn.ReadOnly = true;
			descriptionGVColumn.Width = 100;
			// 
			// categoryGVColumn
			// 
			categoryGVColumn.DataPropertyName = "Category";
			categoryGVColumn.HeaderText = "Category";
			categoryGVColumn.MinimumWidth = 10;
			categoryGVColumn.Name = "categoryGVColumn";
			categoryGVColumn.ReadOnly = true;
			categoryGVColumn.Width = 100;
			// 
			// productRatingGVColumn
			// 
			productRatingGVColumn.DataPropertyName = "ProductRating";
			productRatingGVColumn.HeaderText = "Product Rating";
			productRatingGVColumn.MinimumWidth = 10;
			productRatingGVColumn.Name = "productRatingGVColumn";
			productRatingGVColumn.ReadOnly = true;
			productRatingGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			productRatingGVColumn.Width = 112;
			// 
			// purchaseDateGVColumn
			// 
			purchaseDateGVColumn.DataPropertyName = "PurchaseDate";
			purchaseDateGVColumn.HeaderText = "Purchase Date";
			purchaseDateGVColumn.MinimumWidth = 10;
			purchaseDateGVColumn.Name = "purchaseDateGVColumn";
			purchaseDateGVColumn.ReadOnly = true;
			purchaseDateGVColumn.Width = 100;
			// 
			// myRatingGVColumn
			// 
			myRatingGVColumn.DataPropertyName = "MyRating";
			myRatingGVColumn.HeaderText = "My Rating";
			myRatingGVColumn.MinimumWidth = 10;
			myRatingGVColumn.Name = "myRatingGVColumn";
			myRatingGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			myRatingGVColumn.Width = 112;
			// 
			// miscGVColumn
			// 
			miscGVColumn.DataPropertyName = "Misc";
			miscGVColumn.HeaderText = "Misc";
			miscGVColumn.MinimumWidth = 10;
			miscGVColumn.Name = "miscGVColumn";
			miscGVColumn.ReadOnly = true;
			miscGVColumn.Width = 140;
			// 
			// lastDownloadedGVColumn
			// 
			lastDownloadedGVColumn.DataPropertyName = "LastDownload";
			lastDownloadedGVColumn.HeaderText = "Last Download";
			lastDownloadedGVColumn.MinimumWidth = 10;
			lastDownloadedGVColumn.Name = "lastDownloadedGVColumn";
			lastDownloadedGVColumn.ReadOnly = true;
			lastDownloadedGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			lastDownloadedGVColumn.Width = 108;
			// 
			// tagAndDetailsGVColumn
			// 
			tagAndDetailsGVColumn.DataPropertyName = "BookTags";
			tagAndDetailsGVColumn.HeaderText = "Tags and Details";
			tagAndDetailsGVColumn.MinimumWidth = 10;
			tagAndDetailsGVColumn.Name = "tagAndDetailsGVColumn";
			tagAndDetailsGVColumn.ReadOnly = true;
			tagAndDetailsGVColumn.ScaleFactor = 0F;
			tagAndDetailsGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			tagAndDetailsGVColumn.Width = 100;
			// 
			// showHideColumnsContextMenuStrip
			// 
			showHideColumnsContextMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
			showHideColumnsContextMenuStrip.Name = "contextMenuStrip1";
			showHideColumnsContextMenuStrip.ShowCheckMargin = true;
			showHideColumnsContextMenuStrip.Size = new System.Drawing.Size(83, 4);
			// 
			// syncBindingSource
			// 
			syncBindingSource.DataSource = typeof(GridEntry);
			// 
			// ProductsGrid
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			AutoScroll = true;
			Controls.Add(gridEntryDataGridView);
			Name = "ProductsGrid";
			Size = new System.Drawing.Size(1570, 380);
			Load += new System.EventHandler(ProductsGrid_Load);
			((System.ComponentModel.ISupportInitialize)(gridEntryDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(syncBindingSource)).EndInit();
			ResumeLayout(false);

		}


		#endregion
		private System.Windows.Forms.DataGridView gridEntryDataGridView;
		private System.Windows.Forms.ContextMenuStrip showHideColumnsContextMenuStrip;
		private SyncBindingSource syncBindingSource;
		private System.Windows.Forms.DataGridViewCheckBoxColumn removeGVColumn;
		private LiberateDataGridViewImageButtonColumn liberateGVColumn;
		private System.Windows.Forms.DataGridViewImageColumn coverGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn titleGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn authorsGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn narratorsGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn lengthGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn seriesGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn seriesOrderGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn descriptionGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn categoryGVColumn;
		private MyRatingGridViewColumn productRatingGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn purchaseDateGVColumn;
		private MyRatingGridViewColumn myRatingGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn miscGVColumn;
		private LastDownloadedGridViewColumn lastDownloadedGVColumn;
		private EditTagsDataGridViewImageButtonColumn tagAndDetailsGVColumn;
	}
}
