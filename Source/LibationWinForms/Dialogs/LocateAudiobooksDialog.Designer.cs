namespace LibationWinForms.Dialogs
{
	partial class LocateAudiobooksDialog
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
			components = new System.ComponentModel.Container();
			label1 = new System.Windows.Forms.Label();
			booksFoundLbl = new System.Windows.Forms.Label();
			dataGridView1 = new System.Windows.Forms.DataGridView();
			foundAudiobookBindingSource = new System.Windows.Forms.BindingSource(components);
			iDDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
			fileNameDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			((System.ComponentModel.ISupportInitialize)foundAudiobookBindingSource).BeginInit();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(108, 15);
			label1.TabIndex = 1;
			label1.Text = "Found Audiobooks";
			// 
			// booksFoundLbl
			// 
			booksFoundLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			booksFoundLbl.AutoSize = true;
			booksFoundLbl.Location = new System.Drawing.Point(253, 9);
			booksFoundLbl.Name = "booksFoundLbl";
			booksFoundLbl.Size = new System.Drawing.Size(72, 15);
			booksFoundLbl.TabIndex = 3;
			booksFoundLbl.Text = "IDs Found: 0";
			booksFoundLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.AllowUserToResizeRows = false;
			dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { iDDataGridViewTextBoxColumn, fileNameDataGridViewTextBoxColumn });
			dataGridView1.DataSource = foundAudiobookBindingSource;
			dataGridView1.Location = new System.Drawing.Point(12, 27);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersVisible = false;
			dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			dataGridView1.Size = new System.Drawing.Size(321, 267);
			dataGridView1.TabIndex = 4;
			dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
			// 
			// foundAudiobookBindingSource
			// 
			foundAudiobookBindingSource.DataSource = typeof(LibationUiBase.FoundAudiobook);
			// 
			// iDDataGridViewTextBoxColumn
			// 
			iDDataGridViewTextBoxColumn.AccessibilityDescription = "Audiobook's Audible product ID forund by the scan.";
			iDDataGridViewTextBoxColumn.AccessibilityName = "Found ASIN";
			iDDataGridViewTextBoxColumn.DataPropertyName = "ID";
			iDDataGridViewTextBoxColumn.HeaderText = "Found ID";
			iDDataGridViewTextBoxColumn.Name = "iDDataGridViewTextBoxColumn";
			iDDataGridViewTextBoxColumn.ReadOnly = true;
			iDDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			iDDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			iDDataGridViewTextBoxColumn.Width = 80;
			// 
			// fileNameDataGridViewTextBoxColumn
			// 
			fileNameDataGridViewTextBoxColumn.AccessibilityDescription = "Audiobook file found. Double-click to open containing folder.";
			fileNameDataGridViewTextBoxColumn.AccessibilityName = "Found File";
			fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
			fileNameDataGridViewTextBoxColumn.HeaderText = "Found File";
			fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
			fileNameDataGridViewTextBoxColumn.ReadOnly = true;
			fileNameDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			fileNameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			fileNameDataGridViewTextBoxColumn.Width = 87;
			// 
			// LocateAudiobooksDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(345, 306);
			Controls.Add(dataGridView1);
			Controls.Add(booksFoundLbl);
			Controls.Add(label1);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			Name = "LocateAudiobooksDialog";
			Text = "Locate Audiobooks";
			FormClosing += LocateAudiobooks_FormClosing;
			Shown += LocateAudiobooks_Shown;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			((System.ComponentModel.ISupportInitialize)foundAudiobookBindingSource).EndInit();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label booksFoundLbl;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.BindingSource foundAudiobookBindingSource;
		private AccessibleDataGridViewColumn iDDataGridViewTextBoxColumn;
		private AccessibleDataGridViewColumn fileNameDataGridViewTextBoxColumn;
	}
}