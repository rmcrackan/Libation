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
            this.booksLocationDescLbl = new System.Windows.Forms.Label();
            this.inProgressDescLbl = new System.Windows.Forms.Label();
            this.saveBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.inProgressSelectControl = new LibationWinForms.Dialogs.DirectorySelectControl();
            this.allowLibationFixupCbox = new System.Windows.Forms.CheckBox();
            this.booksSelectControl = new LibationWinForms.Dialogs.DirectoryOrCustomSelectControl();
            this.convertLosslessRb = new System.Windows.Forms.RadioButton();
            this.convertLossyRb = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // booksLocationLbl
            // 
            this.booksLocationLbl.AutoSize = true;
            this.booksLocationLbl.Location = new System.Drawing.Point(13, 15);
            this.booksLocationLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.booksLocationLbl.Name = "booksLocationLbl";
            this.booksLocationLbl.Size = new System.Drawing.Size(85, 15);
            this.booksLocationLbl.TabIndex = 0;
            this.booksLocationLbl.Text = "Books location";
            // 
            // booksLocationDescLbl
            // 
            this.booksLocationDescLbl.AutoSize = true;
            this.booksLocationDescLbl.Location = new System.Drawing.Point(106, 96);
            this.booksLocationDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.booksLocationDescLbl.Name = "booksLocationDescLbl";
            this.booksLocationDescLbl.Size = new System.Drawing.Size(39, 15);
            this.booksLocationDescLbl.TabIndex = 2;
            this.booksLocationDescLbl.Text = "[desc]";
            // 
            // inProgressDescLbl
            // 
            this.inProgressDescLbl.AutoSize = true;
            this.inProgressDescLbl.Location = new System.Drawing.Point(10, 46);
            this.inProgressDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.inProgressDescLbl.Name = "inProgressDescLbl";
            this.inProgressDescLbl.Size = new System.Drawing.Size(43, 45);
            this.inProgressDescLbl.TabIndex = 1;
            this.inProgressDescLbl.Text = "[desc]\r\n[line 2]\r\n[line 3]";
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(714, 268);
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
            this.cancelBtn.Location = new System.Drawing.Point(832, 268);
            this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(88, 27);
            this.cancelBtn.TabIndex = 5;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.convertLossyRb);
            this.groupBox1.Controls.Add(this.convertLosslessRb);
            this.groupBox1.Controls.Add(this.inProgressSelectControl);
            this.groupBox1.Controls.Add(this.allowLibationFixupCbox);
            this.groupBox1.Controls.Add(this.inProgressDescLbl);
            this.groupBox1.Location = new System.Drawing.Point(19, 114);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(902, 143);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Advanced settings for control freaks";
            // 
            // inProgressSelectControl
            // 
            this.inProgressSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inProgressSelectControl.Location = new System.Drawing.Point(10, 94);
            this.inProgressSelectControl.Name = "inProgressSelectControl";
            this.inProgressSelectControl.Size = new System.Drawing.Size(885, 46);
            this.inProgressSelectControl.TabIndex = 2;
            // 
            // allowLibationFixupCbox
            // 
            this.allowLibationFixupCbox.AutoSize = true;
            this.allowLibationFixupCbox.Location = new System.Drawing.Point(10, 24);
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
            this.booksSelectControl.Location = new System.Drawing.Point(106, 12);
            this.booksSelectControl.Name = "booksSelectControl";
            this.booksSelectControl.Size = new System.Drawing.Size(815, 81);
            this.booksSelectControl.TabIndex = 1;
            // 
            // convertLosslessRb
            // 
            this.convertLosslessRb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.convertLosslessRb.AutoSize = true;
            this.convertLosslessRb.Checked = true;
            this.convertLosslessRb.Location = new System.Drawing.Point(692, 24);
            this.convertLosslessRb.Name = "convertLosslessRb";
            this.convertLosslessRb.Size = new System.Drawing.Size(108, 19);
            this.convertLosslessRb.TabIndex = 0;
            this.convertLosslessRb.TabStop = true;
            this.convertLosslessRb.Text = "Mp4a (Lossless)";
            this.convertLosslessRb.UseVisualStyleBackColor = true;
            // 
            // convertLossyRb
            // 
            this.convertLossyRb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.convertLossyRb.AutoSize = true;
            this.convertLossyRb.Location = new System.Drawing.Point(806, 24);
            this.convertLossyRb.Name = "convertLossyRb";
            this.convertLossyRb.Size = new System.Drawing.Size(89, 19);
            this.convertLossyRb.TabIndex = 0;
            this.convertLossyRb.Text = "Mp3 (Lossy)";
            this.convertLossyRb.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AcceptButton = this.saveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(933, 309);
            this.Controls.Add(this.booksSelectControl);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.booksLocationDescLbl);
            this.Controls.Add(this.booksLocationLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label booksLocationLbl;
		private System.Windows.Forms.Label booksLocationDescLbl;
		private System.Windows.Forms.Label inProgressDescLbl;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox allowLibationFixupCbox;
		private DirectoryOrCustomSelectControl booksSelectControl;
		private DirectorySelectControl inProgressSelectControl;
        private System.Windows.Forms.RadioButton convertLossyRb;
        private System.Windows.Forms.RadioButton convertLosslessRb;
    }
}