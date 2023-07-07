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
			this.label1 = new System.Windows.Forms.Label();
			this.foundAudiobooksLV = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.booksFoundLbl = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(108, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Found Audiobooks";
			// 
			// foundAudiobooksLV
			// 
			this.foundAudiobooksLV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.foundAudiobooksLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.foundAudiobooksLV.FullRowSelect = true;
			this.foundAudiobooksLV.Location = new System.Drawing.Point(12, 33);
			this.foundAudiobooksLV.Name = "foundAudiobooksLV";
			this.foundAudiobooksLV.Size = new System.Drawing.Size(321, 261);
			this.foundAudiobooksLV.TabIndex = 2;
			this.foundAudiobooksLV.UseCompatibleStateImageBehavior = false;
			this.foundAudiobooksLV.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Book ID";
			this.columnHeader1.Width = 85;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Title";
			// 
			// booksFoundLbl
			// 
			this.booksFoundLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.booksFoundLbl.AutoSize = true;
			this.booksFoundLbl.Location = new System.Drawing.Point(253, 9);
			this.booksFoundLbl.Name = "booksFoundLbl";
			this.booksFoundLbl.Size = new System.Drawing.Size(80, 15);
			this.booksFoundLbl.TabIndex = 3;
			this.booksFoundLbl.Text = "IDs Found: {0}";
			this.booksFoundLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// LocateAudiobooksDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(345, 306);
			this.Controls.Add(this.booksFoundLbl);
			this.Controls.Add(this.foundAudiobooksLV);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "LocateAudiobooksDialog";
			this.Text = "Locate Audiobooks";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView foundAudiobooksLV;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Label booksFoundLbl;
	}
}