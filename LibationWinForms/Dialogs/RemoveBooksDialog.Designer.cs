
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.gridEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnRemoveBooks = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.removeDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.coverDataGridViewImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.miscDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DatePurchased = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridEntryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.removeDataGridViewCheckBoxColumn,
            this.coverDataGridViewImageColumn,
            this.titleDataGridViewTextBoxColumn,
            this.authorsDataGridViewTextBoxColumn,
            this.miscDataGridViewTextBoxColumn,
            this.DatePurchased});
            this.dataGridView1.DataSource = this.gridEntryBindingSource;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 82;
            this.dataGridView1.Size = new System.Drawing.Size(800, 409);
            this.dataGridView1.TabIndex = 0;
            // 
            // gridEntryBindingSource
            // 
            this.gridEntryBindingSource.AllowNew = false;
            this.gridEntryBindingSource.DataSource = typeof(LibationWinForms.Dialogs.RemovableGridEntry);
            // 
            // btnRemoveBooks
            // 
            this.btnRemoveBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveBooks.Location = new System.Drawing.Point(570, 419);
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
            this.label1.Size = new System.Drawing.Size(169, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "{0} books selected for removal.";
            // 
            // removeDataGridViewCheckBoxColumn
            // 
            this.removeDataGridViewCheckBoxColumn.DataPropertyName = "Remove";
            this.removeDataGridViewCheckBoxColumn.FalseValue = "False";
            this.removeDataGridViewCheckBoxColumn.Frozen = true;
            this.removeDataGridViewCheckBoxColumn.HeaderText = "Remove";
            this.removeDataGridViewCheckBoxColumn.MinimumWidth = 60;
            this.removeDataGridViewCheckBoxColumn.Name = "removeDataGridViewCheckBoxColumn";
            this.removeDataGridViewCheckBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.removeDataGridViewCheckBoxColumn.TrueValue = "True";
            this.removeDataGridViewCheckBoxColumn.Width = 60;
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
            // DatePurchased
            // 
            this.DatePurchased.DataPropertyName = "DatePurchased";
            this.DatePurchased.HeaderText = "Date Purchased";
            this.DatePurchased.Name = "DatePurchased";
            this.DatePurchased.ReadOnly = true;
            this.DatePurchased.Width = 120;
            // 
            // RemoveBooksDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRemoveBooks);
            this.Controls.Add(this.dataGridView1);
            this.Name = "RemoveBooksDialog";
            this.Text = "RemoveBooksDialog";
            this.Shown += new System.EventHandler(this.RemoveBooksDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridEntryBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource gridEntryBindingSource;
        private System.Windows.Forms.Button btnRemoveBooks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn removeDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewImageColumn coverDataGridViewImageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn miscDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DatePurchased;
    }
}