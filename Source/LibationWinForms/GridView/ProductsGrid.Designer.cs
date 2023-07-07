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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.gridEntryDataGridView = new System.Windows.Forms.DataGridView();
			this.removeGVColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.liberateGVColumn = new LibationWinForms.GridView.LiberateDataGridViewImageButtonColumn();
			this.coverGVColumn = new CoverGridViewColumn();
			this.titleGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.authorsGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.narratorsGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lengthGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.seriesGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.seriesOrderGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.descriptionGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.categoryGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.productRatingGVColumn = new LibationWinForms.GridView.MyRatingGridViewColumn();
			this.purchaseDateGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.myRatingGVColumn = new LibationWinForms.GridView.MyRatingGridViewColumn();
			this.miscGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lastDownloadedGVColumn = new LastDownloadedGridViewColumn();
			this.tagAndDetailsGVColumn = new LibationWinForms.GridView.EditTagsDataGridViewImageButtonColumn();
			this.showHideColumnsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.syncBindingSource = new LibationWinForms.GridView.SyncBindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.gridEntryDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.syncBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// gridEntryDataGridView
			// 
			this.gridEntryDataGridView.AllowUserToAddRows = false;
			this.gridEntryDataGridView.AllowUserToDeleteRows = false;
			this.gridEntryDataGridView.AllowUserToOrderColumns = true;
			this.gridEntryDataGridView.AllowUserToResizeRows = false;
			this.gridEntryDataGridView.AutoGenerateColumns = false;
			this.gridEntryDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridEntryDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.removeGVColumn,
            this.liberateGVColumn,
            this.coverGVColumn,
            this.titleGVColumn,
            this.authorsGVColumn,
            this.narratorsGVColumn,
            this.lengthGVColumn,
            this.seriesGVColumn,
			this.seriesOrderGVColumn,
			this.descriptionGVColumn,
            this.categoryGVColumn,
            this.productRatingGVColumn,
            this.purchaseDateGVColumn,
            this.myRatingGVColumn,
            this.miscGVColumn,
			this.lastDownloadedGVColumn,
			this.tagAndDetailsGVColumn});
			this.gridEntryDataGridView.ContextMenuStrip = this.showHideColumnsContextMenuStrip;
			this.gridEntryDataGridView.DataSource = this.syncBindingSource;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.gridEntryDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
			this.gridEntryDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridEntryDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.gridEntryDataGridView.Location = new System.Drawing.Point(0, 0);
			this.gridEntryDataGridView.Name = "gridEntryDataGridView";
			this.gridEntryDataGridView.RowHeadersVisible = false;
			this.gridEntryDataGridView.RowTemplate.Height = 82;
			this.gridEntryDataGridView.Size = new System.Drawing.Size(1570, 380);
			this.gridEntryDataGridView.TabIndex = 0;
			this.gridEntryDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellContentClick);
			this.gridEntryDataGridView.CellToolTipTextNeeded += new System.Windows.Forms.DataGridViewCellToolTipTextNeededEventHandler(this.gridEntryDataGridView_CellToolTipTextNeeded);
			// 
			// removeGVColumn
			// 
			this.removeGVColumn.DataPropertyName = "Remove";
			this.removeGVColumn.FalseValue = "";
			this.removeGVColumn.Frozen = true;
			this.removeGVColumn.HeaderText = "Remove";
			this.removeGVColumn.IndeterminateValue = "";
			this.removeGVColumn.MinimumWidth = 60;
			this.removeGVColumn.Name = "removeGVColumn";
			this.removeGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.removeGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.removeGVColumn.ThreeState = true;
			this.removeGVColumn.TrueValue = "";
			this.removeGVColumn.Width = 60;
			// 
			// liberateGVColumn
			// 
			this.liberateGVColumn.DataPropertyName = "Liberate";
			this.liberateGVColumn.HeaderText = "Liberate";
			this.liberateGVColumn.Name = "liberateGVColumn";
			this.liberateGVColumn.ReadOnly = true;
			this.liberateGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.liberateGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.liberateGVColumn.Width = 75;
			// 
			// coverGVColumn
			// 
			this.coverGVColumn.DataPropertyName = "Cover";
			this.coverGVColumn.HeaderText = "Cover";
			this.coverGVColumn.Name = "coverGVColumn";
			this.coverGVColumn.ReadOnly = true;
			this.coverGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.coverGVColumn.ToolTipText = "Cover Art";
			this.coverGVColumn.Width = 80;
			// 
			// titleGVColumn
			// 
			this.titleGVColumn.DataPropertyName = "Title";
			this.titleGVColumn.HeaderText = "Title";
			this.titleGVColumn.Name = "titleGVColumn";
			this.titleGVColumn.ReadOnly = true;
			this.titleGVColumn.Width = 200;
			// 
			// authorsGVColumn
			// 
			this.authorsGVColumn.DataPropertyName = "Authors";
			this.authorsGVColumn.HeaderText = "Authors";
			this.authorsGVColumn.Name = "authorsGVColumn";
			this.authorsGVColumn.ReadOnly = true;
			this.authorsGVColumn.Width = 100;
			// 
			// narratorsGVColumn
			// 
			this.narratorsGVColumn.DataPropertyName = "Narrators";
			this.narratorsGVColumn.HeaderText = "Narrators";
			this.narratorsGVColumn.Name = "narratorsGVColumn";
			this.narratorsGVColumn.ReadOnly = true;
			this.narratorsGVColumn.Width = 100;
			// 
			// lengthGVColumn
			// 
			this.lengthGVColumn.DataPropertyName = "Length";
			this.lengthGVColumn.HeaderText = "Length";
			this.lengthGVColumn.Name = "lengthGVColumn";
			this.lengthGVColumn.ReadOnly = true;
			this.lengthGVColumn.Width = 100;
			this.lengthGVColumn.ToolTipText = "Recording Length";
			// 
			// seriesGVColumn
			// 
			this.seriesGVColumn.DataPropertyName = "Series";
			this.seriesGVColumn.HeaderText = "Series";
			this.seriesGVColumn.Name = "seriesGVColumn";
			this.seriesGVColumn.ReadOnly = true;
			this.seriesGVColumn.Width = 100;
			// 
			// seriesOrderGVColumn
			// 
			this.seriesOrderGVColumn.DataPropertyName = "SeriesOrder";
			this.seriesOrderGVColumn.HeaderText = "Series\r\nOrder";
			this.seriesOrderGVColumn.Name = "seriesOrderGVColumn";
			this.seriesOrderGVColumn.Width = 60;
			this.seriesOrderGVColumn.ReadOnly = true;
			this.seriesOrderGVColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			// 
			// descriptionGVColumn
			// 
			this.descriptionGVColumn.DataPropertyName = "Description";
			this.descriptionGVColumn.HeaderText = "Description";
			this.descriptionGVColumn.Name = "descriptionGVColumn";
			this.descriptionGVColumn.ReadOnly = true;
			this.descriptionGVColumn.Width = 100;
			this.descriptionGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// categoryGVColumn
			// 
			this.categoryGVColumn.DataPropertyName = "Category";
			this.categoryGVColumn.HeaderText = "Category";
			this.categoryGVColumn.Name = "categoryGVColumn";
			this.categoryGVColumn.ReadOnly = true;
			this.categoryGVColumn.Width = 100;
			// 
			// productRatingGVColumn
			// 
			this.productRatingGVColumn.DataPropertyName = "ProductRating";
			this.productRatingGVColumn.HeaderText = "Product Rating";
			this.productRatingGVColumn.Name = "productRatingGVColumn";
			this.productRatingGVColumn.ReadOnly = true;
			this.productRatingGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.productRatingGVColumn.Width = 112;
			// 
			// purchaseDateGVColumn
			// 
			this.purchaseDateGVColumn.DataPropertyName = "PurchaseDate";
			this.purchaseDateGVColumn.HeaderText = "Purchase Date";
			this.purchaseDateGVColumn.Name = "purchaseDateGVColumn";
			this.purchaseDateGVColumn.ReadOnly = true;
			this.purchaseDateGVColumn.Width = 100;
			// 
			// myRatingGVColumn
			// 
			this.myRatingGVColumn.DataPropertyName = "MyRating";
			this.myRatingGVColumn.HeaderText = "My Rating";
			this.myRatingGVColumn.Name = "myRatingGVColumn";
			this.myRatingGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.myRatingGVColumn.Width = 112;
			// 
			// miscGVColumn
			// 
			this.miscGVColumn.DataPropertyName = "Misc";
			this.miscGVColumn.HeaderText = "Misc";
			this.miscGVColumn.Name = "miscGVColumn";
			this.miscGVColumn.ReadOnly = true;
			this.miscGVColumn.Width = 140;
			// 
			// lastDownloadedGVColumn
			// 
			this.lastDownloadedGVColumn.DataPropertyName = "LastDownload";
			this.lastDownloadedGVColumn.HeaderText = "Last Download";
			this.lastDownloadedGVColumn.Name = "lastDownloadedGVColumn";
			this.lastDownloadedGVColumn.ReadOnly = true;
			this.lastDownloadedGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.lastDownloadedGVColumn.Width = 108;
			// 
			// tagAndDetailsGVColumn
			// 
			this.tagAndDetailsGVColumn.DataPropertyName = "BookTags";
			this.tagAndDetailsGVColumn.HeaderText = "Tags and Details";
			this.tagAndDetailsGVColumn.Name = "tagAndDetailsGVColumn";
			this.tagAndDetailsGVColumn.ReadOnly = true;
			this.tagAndDetailsGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.tagAndDetailsGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.tagAndDetailsGVColumn.Width = 100;
			// 
			// showHideColumnsContextMenuStrip
			// 
			this.showHideColumnsContextMenuStrip.Name = "contextMenuStrip1";
			this.showHideColumnsContextMenuStrip.Size = new System.Drawing.Size(61, 4);
			// 
			// syncBindingSource
			// 
			this.syncBindingSource.DataSource = typeof(IGridEntry);
			// 
			// ProductsGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.Controls.Add(this.gridEntryDataGridView);
			this.Name = "ProductsGrid";
			this.Size = new System.Drawing.Size(1570, 380);
			this.Load += new System.EventHandler(this.ProductsGrid_Load);
			((System.ComponentModel.ISupportInitialize)(this.gridEntryDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.syncBindingSource)).EndInit();
			this.ResumeLayout(false);

		}


		#endregion
		private System.Windows.Forms.DataGridView gridEntryDataGridView;
		private System.Windows.Forms.ContextMenuStrip showHideColumnsContextMenuStrip;
		private SyncBindingSource syncBindingSource;
		private System.Windows.Forms.DataGridViewCheckBoxColumn removeGVColumn;
		private LiberateDataGridViewImageButtonColumn liberateGVColumn;
		private CoverGridViewColumn coverGVColumn;
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
