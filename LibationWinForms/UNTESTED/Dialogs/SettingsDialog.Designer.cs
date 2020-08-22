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
			this.downloadsInProgressGb.SuspendLayout();
			this.decryptInProgressGb.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// booksLocationLbl
			// 
			this.booksLocationLbl.AutoSize = true;
			this.booksLocationLbl.Location = new System.Drawing.Point(12, 17);
			this.booksLocationLbl.Name = "booksLocationLbl";
			this.booksLocationLbl.Size = new System.Drawing.Size(77, 13);
			this.booksLocationLbl.TabIndex = 0;
			this.booksLocationLbl.Text = "Books location";
			// 
			// booksLocationTb
			// 
			this.booksLocationTb.Location = new System.Drawing.Point(95, 14);
			this.booksLocationTb.Name = "booksLocationTb";
			this.booksLocationTb.Size = new System.Drawing.Size(652, 20);
			this.booksLocationTb.TabIndex = 1;
			// 
			// booksLocationSearchBtn
			// 
			this.booksLocationSearchBtn.Location = new System.Drawing.Point(753, 12);
			this.booksLocationSearchBtn.Name = "booksLocationSearchBtn";
			this.booksLocationSearchBtn.Size = new System.Drawing.Size(35, 23);
			this.booksLocationSearchBtn.TabIndex = 2;
			this.booksLocationSearchBtn.Text = "...";
			this.booksLocationSearchBtn.UseVisualStyleBackColor = true;
			this.booksLocationSearchBtn.Click += new System.EventHandler(this.booksLocationSearchBtn_Click);
			// 
			// booksLocationDescLbl
			// 
			this.booksLocationDescLbl.AutoSize = true;
			this.booksLocationDescLbl.Location = new System.Drawing.Point(92, 37);
			this.booksLocationDescLbl.Name = "booksLocationDescLbl";
			this.booksLocationDescLbl.Size = new System.Drawing.Size(36, 13);
			this.booksLocationDescLbl.TabIndex = 3;
			this.booksLocationDescLbl.Text = "[desc]";
			// 
			// downloadsInProgressGb
			// 
			this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressLibationFilesRb);
			this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressWinTempRb);
			this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressDescLbl);
			this.downloadsInProgressGb.Location = new System.Drawing.Point(15, 58);
			this.downloadsInProgressGb.Name = "downloadsInProgressGb";
			this.downloadsInProgressGb.Size = new System.Drawing.Size(758, 117);
			this.downloadsInProgressGb.TabIndex = 4;
			this.downloadsInProgressGb.TabStop = false;
			this.downloadsInProgressGb.Text = "Downloads in progress";
			// 
			// downloadsInProgressLibationFilesRb
			// 
			this.downloadsInProgressLibationFilesRb.AutoSize = true;
			this.downloadsInProgressLibationFilesRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.downloadsInProgressLibationFilesRb.Location = new System.Drawing.Point(9, 81);
			this.downloadsInProgressLibationFilesRb.Name = "downloadsInProgressLibationFilesRb";
			this.downloadsInProgressLibationFilesRb.Size = new System.Drawing.Size(193, 30);
			this.downloadsInProgressLibationFilesRb.TabIndex = 2;
			this.downloadsInProgressLibationFilesRb.TabStop = true;
			this.downloadsInProgressLibationFilesRb.Text = "[desc]\r\n[libationFiles\\DownloadsInProgress]";
			this.downloadsInProgressLibationFilesRb.UseVisualStyleBackColor = true;
			// 
			// downloadsInProgressWinTempRb
			// 
			this.downloadsInProgressWinTempRb.AutoSize = true;
			this.downloadsInProgressWinTempRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.downloadsInProgressWinTempRb.Location = new System.Drawing.Point(9, 45);
			this.downloadsInProgressWinTempRb.Name = "downloadsInProgressWinTempRb";
			this.downloadsInProgressWinTempRb.Size = new System.Drawing.Size(182, 30);
			this.downloadsInProgressWinTempRb.TabIndex = 1;
			this.downloadsInProgressWinTempRb.TabStop = true;
			this.downloadsInProgressWinTempRb.Text = "[desc]\r\n[winTemp\\DownloadsInProgress]";
			this.downloadsInProgressWinTempRb.UseVisualStyleBackColor = true;
			// 
			// downloadsInProgressDescLbl
			// 
			this.downloadsInProgressDescLbl.AutoSize = true;
			this.downloadsInProgressDescLbl.Location = new System.Drawing.Point(6, 16);
			this.downloadsInProgressDescLbl.Name = "downloadsInProgressDescLbl";
			this.downloadsInProgressDescLbl.Size = new System.Drawing.Size(38, 26);
			this.downloadsInProgressDescLbl.TabIndex = 0;
			this.downloadsInProgressDescLbl.Text = "[desc]\r\n[line 2]";
			// 
			// decryptInProgressGb
			// 
			this.decryptInProgressGb.Controls.Add(this.decryptInProgressLibationFilesRb);
			this.decryptInProgressGb.Controls.Add(this.decryptInProgressWinTempRb);
			this.decryptInProgressGb.Controls.Add(this.decryptInProgressDescLbl);
			this.decryptInProgressGb.Location = new System.Drawing.Point(9, 183);
			this.decryptInProgressGb.Name = "decryptInProgressGb";
			this.decryptInProgressGb.Size = new System.Drawing.Size(758, 117);
			this.decryptInProgressGb.TabIndex = 5;
			this.decryptInProgressGb.TabStop = false;
			this.decryptInProgressGb.Text = "Decrypt in progress";
			// 
			// decryptInProgressLibationFilesRb
			// 
			this.decryptInProgressLibationFilesRb.AutoSize = true;
			this.decryptInProgressLibationFilesRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.decryptInProgressLibationFilesRb.Location = new System.Drawing.Point(6, 81);
			this.decryptInProgressLibationFilesRb.Name = "decryptInProgressLibationFilesRb";
			this.decryptInProgressLibationFilesRb.Size = new System.Drawing.Size(177, 30);
			this.decryptInProgressLibationFilesRb.TabIndex = 2;
			this.decryptInProgressLibationFilesRb.TabStop = true;
			this.decryptInProgressLibationFilesRb.Text = "[desc]\r\n[libationFiles\\DecryptInProgress]";
			this.decryptInProgressLibationFilesRb.UseVisualStyleBackColor = true;
			// 
			// decryptInProgressWinTempRb
			// 
			this.decryptInProgressWinTempRb.AutoSize = true;
			this.decryptInProgressWinTempRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.decryptInProgressWinTempRb.Location = new System.Drawing.Point(6, 45);
			this.decryptInProgressWinTempRb.Name = "decryptInProgressWinTempRb";
			this.decryptInProgressWinTempRb.Size = new System.Drawing.Size(166, 30);
			this.decryptInProgressWinTempRb.TabIndex = 1;
			this.decryptInProgressWinTempRb.TabStop = true;
			this.decryptInProgressWinTempRb.Text = "[desc]\r\n[winTemp\\DecryptInProgress]";
			this.decryptInProgressWinTempRb.UseVisualStyleBackColor = true;
			// 
			// decryptInProgressDescLbl
			// 
			this.decryptInProgressDescLbl.AutoSize = true;
			this.decryptInProgressDescLbl.Location = new System.Drawing.Point(6, 16);
			this.decryptInProgressDescLbl.Name = "decryptInProgressDescLbl";
			this.decryptInProgressDescLbl.Size = new System.Drawing.Size(38, 26);
			this.decryptInProgressDescLbl.TabIndex = 0;
			this.decryptInProgressDescLbl.Text = "[desc]\r\n[line 2]";
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(612, 367);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(75, 23);
			this.saveBtn.TabIndex = 7;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(713, 367);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 8;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.downloadsInProgressGb);
			this.groupBox1.Controls.Add(this.decryptInProgressGb);
			this.groupBox1.Location = new System.Drawing.Point(15, 53);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(773, 308);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Advanced settings for control freaks";
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(800, 402);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.Controls.Add(this.booksLocationDescLbl);
			this.Controls.Add(this.booksLocationSearchBtn);
			this.Controls.Add(this.booksLocationTb);
			this.Controls.Add(this.booksLocationLbl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
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
	}
}