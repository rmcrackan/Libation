namespace LibationWinForms.Dialogs
{
	partial class SettingsDialog
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
			this.booksLocationLbl = new System.Windows.Forms.Label();
			this.booksLocationTb = new System.Windows.Forms.TextBox();
			this.booksLocationSearchBtn = new System.Windows.Forms.Button();
			this.booksLocationDescLbl = new System.Windows.Forms.Label();
			this.downloadsInProgressGb = new System.Windows.Forms.GroupBox();
			this.downloadsInProgressLibationFilesRb = new System.Windows.Forms.RadioButton();
			this.downloadsInProgressWinTempRb = new System.Windows.Forms.RadioButton();
			this.downloadsInProgressDescLbl = new System.Windows.Forms.Label();
			this.decryptInProgressGb = new System.Windows.Forms.GroupBox();
			this.decryptInProgressLibationFilesRb = new System.Windows.Forms.RadioButton();
			this.decryptInProgressWinTempRb = new System.Windows.Forms.RadioButton();
			this.decryptInProgressDescLbl = new System.Windows.Forms.Label();
			this.saveBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.allowLibationFixupCbox = new System.Windows.Forms.CheckBox();
			this.downloadsInProgressGb.SuspendLayout();
			this.decryptInProgressGb.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// booksLocationLbl
			// 
			this.booksLocationLbl.AutoSize = true;
			this.booksLocationLbl.Location = new System.Drawing.Point(14, 20);
			this.booksLocationLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.booksLocationLbl.Name = "booksLocationLbl";
			this.booksLocationLbl.Size = new System.Drawing.Size(85, 15);
			this.booksLocationLbl.TabIndex = 0;
			this.booksLocationLbl.Text = "Books location";
			// 
			// booksLocationTb
			// 
			this.booksLocationTb.Location = new System.Drawing.Point(111, 16);
			this.booksLocationTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.booksLocationTb.Name = "booksLocationTb";
			this.booksLocationTb.Size = new System.Drawing.Size(760, 23);
			this.booksLocationTb.TabIndex = 1;
			// 
			// booksLocationSearchBtn
			// 
			this.booksLocationSearchBtn.Location = new System.Drawing.Point(878, 14);
			this.booksLocationSearchBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.booksLocationSearchBtn.Name = "booksLocationSearchBtn";
			this.booksLocationSearchBtn.Size = new System.Drawing.Size(41, 27);
			this.booksLocationSearchBtn.TabIndex = 2;
			this.booksLocationSearchBtn.Text = "...";
			this.booksLocationSearchBtn.UseVisualStyleBackColor = true;
			this.booksLocationSearchBtn.Click += new System.EventHandler(this.booksLocationSearchBtn_Click);
			// 
			// booksLocationDescLbl
			// 
			this.booksLocationDescLbl.AutoSize = true;
			this.booksLocationDescLbl.Location = new System.Drawing.Point(107, 43);
			this.booksLocationDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.booksLocationDescLbl.Name = "booksLocationDescLbl";
			this.booksLocationDescLbl.Size = new System.Drawing.Size(39, 15);
			this.booksLocationDescLbl.TabIndex = 3;
			this.booksLocationDescLbl.Text = "[desc]";
			// 
			// downloadsInProgressGb
			// 
			this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressLibationFilesRb);
			this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressWinTempRb);
			this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressDescLbl);
			this.downloadsInProgressGb.Location = new System.Drawing.Point(10, 49);
			this.downloadsInProgressGb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.downloadsInProgressGb.Name = "downloadsInProgressGb";
			this.downloadsInProgressGb.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.downloadsInProgressGb.Size = new System.Drawing.Size(884, 135);
			this.downloadsInProgressGb.TabIndex = 4;
			this.downloadsInProgressGb.TabStop = false;
			this.downloadsInProgressGb.Text = "Downloads in progress";
			// 
			// downloadsInProgressLibationFilesRb
			// 
			this.downloadsInProgressLibationFilesRb.AutoSize = true;
			this.downloadsInProgressLibationFilesRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.downloadsInProgressLibationFilesRb.Location = new System.Drawing.Point(10, 93);
			this.downloadsInProgressLibationFilesRb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.downloadsInProgressLibationFilesRb.Name = "downloadsInProgressLibationFilesRb";
			this.downloadsInProgressLibationFilesRb.Size = new System.Drawing.Size(215, 34);
			this.downloadsInProgressLibationFilesRb.TabIndex = 2;
			this.downloadsInProgressLibationFilesRb.TabStop = true;
			this.downloadsInProgressLibationFilesRb.Text = "[desc]\r\n[libationFiles\\DownloadsInProgress]";
			this.downloadsInProgressLibationFilesRb.UseVisualStyleBackColor = true;
			// 
			// downloadsInProgressWinTempRb
			// 
			this.downloadsInProgressWinTempRb.AutoSize = true;
			this.downloadsInProgressWinTempRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.downloadsInProgressWinTempRb.Location = new System.Drawing.Point(10, 52);
			this.downloadsInProgressWinTempRb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.downloadsInProgressWinTempRb.Name = "downloadsInProgressWinTempRb";
			this.downloadsInProgressWinTempRb.Size = new System.Drawing.Size(200, 34);
			this.downloadsInProgressWinTempRb.TabIndex = 1;
			this.downloadsInProgressWinTempRb.TabStop = true;
			this.downloadsInProgressWinTempRb.Text = "[desc]\r\n[winTemp\\DownloadsInProgress]";
			this.downloadsInProgressWinTempRb.UseVisualStyleBackColor = true;
			// 
			// downloadsInProgressDescLbl
			// 
			this.downloadsInProgressDescLbl.AutoSize = true;
			this.downloadsInProgressDescLbl.Location = new System.Drawing.Point(7, 18);
			this.downloadsInProgressDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.downloadsInProgressDescLbl.Name = "downloadsInProgressDescLbl";
			this.downloadsInProgressDescLbl.Size = new System.Drawing.Size(43, 30);
			this.downloadsInProgressDescLbl.TabIndex = 0;
			this.downloadsInProgressDescLbl.Text = "[desc]\r\n[line 2]";
			// 
			// decryptInProgressGb
			// 
			this.decryptInProgressGb.Controls.Add(this.decryptInProgressLibationFilesRb);
			this.decryptInProgressGb.Controls.Add(this.decryptInProgressWinTempRb);
			this.decryptInProgressGb.Controls.Add(this.decryptInProgressDescLbl);
			this.decryptInProgressGb.Location = new System.Drawing.Point(10, 193);
			this.decryptInProgressGb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.decryptInProgressGb.Name = "decryptInProgressGb";
			this.decryptInProgressGb.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.decryptInProgressGb.Size = new System.Drawing.Size(884, 135);
			this.decryptInProgressGb.TabIndex = 5;
			this.decryptInProgressGb.TabStop = false;
			this.decryptInProgressGb.Text = "Decrypt in progress";
			// 
			// decryptInProgressLibationFilesRb
			// 
			this.decryptInProgressLibationFilesRb.AutoSize = true;
			this.decryptInProgressLibationFilesRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.decryptInProgressLibationFilesRb.Location = new System.Drawing.Point(7, 93);
			this.decryptInProgressLibationFilesRb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.decryptInProgressLibationFilesRb.Name = "decryptInProgressLibationFilesRb";
			this.decryptInProgressLibationFilesRb.Size = new System.Drawing.Size(197, 34);
			this.decryptInProgressLibationFilesRb.TabIndex = 2;
			this.decryptInProgressLibationFilesRb.TabStop = true;
			this.decryptInProgressLibationFilesRb.Text = "[desc]\r\n[libationFiles\\DecryptInProgress]";
			this.decryptInProgressLibationFilesRb.UseVisualStyleBackColor = true;
			// 
			// decryptInProgressWinTempRb
			// 
			this.decryptInProgressWinTempRb.AutoSize = true;
			this.decryptInProgressWinTempRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.decryptInProgressWinTempRb.Location = new System.Drawing.Point(7, 52);
			this.decryptInProgressWinTempRb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.decryptInProgressWinTempRb.Name = "decryptInProgressWinTempRb";
			this.decryptInProgressWinTempRb.Size = new System.Drawing.Size(182, 34);
			this.decryptInProgressWinTempRb.TabIndex = 1;
			this.decryptInProgressWinTempRb.TabStop = true;
			this.decryptInProgressWinTempRb.Text = "[desc]\r\n[winTemp\\DecryptInProgress]";
			this.decryptInProgressWinTempRb.UseVisualStyleBackColor = true;
			// 
			// decryptInProgressDescLbl
			// 
			this.decryptInProgressDescLbl.AutoSize = true;
			this.decryptInProgressDescLbl.Location = new System.Drawing.Point(7, 18);
			this.decryptInProgressDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.decryptInProgressDescLbl.Name = "decryptInProgressDescLbl";
			this.decryptInProgressDescLbl.Size = new System.Drawing.Size(43, 30);
			this.decryptInProgressDescLbl.TabIndex = 0;
			this.decryptInProgressDescLbl.Text = "[desc]\r\n[line 2]";
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(714, 401);
			this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 7;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(832, 401);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 8;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.allowLibationFixupCbox);
			this.groupBox1.Controls.Add(this.downloadsInProgressGb);
			this.groupBox1.Controls.Add(this.decryptInProgressGb);
			this.groupBox1.Location = new System.Drawing.Point(18, 61);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.groupBox1.Size = new System.Drawing.Size(902, 334);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Advanced settings for control freaks";
			// 
			// allowLibationFixupCbox
			// 
			this.allowLibationFixupCbox.AutoSize = true;
			this.allowLibationFixupCbox.Location = new System.Drawing.Point(10, 24);
			this.allowLibationFixupCbox.Name = "allowLibationFixupCbox";
			this.allowLibationFixupCbox.Size = new System.Drawing.Size(262, 19);
			this.allowLibationFixupCbox.TabIndex = 6;
			this.allowLibationFixupCbox.Text = "Allow Libation to fix up audiobook metadata";
			this.allowLibationFixupCbox.UseVisualStyleBackColor = true;
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(933, 442);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.Controls.Add(this.booksLocationDescLbl);
			this.Controls.Add(this.booksLocationSearchBtn);
			this.Controls.Add(this.booksLocationTb);
			this.Controls.Add(this.booksLocationLbl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "SettingsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Settings";
			this.Load += new System.EventHandler(this.SettingsDialog_Load);
			this.downloadsInProgressGb.ResumeLayout(false);
			this.downloadsInProgressGb.PerformLayout();
			this.decryptInProgressGb.ResumeLayout(false);
			this.decryptInProgressGb.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label booksLocationLbl;
		private System.Windows.Forms.TextBox booksLocationTb;
		private System.Windows.Forms.Button booksLocationSearchBtn;
		private System.Windows.Forms.Label booksLocationDescLbl;
		private System.Windows.Forms.GroupBox downloadsInProgressGb;
		private System.Windows.Forms.Label downloadsInProgressDescLbl;
		private System.Windows.Forms.RadioButton downloadsInProgressWinTempRb;
		private System.Windows.Forms.RadioButton downloadsInProgressLibationFilesRb;
		private System.Windows.Forms.GroupBox decryptInProgressGb;
		private System.Windows.Forms.Label decryptInProgressDescLbl;
		private System.Windows.Forms.RadioButton decryptInProgressLibationFilesRb;
		private System.Windows.Forms.RadioButton decryptInProgressWinTempRb;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox allowLibationFixupCbox;
    }
}