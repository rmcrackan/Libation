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
			this.logsBtn = new System.Windows.Forms.Button();
			this.booksSelectControl = new LibationWinForms.Dialogs.DirectoryOrCustomSelectControl();
			this.booksGb = new System.Windows.Forms.GroupBox();
			this.loggingLevelLbl = new System.Windows.Forms.Label();
			this.loggingLevelCb = new System.Windows.Forms.ComboBox();
			this.decryptAndConvertGb = new System.Windows.Forms.GroupBox();
			this.badBookGb = new System.Windows.Forms.GroupBox();
			this.badBookAskRb = new System.Windows.Forms.RadioButton();
			this.badBookAbortRb = new System.Windows.Forms.RadioButton();
			this.badBookRetryRb = new System.Windows.Forms.RadioButton();
			this.badBookIgnoreRb = new System.Windows.Forms.RadioButton();
			this.advancedSettingsGb.SuspendLayout();
			this.booksGb.SuspendLayout();
			this.decryptAndConvertGb.SuspendLayout();
			this.badBookGb.SuspendLayout();
			this.SuspendLayout();
			// 
			// booksLocationDescLbl
			// 
			this.booksLocationDescLbl.AutoSize = true;
			this.booksLocationDescLbl.Location = new System.Drawing.Point(7, 19);
			this.booksLocationDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.booksLocationDescLbl.Name = "booksLocationDescLbl";
			this.booksLocationDescLbl.Size = new System.Drawing.Size(69, 15);
			this.booksLocationDescLbl.TabIndex = 1;
			this.booksLocationDescLbl.Text = "[book desc]";
			// 
			// inProgressDescLbl
			// 
			this.inProgressDescLbl.AutoSize = true;
			this.inProgressDescLbl.Location = new System.Drawing.Point(8, 149);
			this.inProgressDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.inProgressDescLbl.Name = "inProgressDescLbl";
			this.inProgressDescLbl.Size = new System.Drawing.Size(43, 45);
			this.inProgressDescLbl.TabIndex = 15;
			this.inProgressDescLbl.Text = "[desc]\r\n[line 2]\r\n[line 3]";
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(714, 445);
			this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 17;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(832, 445);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 18;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// advancedSettingsGb
			// 
			this.advancedSettingsGb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedSettingsGb.Controls.Add(this.badBookGb);
			this.advancedSettingsGb.Controls.Add(this.decryptAndConvertGb);
			this.advancedSettingsGb.Controls.Add(this.inProgressSelectControl);
			this.advancedSettingsGb.Controls.Add(this.inProgressDescLbl);
			this.advancedSettingsGb.Location = new System.Drawing.Point(12, 176);
			this.advancedSettingsGb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.advancedSettingsGb.Name = "advancedSettingsGb";
			this.advancedSettingsGb.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.advancedSettingsGb.Size = new System.Drawing.Size(908, 258);
			this.advancedSettingsGb.TabIndex = 6;
			this.advancedSettingsGb.TabStop = false;
			this.advancedSettingsGb.Text = "Advanced settings for control freaks";
			// 
			// convertLossyRb
			// 
			this.convertLossyRb.AutoSize = true;
			this.convertLossyRb.Location = new System.Drawing.Point(6, 81);
			this.convertLossyRb.Name = "convertLossyRb";
			this.convertLossyRb.Size = new System.Drawing.Size(242, 19);
			this.convertLossyRb.TabIndex = 10;
			this.convertLossyRb.Text = "Download my books as .MP3 files (Lossy)";
			this.convertLossyRb.UseVisualStyleBackColor = true;
			// 
			// convertLosslessRb
			// 
			this.convertLosslessRb.AutoSize = true;
			this.convertLosslessRb.Checked = true;
			this.convertLosslessRb.Location = new System.Drawing.Point(6, 56);
			this.convertLosslessRb.Name = "convertLosslessRb";
			this.convertLosslessRb.Size = new System.Drawing.Size(327, 19);
			this.convertLosslessRb.TabIndex = 9;
			this.convertLosslessRb.TabStop = true;
			this.convertLosslessRb.Text = "Download my books as .M4B files (Lossless Mp4a format)";
			this.convertLosslessRb.UseVisualStyleBackColor = true;
			// 
			// inProgressSelectControl
			// 
			this.inProgressSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inProgressSelectControl.Location = new System.Drawing.Point(7, 197);
			this.inProgressSelectControl.Name = "inProgressSelectControl";
			this.inProgressSelectControl.Size = new System.Drawing.Size(552, 52);
			this.inProgressSelectControl.TabIndex = 16;
			// 
			// allowLibationFixupCbox
			// 
			this.allowLibationFixupCbox.AutoSize = true;
			this.allowLibationFixupCbox.Checked = true;
			this.allowLibationFixupCbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.allowLibationFixupCbox.Location = new System.Drawing.Point(6, 22);
			this.allowLibationFixupCbox.Name = "allowLibationFixupCbox";
			this.allowLibationFixupCbox.Size = new System.Drawing.Size(262, 19);
			this.allowLibationFixupCbox.TabIndex = 8;
			this.allowLibationFixupCbox.Text = "Allow Libation to fix up audiobook metadata";
			this.allowLibationFixupCbox.UseVisualStyleBackColor = true;
			this.allowLibationFixupCbox.CheckedChanged += new System.EventHandler(this.allowLibationFixupCbox_CheckedChanged);
			// 
			// logsBtn
			// 
			this.logsBtn.Location = new System.Drawing.Point(262, 147);
			this.logsBtn.Name = "logsBtn";
			this.logsBtn.Size = new System.Drawing.Size(132, 23);
			this.logsBtn.TabIndex = 5;
			this.logsBtn.Text = "Open log folder";
			this.logsBtn.UseVisualStyleBackColor = true;
			this.logsBtn.Click += new System.EventHandler(this.logsBtn_Click);
			// 
			// booksSelectControl
			// 
			this.booksSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksSelectControl.Location = new System.Drawing.Point(7, 37);
			this.booksSelectControl.Name = "booksSelectControl";
			this.booksSelectControl.Size = new System.Drawing.Size(895, 87);
			this.booksSelectControl.TabIndex = 2;
			// 
			// booksGb
			// 
			this.booksGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksGb.Controls.Add(this.booksSelectControl);
			this.booksGb.Controls.Add(this.booksLocationDescLbl);
			this.booksGb.Location = new System.Drawing.Point(12, 12);
			this.booksGb.Name = "booksGb";
			this.booksGb.Size = new System.Drawing.Size(908, 129);
			this.booksGb.TabIndex = 0;
			this.booksGb.TabStop = false;
			this.booksGb.Text = "Books location";
			// 
			// loggingLevelLbl
			// 
			this.loggingLevelLbl.AutoSize = true;
			this.loggingLevelLbl.Location = new System.Drawing.Point(12, 150);
			this.loggingLevelLbl.Name = "loggingLevelLbl";
			this.loggingLevelLbl.Size = new System.Drawing.Size(78, 15);
			this.loggingLevelLbl.TabIndex = 3;
			this.loggingLevelLbl.Text = "Logging level";
			// 
			// loggingLevelCb
			// 
			this.loggingLevelCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.loggingLevelCb.FormattingEnabled = true;
			this.loggingLevelCb.Location = new System.Drawing.Point(96, 147);
			this.loggingLevelCb.Name = "loggingLevelCb";
			this.loggingLevelCb.Size = new System.Drawing.Size(129, 23);
			this.loggingLevelCb.TabIndex = 4;
			// 
			// decryptAndConvertGb
			// 
			this.decryptAndConvertGb.Controls.Add(this.allowLibationFixupCbox);
			this.decryptAndConvertGb.Controls.Add(this.convertLossyRb);
			this.decryptAndConvertGb.Controls.Add(this.convertLosslessRb);
			this.decryptAndConvertGb.Location = new System.Drawing.Point(7, 22);
			this.decryptAndConvertGb.Name = "decryptAndConvertGb";
			this.decryptAndConvertGb.Size = new System.Drawing.Size(359, 124);
			this.decryptAndConvertGb.TabIndex = 7;
			this.decryptAndConvertGb.TabStop = false;
			this.decryptAndConvertGb.Text = "Decrypt and convert";
			// 
			// badBookGb
			// 
			this.badBookGb.Controls.Add(this.badBookIgnoreRb);
			this.badBookGb.Controls.Add(this.badBookRetryRb);
			this.badBookGb.Controls.Add(this.badBookAbortRb);
			this.badBookGb.Controls.Add(this.badBookAskRb);
			this.badBookGb.Location = new System.Drawing.Point(372, 22);
			this.badBookGb.Name = "badBookGb";
			this.badBookGb.Size = new System.Drawing.Size(529, 124);
			this.badBookGb.TabIndex = 11;
			this.badBookGb.TabStop = false;
			this.badBookGb.Text = "badBookGb";
			// 
			// badBookAskRb
			// 
			this.badBookAskRb.AutoSize = true;
			this.badBookAskRb.Location = new System.Drawing.Point(6, 22);
			this.badBookAskRb.Name = "badBookAskRb";
			this.badBookAskRb.Size = new System.Drawing.Size(94, 19);
			this.badBookAskRb.TabIndex = 12;
			this.badBookAskRb.TabStop = true;
			this.badBookAskRb.Text = "radioButton1";
			this.badBookAskRb.UseVisualStyleBackColor = true;
			// 
			// badBookAbortRb
			// 
			this.badBookAbortRb.AutoSize = true;
			this.badBookAbortRb.Location = new System.Drawing.Point(6, 47);
			this.badBookAbortRb.Name = "badBookAbortRb";
			this.badBookAbortRb.Size = new System.Drawing.Size(94, 19);
			this.badBookAbortRb.TabIndex = 13;
			this.badBookAbortRb.TabStop = true;
			this.badBookAbortRb.Text = "radioButton2";
			this.badBookAbortRb.UseVisualStyleBackColor = true;
			// 
			// badBookRetryRb
			// 
			this.badBookRetryRb.AutoSize = true;
			this.badBookRetryRb.Location = new System.Drawing.Point(6, 72);
			this.badBookRetryRb.Name = "badBookRetryRb";
			this.badBookRetryRb.Size = new System.Drawing.Size(94, 19);
			this.badBookRetryRb.TabIndex = 14;
			this.badBookRetryRb.TabStop = true;
			this.badBookRetryRb.Text = "radioButton3";
			this.badBookRetryRb.UseVisualStyleBackColor = true;
			// 
			// badBookIgnoreRb
			// 
			this.badBookIgnoreRb.AutoSize = true;
			this.badBookIgnoreRb.Location = new System.Drawing.Point(6, 97);
			this.badBookIgnoreRb.Name = "badBookIgnoreRb";
			this.badBookIgnoreRb.Size = new System.Drawing.Size(94, 19);
			this.badBookIgnoreRb.TabIndex = 15;
			this.badBookIgnoreRb.TabStop = true;
			this.badBookIgnoreRb.Text = "radioButton4";
			this.badBookIgnoreRb.UseVisualStyleBackColor = true;
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(933, 488);
			this.Controls.Add(this.logsBtn);
			this.Controls.Add(this.loggingLevelCb);
			this.Controls.Add(this.loggingLevelLbl);
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
			this.decryptAndConvertGb.ResumeLayout(false);
			this.decryptAndConvertGb.PerformLayout();
			this.badBookGb.ResumeLayout(false);
			this.badBookGb.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

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
		private System.Windows.Forms.Label loggingLevelLbl;
		private System.Windows.Forms.ComboBox loggingLevelCb;
		private System.Windows.Forms.GroupBox decryptAndConvertGb;
		private System.Windows.Forms.GroupBox badBookGb;
		private System.Windows.Forms.RadioButton badBookRetryRb;
		private System.Windows.Forms.RadioButton badBookAbortRb;
		private System.Windows.Forms.RadioButton badBookAskRb;
		private System.Windows.Forms.RadioButton badBookIgnoreRb;
	}
}