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
			this.booksLocationDescLbl = new System.Windows.Forms.Label();
			this.inProgressDescLbl = new System.Windows.Forms.Label();
			this.saveBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.advancedSettingsGb = new System.Windows.Forms.GroupBox();
			this.convertLossyRb = new System.Windows.Forms.RadioButton();
			this.convertLosslessRb = new System.Windows.Forms.RadioButton();
			this.inProgressSelectControl = new LibationWinForms.Dialogs.DirectorySelectControl();
			this.allowLibationFixupCbox = new System.Windows.Forms.CheckBox();
			this.booksSelectControl = new LibationWinForms.Dialogs.DirectoryOrCustomSelectControl();
			this.booksGb = new System.Windows.Forms.GroupBox();
			this.logsBtn = new System.Windows.Forms.Button();
			this.advancedSettingsGb.SuspendLayout();
			this.booksGb.SuspendLayout();
			this.SuspendLayout();
			// 
			// booksLocationDescLbl
			// 
			this.booksLocationDescLbl.AutoSize = true;
			this.booksLocationDescLbl.Location = new System.Drawing.Point(7, 19);
			this.booksLocationDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.booksLocationDescLbl.Name = "booksLocationDescLbl";
			this.booksLocationDescLbl.Size = new System.Drawing.Size(69, 15);
			this.booksLocationDescLbl.TabIndex = 2;
			this.booksLocationDescLbl.Text = "[book desc]";
			// 
			// inProgressDescLbl
			// 
			this.inProgressDescLbl.AutoSize = true;
			this.inProgressDescLbl.Location = new System.Drawing.Point(8, 127);
			this.inProgressDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.inProgressDescLbl.Name = "inProgressDescLbl";
			this.inProgressDescLbl.Size = new System.Drawing.Size(43, 45);
			this.inProgressDescLbl.TabIndex = 1;
			this.inProgressDescLbl.Text = "[desc]\r\n[line 2]\r\n[line 3]";
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(714, 380);
			this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 4;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(832, 380);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 5;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// advancedSettingsGb
			// 
			this.advancedSettingsGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedSettingsGb.Controls.Add(this.logsBtn);
			this.advancedSettingsGb.Controls.Add(this.convertLossyRb);
			this.advancedSettingsGb.Controls.Add(this.convertLosslessRb);
			this.advancedSettingsGb.Controls.Add(this.inProgressSelectControl);
			this.advancedSettingsGb.Controls.Add(this.allowLibationFixupCbox);
			this.advancedSettingsGb.Controls.Add(this.inProgressDescLbl);
			this.advancedSettingsGb.Location = new System.Drawing.Point(12, 141);
			this.advancedSettingsGb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.advancedSettingsGb.Name = "advancedSettingsGb";
			this.advancedSettingsGb.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.advancedSettingsGb.Size = new System.Drawing.Size(908, 226);
			this.advancedSettingsGb.TabIndex = 3;
			this.advancedSettingsGb.TabStop = false;
			this.advancedSettingsGb.Text = "Advanced settings for control freaks";
			// 
			// convertLossyRb
			// 
			this.convertLossyRb.AutoSize = true;
			this.convertLossyRb.Location = new System.Drawing.Point(7, 88);
			this.convertLossyRb.Name = "convertLossyRb";
			this.convertLossyRb.Size = new System.Drawing.Size(242, 19);
			this.convertLossyRb.TabIndex = 0;
			this.convertLossyRb.Text = "Download my books as .MP3 files (Lossy)";
			this.convertLossyRb.UseVisualStyleBackColor = true;
			// 
			// convertLosslessRb
			// 
			this.convertLosslessRb.AutoSize = true;
			this.convertLosslessRb.Checked = true;
			this.convertLosslessRb.Location = new System.Drawing.Point(7, 63);
			this.convertLosslessRb.Name = "convertLosslessRb";
			this.convertLosslessRb.Size = new System.Drawing.Size(327, 19);
			this.convertLosslessRb.TabIndex = 0;
			this.convertLosslessRb.TabStop = true;
			this.convertLosslessRb.Text = "Download my books as .M4B files (Lossless Mp4a format)";
			this.convertLosslessRb.UseVisualStyleBackColor = true;
			// 
			// inProgressSelectControl
			// 
			this.inProgressSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inProgressSelectControl.Location = new System.Drawing.Point(10, 175);
			this.inProgressSelectControl.Name = "inProgressSelectControl";
			this.inProgressSelectControl.Size = new System.Drawing.Size(552, 46);
			this.inProgressSelectControl.TabIndex = 2;
			// 
			// allowLibationFixupCbox
			// 
			this.allowLibationFixupCbox.AutoSize = true;
			this.allowLibationFixupCbox.Checked = true;
			this.allowLibationFixupCbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.allowLibationFixupCbox.Location = new System.Drawing.Point(7, 22);
			this.allowLibationFixupCbox.Name = "allowLibationFixupCbox";
			this.allowLibationFixupCbox.Size = new System.Drawing.Size(262, 19);
			this.allowLibationFixupCbox.TabIndex = 0;
			this.allowLibationFixupCbox.Text = "Allow Libation to fix up audiobook metadata";
			this.allowLibationFixupCbox.UseVisualStyleBackColor = true;
			this.allowLibationFixupCbox.CheckedChanged += new System.EventHandler(this.allowLibationFixupCbox_CheckedChanged);
			// 
			// booksSelectControl
			// 
			this.booksSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksSelectControl.Location = new System.Drawing.Point(7, 37);
			this.booksSelectControl.Name = "booksSelectControl";
			this.booksSelectControl.Size = new System.Drawing.Size(895, 81);
			this.booksSelectControl.TabIndex = 1;
			// 
			// booksGb
			// 
			this.booksGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksGb.Controls.Add(this.booksSelectControl);
			this.booksGb.Controls.Add(this.booksLocationDescLbl);
			this.booksGb.Location = new System.Drawing.Point(12, 12);
			this.booksGb.Name = "booksGb";
			this.booksGb.Size = new System.Drawing.Size(908, 123);
			this.booksGb.TabIndex = 6;
			this.booksGb.TabStop = false;
			this.booksGb.Text = "Books location";
			// 
			// logsBtn
			// 
			this.logsBtn.Location = new System.Drawing.Point(826, 18);
			this.logsBtn.Name = "logsBtn";
			this.logsBtn.Size = new System.Drawing.Size(75, 64);
			this.logsBtn.TabIndex = 3;
			this.logsBtn.Text = "Open log\r\nfiles folder";
			this.logsBtn.UseVisualStyleBackColor = true;
			this.logsBtn.Click += new System.EventHandler(this.logsBtn_Click);
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(933, 421);
			this.Controls.Add(this.booksGb);
			this.Controls.Add(this.advancedSettingsGb);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "SettingsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Settings";
			this.Load += new System.EventHandler(this.SettingsDialog_Load);
			this.advancedSettingsGb.ResumeLayout(false);
			this.advancedSettingsGb.PerformLayout();
			this.booksGb.ResumeLayout(false);
			this.booksGb.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label booksLocationDescLbl;
		private System.Windows.Forms.Label inProgressDescLbl;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.GroupBox advancedSettingsGb;
        private System.Windows.Forms.CheckBox allowLibationFixupCbox;
		private DirectoryOrCustomSelectControl booksSelectControl;
		private DirectorySelectControl inProgressSelectControl;
        private System.Windows.Forms.RadioButton convertLossyRb;
        private System.Windows.Forms.RadioButton convertLosslessRb;
		private System.Windows.Forms.GroupBox booksGb;
		private System.Windows.Forms.Button logsBtn;
	}
}