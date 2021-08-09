namespace LibationWinForms
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
            this.gridEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridEntryDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewImageButtonBoxColumn1 = new LibationWinForms.LiberateDataGridViewImageButtonColumn();
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProductRating = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PurchaseDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MyRating = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewImageButtonBoxColumn2 = new LibationWinForms.EditTagsDataGridViewImageButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridEntryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridEntryDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // gridEntryBindingSource
            // 
            this.gridEntryBindingSource.DataSource = typeof(LibationWinForms.GridEntry);
            // 
            // gridEntryDataGridView
            // 
            this.gridEntryDataGridView.AllowUserToAddRows = false;
            this.gridEntryDataGridView.AllowUserToDeleteRows = false;
            this.gridEntryDataGridView.AutoGenerateColumns = false;
            this.gridEntryDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridEntryDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewImageButtonBoxColumn1,
            this.dataGridViewImageColumn1,
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.ProductRating,
            this.PurchaseDate,
            this.MyRating,
            this.dataGridViewTextBoxColumn11,
            this.dataGridViewImageButtonBoxColumn2});
            this.gridEntryDataGridView.DataSource = this.gridEntryBindingSource;
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
            this.gridEntryDataGridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gridEntryDataGridView.Name = "gridEntryDataGridView";
            this.gridEntryDataGridView.ReadOnly = true;
            this.gridEntryDataGridView.RowHeadersVisible = false;
            this.gridEntryDataGridView.RowTemplate.Height = 82;
            this.gridEntryDataGridView.Size = new System.Drawing.Size(1503, 380);
            this.gridEntryDataGridView.TabIndex = 0;
            // 
            // dataGridViewImageButtonBoxColumn1
            // 
            this.dataGridViewImageButtonBoxColumn1.DataPropertyName = "Liberate";
            this.dataGridViewImageButtonBoxColumn1.HeaderText = "Liberate";
            this.dataGridViewImageButtonBoxColumn1.Name = "dataGridViewImageButtonBoxColumn1";
            this.dataGridViewImageButtonBoxColumn1.ReadOnly = true;
            this.dataGridViewImageButtonBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageButtonBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewImageButtonBoxColumn1.Width = 70;
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.DataPropertyName = "Cover";
            this.dataGridViewImageColumn1.HeaderText = "Cover";
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.ReadOnly = true;
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageColumn1.ToolTipText = "Cover Art";
            this.dataGridViewImageColumn1.Width = 80;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Title";
            this.dataGridViewTextBoxColumn1.HeaderText = "Title";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.ToolTipText = "Book Title";
            this.dataGridViewTextBoxColumn1.Width = 200;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Authors";
            this.dataGridViewTextBoxColumn2.HeaderText = "Authors";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.ToolTipText = "Book Author(s)";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Narrators";
            this.dataGridViewTextBoxColumn3.HeaderText = "Narrators";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.ToolTipText = "Audiobook Narrator(s)";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "Length";
            this.dataGridViewTextBoxColumn4.HeaderText = "Length";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.ToolTipText = "Recording Length";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Series";
            this.dataGridViewTextBoxColumn5.HeaderText = "Series";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.ToolTipText = "Book Series";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "Description";
            this.dataGridViewTextBoxColumn6.HeaderText = "Description";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "Category";
            this.dataGridViewTextBoxColumn7.HeaderText = "Category";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.ReadOnly = true;
            // 
            // ProductRating
            // 
            this.ProductRating.DataPropertyName = "ProductRating";
            this.ProductRating.HeaderText = "Product Rating";
            this.ProductRating.Name = "ProductRating";
            this.ProductRating.ReadOnly = true;
            this.ProductRating.Width = 108;
            // 
            // PurchaseDate
            // 
            this.PurchaseDate.DataPropertyName = "PurchaseDate";
            this.PurchaseDate.HeaderText = "Purchase Date";
            this.PurchaseDate.Name = "PurchaseDate";
            this.PurchaseDate.ReadOnly = true;
            // 
            // MyRating
            // 
            this.MyRating.DataPropertyName = "MyRating";
            this.MyRating.HeaderText = "My Rating";
            this.MyRating.Name = "MyRating";
            this.MyRating.ReadOnly = true;
            this.MyRating.Width = 108;
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.DataPropertyName = "Misc";
            this.dataGridViewTextBoxColumn11.HeaderText = "Misc";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            this.dataGridViewTextBoxColumn11.ReadOnly = true;
            this.dataGridViewTextBoxColumn11.Width = 135;
            // 
            // dataGridViewImageButtonBoxColumn2
            // 
            this.dataGridViewImageButtonBoxColumn2.DataPropertyName = "DisplayTags";
            this.dataGridViewImageButtonBoxColumn2.HeaderText = "Edit Tags";
            this.dataGridViewImageButtonBoxColumn2.Name = "dataGridViewImageButtonBoxColumn2";
            this.dataGridViewImageButtonBoxColumn2.ReadOnly = true;
            this.dataGridViewImageButtonBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageButtonBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ProductsGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridEntryDataGridView);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ProductsGrid";
            this.Size = new System.Drawing.Size(1503, 380);
            ((System.ComponentModel.ISupportInitialize)(this.gridEntryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridEntryDataGridView)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.BindingSource gridEntryBindingSource;
		private System.Windows.Forms.DataGridView gridEntryDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private LiberateDataGridViewImageButtonColumn dataGridViewImageButtonBoxColumn1;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProductRating;
        private System.Windows.Forms.DataGridViewTextBoxColumn PurchaseDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn MyRating;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private EditTagsDataGridViewImageButtonColumn dataGridViewImageButtonBoxColumn2;
    }
}
