using LibationWinForms.GridView;

namespace LibationWinForms.Dialogs;

partial class FindBetterQualityBooksDialog
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
		dataGridView1 = new System.Windows.Forms.DataGridView();
		asinDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
		titleDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
		foundFileDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
		codecDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
		bitrateStringDataGridViewTextBoxColumn = new BitrateDataGridTextBoxColumn();
		availableCodecDataGridViewTextBoxColumn = new AccessibleDataGridViewColumn();
		availableBitrateStringDataGridViewTextBoxColumn = new BitrateDataGridTextBoxColumn();
		isSignificantDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		bookDataViewModelBindingSource = new SyncBindingSource(components);
		btnScan = new System.Windows.Forms.Button();
		cboxUseWidevine = new System.Windows.Forms.CheckBox();
		lblScanCount = new System.Windows.Forms.Label();
		btnMarkBooks = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
		((System.ComponentModel.ISupportInitialize)bookDataViewModelBindingSource).BeginInit();
		SuspendLayout();
		// 
		// dataGridView1
		// 
		dataGridView1.AllowUserToAddRows = false;
		dataGridView1.AllowUserToDeleteRows = false;
		dataGridView1.AllowUserToResizeRows = false;
		dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		dataGridView1.AutoGenerateColumns = false;
		dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { asinDataGridViewTextBoxColumn, titleDataGridViewTextBoxColumn, foundFileDataGridViewTextBoxColumn, codecDataGridViewTextBoxColumn, bitrateStringDataGridViewTextBoxColumn, availableCodecDataGridViewTextBoxColumn, availableBitrateStringDataGridViewTextBoxColumn, isSignificantDataGridViewCheckBoxColumn });
		dataGridView1.DataSource = bookDataViewModelBindingSource;
		dataGridView1.Location = new System.Drawing.Point(12, 12);
		dataGridView1.Name = "dataGridView1";
		dataGridView1.RowHeadersVisible = false;
		dataGridView1.Size = new System.Drawing.Size(897, 397);
		dataGridView1.TabIndex = 0;
		dataGridView1.CellFormatting += dataGridView1_CellFormatting;
		// 
		// asinDataGridViewTextBoxColumn
		// 
		asinDataGridViewTextBoxColumn.AccessibilityDescription = "Audible product identifier.";
		asinDataGridViewTextBoxColumn.AccessibilityName = "ASIN";
		asinDataGridViewTextBoxColumn.DataPropertyName = "Asin";
		asinDataGridViewTextBoxColumn.HeaderText = "Asin";
		asinDataGridViewTextBoxColumn.Name = "asinDataGridViewTextBoxColumn";
		asinDataGridViewTextBoxColumn.ReadOnly = true;
		asinDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		asinDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		asinDataGridViewTextBoxColumn.Width = 80;
		// 
		// titleDataGridViewTextBoxColumn
		// 
		titleDataGridViewTextBoxColumn.AccessibilityDescription = "Title of the Audiobook to scan for.";
		titleDataGridViewTextBoxColumn.AccessibilityName = "Book Title";
		titleDataGridViewTextBoxColumn.DataPropertyName = "Title";
		titleDataGridViewTextBoxColumn.HeaderText = "Title";
		titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
		titleDataGridViewTextBoxColumn.ReadOnly = true;
		titleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		titleDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		titleDataGridViewTextBoxColumn.Width = 180;
		// 
		// foundFileDataGridViewTextBoxColumn
		// 
		foundFileDataGridViewTextBoxColumn.AccessibilityDescription = "Highest quality audio file that has been found in your 'Books' folder.";
		foundFileDataGridViewTextBoxColumn.AccessibilityName = "Best Found File";
		foundFileDataGridViewTextBoxColumn.DataPropertyName = "FoundFile";
		foundFileDataGridViewTextBoxColumn.HeaderText = "Best Found File";
		foundFileDataGridViewTextBoxColumn.Name = "foundFileDataGridViewTextBoxColumn";
		foundFileDataGridViewTextBoxColumn.ReadOnly = true;
		foundFileDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		foundFileDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		foundFileDataGridViewTextBoxColumn.Width = 180;
		// 
		// codecDataGridViewTextBoxColumn
		// 
		codecDataGridViewTextBoxColumn.AccessibilityDescription = "The audio format codec of the Best Found File";
		codecDataGridViewTextBoxColumn.AccessibilityName = "Existing Codec";
		codecDataGridViewTextBoxColumn.DataPropertyName = "Codec";
		codecDataGridViewTextBoxColumn.HeaderText = "Existing Codec";
		codecDataGridViewTextBoxColumn.Name = "codecDataGridViewTextBoxColumn";
		codecDataGridViewTextBoxColumn.ReadOnly = true;
		codecDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		codecDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		codecDataGridViewTextBoxColumn.Width = 80;
		// 
		// bitrateStringDataGridViewTextBoxColumn
		// 
		bitrateStringDataGridViewTextBoxColumn.AccessibilityDescription = "The audio bitrate of the Best Found File";
		bitrateStringDataGridViewTextBoxColumn.AccessibilityName = "Existing Bitrate";
		bitrateStringDataGridViewTextBoxColumn.DataPropertyName = "Bitrate";
		bitrateStringDataGridViewTextBoxColumn.HeaderText = "Existing Bitrate";
		bitrateStringDataGridViewTextBoxColumn.Name = "bitrateStringDataGridViewTextBoxColumn";
		bitrateStringDataGridViewTextBoxColumn.ReadOnly = true;
		bitrateStringDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		bitrateStringDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		bitrateStringDataGridViewTextBoxColumn.Width = 80;
		// 
		// availableCodecDataGridViewTextBoxColumn
		// 
		availableCodecDataGridViewTextBoxColumn.AccessibilityDescription = "The audio format codec available from Audible.";
		availableCodecDataGridViewTextBoxColumn.AccessibilityName = "Available Codec";
		availableCodecDataGridViewTextBoxColumn.DataPropertyName = "AvailableCodec";
		availableCodecDataGridViewTextBoxColumn.HeaderText = "Available Codec";
		availableCodecDataGridViewTextBoxColumn.Name = "availableCodecDataGridViewTextBoxColumn";
		availableCodecDataGridViewTextBoxColumn.ReadOnly = true;
		availableCodecDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		availableCodecDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		availableCodecDataGridViewTextBoxColumn.Width = 80;
		// 
		// availableBitrateStringDataGridViewTextBoxColumn
		// 
		availableBitrateStringDataGridViewTextBoxColumn.AccessibilityDescription = "The highest audio bitrate available from Audible.";
		availableBitrateStringDataGridViewTextBoxColumn.AccessibilityName = "Available Bitrate";
		availableBitrateStringDataGridViewTextBoxColumn.DataPropertyName = "AvailableBitrate";
		availableBitrateStringDataGridViewTextBoxColumn.HeaderText = "Available Bitrate";
		availableBitrateStringDataGridViewTextBoxColumn.Name = "availableBitrateStringDataGridViewTextBoxColumn";
		availableBitrateStringDataGridViewTextBoxColumn.ReadOnly = true;
		availableBitrateStringDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		availableBitrateStringDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		availableBitrateStringDataGridViewTextBoxColumn.Width = 80;
		// 
		// isSignificantDataGridViewCheckBoxColumn
		// 
		isSignificantDataGridViewCheckBoxColumn.DataPropertyName = "IsSignificant";
		isSignificantDataGridViewCheckBoxColumn.HeaderText = "Significantly Greater?";
		isSignificantDataGridViewCheckBoxColumn.Name = "isSignificantDataGridViewCheckBoxColumn";
		isSignificantDataGridViewCheckBoxColumn.ReadOnly = true;
		isSignificantDataGridViewCheckBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		// 
		// bookDataViewModelBindingSource
		// 
		bookDataViewModelBindingSource.DataSource = typeof(LibationUiBase.BookDataViewModel);
		// 
		// btnScan
		// 
		btnScan.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		btnScan.Location = new System.Drawing.Point(120, 415);
		btnScan.Name = "btnScan";
		btnScan.Size = new System.Drawing.Size(221, 23);
		btnScan.TabIndex = 1;
		btnScan.Text = "Scan Audible for Higher Quality Audio";
		btnScan.UseVisualStyleBackColor = true;
		btnScan.Click += btnScan_Click;
		// 
		// cboxUseWidevine
		// 
		cboxUseWidevine.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		cboxUseWidevine.AutoSize = true;
		cboxUseWidevine.Location = new System.Drawing.Point(12, 418);
		cboxUseWidevine.Name = "cboxUseWidevine";
		cboxUseWidevine.Size = new System.Drawing.Size(102, 19);
		cboxUseWidevine.TabIndex = 2;
		cboxUseWidevine.Text = "Use Widevine?";
		cboxUseWidevine.UseVisualStyleBackColor = true;
		// 
		// lblScanCount
		// 
		lblScanCount.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		lblScanCount.AutoSize = true;
		lblScanCount.Location = new System.Drawing.Point(369, 419);
		lblScanCount.Name = "lblScanCount";
		lblScanCount.Size = new System.Drawing.Size(52, 15);
		lblScanCount.TabIndex = 3;
		lblScanCount.Text = "## of ##";
		// 
		// btnMarkBooks
		// 
		btnMarkBooks.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		btnMarkBooks.Location = new System.Drawing.Point(699, 415);
		btnMarkBooks.Name = "btnMarkBooks";
		btnMarkBooks.Size = new System.Drawing.Size(210, 23);
		btnMarkBooks.TabIndex = 4;
		btnMarkBooks.Text = "Mark 1,000 books as 'Not Liberated'";
		btnMarkBooks.UseVisualStyleBackColor = true;
		btnMarkBooks.Click += btnMarkBooks_Click;
		// 
		// FindBetterQualityBooksDialog
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		ClientSize = new System.Drawing.Size(921, 450);
		Controls.Add(btnMarkBooks);
		Controls.Add(lblScanCount);
		Controls.Add(cboxUseWidevine);
		Controls.Add(btnScan);
		Controls.Add(dataGridView1);
		Name = "FindBetterQualityBooksDialog";
		Text = "Scan Audible for Better Quality Audiobooks";
		((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
		((System.ComponentModel.ISupportInitialize)bookDataViewModelBindingSource).EndInit();
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private System.Windows.Forms.DataGridView dataGridView1;
	private LibationWinForms.GridView.SyncBindingSource bookDataViewModelBindingSource;
	private System.Windows.Forms.Button btnScan;
	private AccessibleDataGridViewColumn asinDataGridViewTextBoxColumn;
	private AccessibleDataGridViewColumn titleDataGridViewTextBoxColumn;
	private AccessibleDataGridViewColumn foundFileDataGridViewTextBoxColumn;
	private AccessibleDataGridViewColumn codecDataGridViewTextBoxColumn;
	private BitrateDataGridTextBoxColumn bitrateStringDataGridViewTextBoxColumn;
	private AccessibleDataGridViewColumn availableCodecDataGridViewTextBoxColumn;
	private BitrateDataGridTextBoxColumn availableBitrateStringDataGridViewTextBoxColumn;
	private System.Windows.Forms.DataGridViewCheckBoxColumn isSignificantDataGridViewCheckBoxColumn;
	private System.Windows.Forms.CheckBox cboxUseWidevine;
	private System.Windows.Forms.Label lblScanCount;
	private System.Windows.Forms.Button btnMarkBooks;
}