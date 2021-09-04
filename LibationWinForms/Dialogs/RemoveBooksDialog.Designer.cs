
namespace LibationWinForms.Dialogs
{
    partial class RemoveBooksDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this._dataGridView = new System.Windows.Forms.DataGridView();
			this.removeDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.coverDataGridViewImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.authorsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.miscDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.purchaseDateGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.gridEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.btnRemoveBooks = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this._dataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridEntryBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// _dataGridView
			// 
			this._dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._dataGridView.AutoGenerateColumns = false;
			this._dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this._dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.removeDataGridViewCheckBoxColumn,
            this.coverDataGridViewImageColumn,
            this.titleDataGridViewTextBoxColumn,
            this.authorsDataGridViewTextBoxColumn,
            this.miscDataGridViewTextBoxColumn,
            this.purchaseDateGridViewTextBoxColumn});
			this._dataGridView.DataSource = this.gridEntryBindingSource;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this._dataGridView.DefaultCellStyle = dataGridViewCellStyle1;
			this._dataGridView.Location = new System.Drawing.Point(0, 0);
			this._dataGridView.Name = "_dataGridView";
			this._dataGridView.RowHeadersVisible = false;
			this._dataGridView.RowTemplate.Height = 82;
			this._dataGridView.Size = new System.Drawing.Size(730, 409);
			this._dataGridView.TabIndex = 0;
			// 
			// removeDataGridViewCheckBoxColumn
			// 
			this.removeDataGridViewCheckBoxColumn.DataPropertyName = "Remove";
			this.removeDataGridViewCheckBoxColumn.FalseValue = "False";
			this.removeDataGridViewCheckBoxColumn.Frozen = true;
			this.removeDataGridViewCheckBoxColumn.HeaderText = "Remove";
			this.removeDataGridViewCheckBoxColumn.MinimumWidth = 80;
			this.removeDataGridViewCheckBoxColumn.Name = "removeDataGridViewCheckBoxColumn";
			this.removeDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.removeDataGridViewCheckBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.removeDataGridViewCheckBoxColumn.TrueValue = "True";
			this.removeDataGridViewCheckBoxColumn.Width = 80;
			// 
			// coverDataGridViewImageColumn
			// 
			this.coverDataGridViewImageColumn.DataPropertyName = "Cover";
			this.coverDataGridViewImageColumn.HeaderText = "Cover";
			this.coverDataGridViewImageColumn.MinimumWidth = 80;
			this.coverDataGridViewImageColumn.Name = "coverDataGridViewImageColumn";
			this.coverDataGridViewImageColumn.ReadOnly = true;
			this.coverDataGridViewImageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.coverDataGridViewImageColumn.Width = 80;
			// 
			// titleDataGridViewTextBoxColumn
			// 
			this.titleDataGridViewTextBoxColumn.DataPropertyName = "Title";
			this.titleDataGridViewTextBoxColumn.HeaderText = "Title";
			this.titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
			this.titleDataGridViewTextBoxColumn.ReadOnly = true;
			this.titleDataGridViewTextBoxColumn.Width = 200;
			// 
			// authorsDataGridViewTextBoxColumn
			// 
			this.authorsDataGridViewTextBoxColumn.DataPropertyName = "Authors";
			this.authorsDataGridViewTextBoxColumn.HeaderText = "Authors";
			this.authorsDataGridViewTextBoxColumn.Name = "authorsDataGridViewTextBoxColumn";
			this.authorsDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// miscDataGridViewTextBoxColumn
			// 
			this.miscDataGridViewTextBoxColumn.DataPropertyName = "Misc";
			this.miscDataGridViewTextBoxColumn.HeaderText = "Misc";
			this.miscDataGridViewTextBoxColumn.Name = "miscDataGridViewTextBoxColumn";
			this.miscDataGridViewTextBoxColumn.ReadOnly = true;
			this.miscDataGridViewTextBoxColumn.Width = 150;
			// 
			// purchaseDateGridViewTextBoxColumn
			// 
			this.purchaseDateGridViewTextBoxColumn.DataPropertyName = "PurchaseDate";
			this.purchaseDateGridViewTextBoxColumn.HeaderText = "Purchase Date";
			this.purchaseDateGridViewTextBoxColumn.Name = "purchaseDateGridViewTextBoxColumn";
			this.purchaseDateGridViewTextBoxColumn.ReadOnly = true;
			this.purchaseDateGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// gridEntryBindingSource
			// 
			this.gridEntryBindingSource.AllowNew = false;
			this.gridEntryBindingSource.DataSource = typeof(LibationWinForms.Dialogs.RemovableGridEntry);
			// 
			// btnRemoveBooks
			// 
			this.btnRemoveBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRemoveBooks.Location = new System.Drawing.Point(500, 419);
			this.btnRemoveBooks.Name = "btnRemoveBooks";
			this.btnRemoveBooks.Size = new System.Drawing.Size(218, 23);
			this.btnRemoveBooks.TabIndex = 1;
			this.btnRemoveBooks.Text = "Remove Selected Books from Libation";
			this.btnRemoveBooks.UseVisualStyleBackColor = true;
			this.btnRemoveBooks.Click += new System.EventHandler(this.btnRemoveBooks_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 423);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(178, 15);
			this.label1.TabIndex = 2;
			this.label1.Text = "{0} book{1} selected for removal.";
			// 
			// RemoveBooksDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(730, 450);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnRemoveBooks);
			this.Controls.Add(this._dataGridView);
			this.Name = "RemoveBooksDialog";
			this.Text = "RemoveBooksDialog";
			this.Shown += new System.EventHandler(this.RemoveBooksDialog_Shown);
			((System.ComponentModel.ISupportInitialize)(this._dataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridEntryBindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView _dataGridView;
        private System.Windows.Forms.BindingSource gridEntryBindingSource;
        private System.Windows.Forms.Button btnRemoveBooks;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DataGridViewCheckBoxColumn removeDataGridViewCheckBoxColumn;
		private System.Windows.Forms.DataGridViewImageColumn coverDataGridViewImageColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn authorsDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn miscDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn purchaseDateGridViewTextBoxColumn;
	}
}