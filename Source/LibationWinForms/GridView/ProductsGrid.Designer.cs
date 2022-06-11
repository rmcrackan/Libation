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
			this.coverGVColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.titleGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.authorsGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.narratorsGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lengthGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.seriesGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.descriptionGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.categoryGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.productRatingGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.purchaseDateGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.myRatingGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.miscGVColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tagAndDetailsGVColumn = new LibationWinForms.GridView.EditTagsDataGridViewImageButtonColumn();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
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
            this.descriptionGVColumn,
            this.categoryGVColumn,
            this.productRatingGVColumn,
            this.purchaseDateGVColumn,
            this.myRatingGVColumn,
            this.miscGVColumn,
            this.tagAndDetailsGVColumn});
			this.gridEntryDataGridView.ContextMenuStrip = this.contextMenuStrip1;
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
			// 
			// narratorsGVColumn
			// 
			this.narratorsGVColumn.DataPropertyName = "Narrators";
			this.narratorsGVColumn.HeaderText = "Narrators";
			this.narratorsGVColumn.Name = "narratorsGVColumn";
			this.narratorsGVColumn.ReadOnly = true;
			// 
			// lengthGVColumn
			// 
			this.lengthGVColumn.DataPropertyName = "Length";
			this.lengthGVColumn.HeaderText = "Length";
			this.lengthGVColumn.Name = "lengthGVColumn";
			this.lengthGVColumn.ReadOnly = true;
			this.lengthGVColumn.ToolTipText = "Recording Length";
			// 
			// seriesGVColumn
			// 
			this.seriesGVColumn.DataPropertyName = "Series";
			this.seriesGVColumn.HeaderText = "Series";
			this.seriesGVColumn.Name = "seriesGVColumn";
			this.seriesGVColumn.ReadOnly = true;
			// 
			// descriptionGVColumn
			// 
			this.descriptionGVColumn.DataPropertyName = "Description";
			this.descriptionGVColumn.HeaderText = "Description";
			this.descriptionGVColumn.Name = "descriptionGVColumn";
			this.descriptionGVColumn.ReadOnly = true;
			this.descriptionGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// categoryGVColumn
			// 
			this.categoryGVColumn.DataPropertyName = "Category";
			this.categoryGVColumn.HeaderText = "Category";
			this.categoryGVColumn.Name = "categoryGVColumn";
			this.categoryGVColumn.ReadOnly = true;
			// 
			// productRatingGVColumn
			// 
			this.productRatingGVColumn.DataPropertyName = "ProductRating";
			this.productRatingGVColumn.HeaderText = "Product Rating";
			this.productRatingGVColumn.Name = "productRatingGVColumn";
			this.productRatingGVColumn.ReadOnly = true;
			this.productRatingGVColumn.Width = 108;
			// 
			// purchaseDateGVColumn
			// 
			this.purchaseDateGVColumn.DataPropertyName = "PurchaseDate";
			this.purchaseDateGVColumn.HeaderText = "Purchase Date";
			this.purchaseDateGVColumn.Name = "purchaseDateGVColumn";
			this.purchaseDateGVColumn.ReadOnly = true;
			// 
			// myRatingGVColumn
			// 
			this.myRatingGVColumn.DataPropertyName = "MyRating";
			this.myRatingGVColumn.HeaderText = "My Rating";
			this.myRatingGVColumn.Name = "myRatingGVColumn";
			this.myRatingGVColumn.ReadOnly = true;
			this.myRatingGVColumn.Width = 108;
			// 
			// miscGVColumn
			// 
			this.miscGVColumn.DataPropertyName = "Misc";
			this.miscGVColumn.HeaderText = "Misc";
			this.miscGVColumn.Name = "miscGVColumn";
			this.miscGVColumn.ReadOnly = true;
			this.miscGVColumn.Width = 135;
			// 
			// tagAndDetailsGVColumn
			// 
			this.tagAndDetailsGVColumn.DataPropertyName = "DisplayTags";
			this.tagAndDetailsGVColumn.HeaderText = "Tags and Details";
			this.tagAndDetailsGVColumn.Name = "tagAndDetailsGVColumn";
			this.tagAndDetailsGVColumn.ReadOnly = true;
			this.tagAndDetailsGVColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.tagAndDetailsGVColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
			// 
			// syncBindingSource
			// 
			this.syncBindingSource.DataSource = typeof(LibationWinForms.GridView.GridEntry);
			// 
			// ProductsGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private SyncBindingSource syncBindingSource;
		private System.Windows.Forms.DataGridViewCheckBoxColumn removeGVColumn;
		private LiberateDataGridViewImageButtonColumn liberateGVColumn;
		private System.Windows.Forms.DataGridViewImageColumn coverGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn titleGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn authorsGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn narratorsGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn lengthGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn seriesGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn descriptionGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn categoryGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn productRatingGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn purchaseDateGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn myRatingGVColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn miscGVColumn;
		private EditTagsDataGridViewImageButtonColumn tagAndDetailsGVColumn;
	}
}
