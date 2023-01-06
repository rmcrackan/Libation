namespace LibationWinForms.Dialogs
{
	partial class BookRecordsDialog
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
			this.syncBindingSource = new LibationWinForms.GridView.SyncBindingSource(this.components);
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.checkboxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.createdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.startTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.modifiedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.endTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.noteColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.titleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.checkAllBtn = new System.Windows.Forms.Button();
			this.uncheckAllBtn = new System.Windows.Forms.Button();
			this.deleteCheckedBtn = new System.Windows.Forms.Button();
			this.exportAllBtn = new System.Windows.Forms.Button();
			this.exportCheckedBtn = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.syncBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.checkboxColumn,
            this.typeColumn,
            this.createdColumn,
            this.startTimeColumn,
            this.modifiedColumn,
            this.endTimeColumn,
            this.noteColumn,
            this.titleColumn});
			this.dataGridView1.Location = new System.Drawing.Point(0, 0);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.RowTemplate.Height = 25;
			this.dataGridView1.Size = new System.Drawing.Size(334, 291);
			this.dataGridView1.TabIndex = 0;
			// 
			// checkboxColumn
			// 
			this.checkboxColumn.DataPropertyName = "IsChecked";
			this.checkboxColumn.HeaderText = "Checked";
			this.checkboxColumn.Name = "checkboxColumn";
			this.checkboxColumn.Width = 60;
			// 
			// typeColumn
			// 
			this.typeColumn.DataPropertyName = "Type";
			this.typeColumn.HeaderText = "Type";
			this.typeColumn.Name = "typeColumn";
			this.typeColumn.ReadOnly = true;
			this.typeColumn.Width = 80;
			// 
			// createdColumn
			// 
			this.createdColumn.DataPropertyName = "Created";
			this.createdColumn.HeaderText = "Created";
			this.createdColumn.Name = "createdColumn";
			this.createdColumn.ReadOnly = true;
			// 
			// startTimeColumn
			// 
			this.startTimeColumn.DataPropertyName = "Start";
			this.startTimeColumn.HeaderText = "Start";
			this.startTimeColumn.Name = "startTimeColumn";
			this.startTimeColumn.ReadOnly = true;
			// 
			// modifiedColumn
			// 
			this.modifiedColumn.DataPropertyName = "Modified";
			this.modifiedColumn.HeaderText = "Modified";
			this.modifiedColumn.Name = "modifiedColumn";
			this.modifiedColumn.ReadOnly = true;
			// 
			// endTimeColumn
			// 
			this.endTimeColumn.DataPropertyName = "End";
			this.endTimeColumn.HeaderText = "End";
			this.endTimeColumn.Name = "endTimeColumn";
			this.endTimeColumn.ReadOnly = true;
			// 
			// noteColumn
			// 
			this.noteColumn.DataPropertyName = "Note";
			this.noteColumn.HeaderText = "Note";
			this.noteColumn.Name = "noteColumn";
			this.noteColumn.ReadOnly = true;
			// 
			// titleColumn
			// 
			this.titleColumn.DataPropertyName = "Title";
			this.titleColumn.HeaderText = "Title";
			this.titleColumn.Name = "titleColumn";
			this.titleColumn.ReadOnly = true;
			// 
			// checkAllBtn
			// 
			this.checkAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkAllBtn.Location = new System.Drawing.Point(12, 297);
			this.checkAllBtn.Name = "checkAllBtn";
			this.checkAllBtn.Size = new System.Drawing.Size(80, 23);
			this.checkAllBtn.TabIndex = 1;
			this.checkAllBtn.Text = "Check All";
			this.checkAllBtn.UseVisualStyleBackColor = true;
			this.checkAllBtn.Click += new System.EventHandler(this.checkAllBtn_Click);
			// 
			// uncheckAllBtn
			// 
			this.uncheckAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.uncheckAllBtn.Location = new System.Drawing.Point(12, 326);
			this.uncheckAllBtn.Name = "uncheckAllBtn";
			this.uncheckAllBtn.Size = new System.Drawing.Size(80, 23);
			this.uncheckAllBtn.TabIndex = 2;
			this.uncheckAllBtn.Text = "Uncheck All";
			this.uncheckAllBtn.UseVisualStyleBackColor = true;
			this.uncheckAllBtn.Click += new System.EventHandler(this.uncheckAllBtn_Click);
			// 
			// deleteCheckedBtn
			// 
			this.deleteCheckedBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteCheckedBtn.Location = new System.Drawing.Point(115, 297);
			this.deleteCheckedBtn.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.deleteCheckedBtn.Name = "deleteCheckedBtn";
			this.deleteCheckedBtn.Size = new System.Drawing.Size(61, 52);
			this.deleteCheckedBtn.TabIndex = 3;
			this.deleteCheckedBtn.Text = "Delete Checked";
			this.deleteCheckedBtn.UseVisualStyleBackColor = true;
			this.deleteCheckedBtn.Click += new System.EventHandler(this.deleteCheckedBtn_Click);
			// 
			// exportAllBtn
			// 
			this.exportAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exportAllBtn.Location = new System.Drawing.Point(221, 326);
			this.exportAllBtn.Name = "exportAllBtn";
			this.exportAllBtn.Size = new System.Drawing.Size(101, 23);
			this.exportAllBtn.TabIndex = 4;
			this.exportAllBtn.Text = "Export All";
			this.exportAllBtn.UseVisualStyleBackColor = true;
			this.exportAllBtn.Click += new System.EventHandler(this.exportAllBtn_Click);
			// 
			// exportCheckedBtn
			// 
			this.exportCheckedBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exportCheckedBtn.Location = new System.Drawing.Point(221, 297);
			this.exportCheckedBtn.Name = "exportCheckedBtn";
			this.exportCheckedBtn.Size = new System.Drawing.Size(101, 23);
			this.exportCheckedBtn.TabIndex = 5;
			this.exportCheckedBtn.Text = "Export Checked";
			this.exportCheckedBtn.UseVisualStyleBackColor = true;
			this.exportCheckedBtn.Click += new System.EventHandler(this.exportCheckedBtn_Click);
			// 
			// BookRecordsDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(334, 361);
			this.Controls.Add(this.exportCheckedBtn);
			this.Controls.Add(this.exportAllBtn);
			this.Controls.Add(this.deleteCheckedBtn);
			this.Controls.Add(this.uncheckAllBtn);
			this.Controls.Add(this.checkAllBtn);
			this.Controls.Add(this.dataGridView1);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(350, 400);
			this.Name = "BookRecordsDialog";
			this.Text = "Book Dialog";
			this.Shown += new System.EventHandler(this.BookRecordsDialog_Shown);
			((System.ComponentModel.ISupportInitialize)(this.syncBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private LibationWinForms.GridView.SyncBindingSource syncBindingSource;
		private System.Windows.Forms.Button checkAllBtn;
		private System.Windows.Forms.Button uncheckAllBtn;
		private System.Windows.Forms.Button deleteCheckedBtn;
		private System.Windows.Forms.Button exportAllBtn;
		private System.Windows.Forms.Button exportCheckedBtn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn checkboxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn typeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn createdColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn startTimeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn modifiedColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn endTimeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn noteColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn titleColumn;
	}
}