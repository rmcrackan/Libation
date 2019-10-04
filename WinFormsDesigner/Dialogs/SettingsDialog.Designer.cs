namespace WinFormsDesigner
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
            this.settingsFileLbl = new System.Windows.Forms.Label();
            this.settingsFileTb = new System.Windows.Forms.TextBox();
            this.decryptKeyLbl = new System.Windows.Forms.Label();
            this.decryptKeyTb = new System.Windows.Forms.TextBox();
            this.booksLocationLbl = new System.Windows.Forms.Label();
            this.booksLocationTb = new System.Windows.Forms.TextBox();
            this.booksLocationSearchBtn = new System.Windows.Forms.Button();
            this.settingsFileDescLbl = new System.Windows.Forms.Label();
            this.decryptKeyDescLbl = new System.Windows.Forms.Label();
            this.booksLocationDescLbl = new System.Windows.Forms.Label();
            this.libationFilesGb = new System.Windows.Forms.GroupBox();
            this.libationFilesDescLbl = new System.Windows.Forms.Label();
            this.libationFilesCustomBtn = new System.Windows.Forms.Button();
            this.libationFilesCustomTb = new System.Windows.Forms.TextBox();
            this.libationFilesCustomRb = new System.Windows.Forms.RadioButton();
            this.libationFilesMyDocsRb = new System.Windows.Forms.RadioButton();
            this.libationFilesRootRb = new System.Windows.Forms.RadioButton();
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
            this.libationFilesGb.SuspendLayout();
            this.downloadsInProgressGb.SuspendLayout();
            this.decryptInProgressGb.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsFileLbl
            // 
            this.settingsFileLbl.AutoSize = true;
            this.settingsFileLbl.Location = new System.Drawing.Point(7, 15);
            this.settingsFileLbl.Name = "settingsFileLbl";
            this.settingsFileLbl.Size = new System.Drawing.Size(61, 13);
            this.settingsFileLbl.TabIndex = 0;
            this.settingsFileLbl.Text = "Settings file";
            // 
            // settingsFileTb
            // 
            this.settingsFileTb.Location = new System.Drawing.Point(90, 12);
            this.settingsFileTb.Name = "settingsFileTb";
            this.settingsFileTb.ReadOnly = true;
            this.settingsFileTb.Size = new System.Drawing.Size(698, 20);
            this.settingsFileTb.TabIndex = 1;
            // 
            // decryptKeyLbl
            // 
            this.decryptKeyLbl.AutoSize = true;
            this.decryptKeyLbl.Location = new System.Drawing.Point(7, 59);
            this.decryptKeyLbl.Name = "decryptKeyLbl";
            this.decryptKeyLbl.Size = new System.Drawing.Size(64, 13);
            this.decryptKeyLbl.TabIndex = 3;
            this.decryptKeyLbl.Text = "Decrypt key";
            // 
            // decryptKeyTb
            // 
            this.decryptKeyTb.Location = new System.Drawing.Point(90, 56);
            this.decryptKeyTb.Name = "decryptKeyTb";
            this.decryptKeyTb.Size = new System.Drawing.Size(100, 20);
            this.decryptKeyTb.TabIndex = 4;
            // 
            // booksLocationLbl
            // 
            this.booksLocationLbl.AutoSize = true;
            this.booksLocationLbl.Location = new System.Drawing.Point(7, 103);
            this.booksLocationLbl.Name = "booksLocationLbl";
            this.booksLocationLbl.Size = new System.Drawing.Size(77, 13);
            this.booksLocationLbl.TabIndex = 6;
            this.booksLocationLbl.Text = "Books location";
            // 
            // booksLocationTb
            // 
            this.booksLocationTb.Location = new System.Drawing.Point(90, 100);
            this.booksLocationTb.Name = "booksLocationTb";
            this.booksLocationTb.Size = new System.Drawing.Size(657, 20);
            this.booksLocationTb.TabIndex = 7;
            // 
            // booksLocationSearchBtn
            // 
            this.booksLocationSearchBtn.Location = new System.Drawing.Point(753, 98);
            this.booksLocationSearchBtn.Name = "booksLocationSearchBtn";
            this.booksLocationSearchBtn.Size = new System.Drawing.Size(35, 23);
            this.booksLocationSearchBtn.TabIndex = 8;
            this.booksLocationSearchBtn.Text = "...";
            this.booksLocationSearchBtn.UseVisualStyleBackColor = true;
            // 
            // settingsFileDescLbl
            // 
            this.settingsFileDescLbl.AutoSize = true;
            this.settingsFileDescLbl.Location = new System.Drawing.Point(87, 35);
            this.settingsFileDescLbl.Name = "settingsFileDescLbl";
            this.settingsFileDescLbl.Size = new System.Drawing.Size(36, 13);
            this.settingsFileDescLbl.TabIndex = 2;
            this.settingsFileDescLbl.Text = "[desc]";
            // 
            // decryptKeyDescLbl
            // 
            this.decryptKeyDescLbl.AutoSize = true;
            this.decryptKeyDescLbl.Location = new System.Drawing.Point(87, 79);
            this.decryptKeyDescLbl.Name = "decryptKeyDescLbl";
            this.decryptKeyDescLbl.Size = new System.Drawing.Size(36, 13);
            this.decryptKeyDescLbl.TabIndex = 5;
            this.decryptKeyDescLbl.Text = "[desc]";
            // 
            // booksLocationDescLbl
            // 
            this.booksLocationDescLbl.AutoSize = true;
            this.booksLocationDescLbl.Location = new System.Drawing.Point(87, 123);
            this.booksLocationDescLbl.Name = "booksLocationDescLbl";
            this.booksLocationDescLbl.Size = new System.Drawing.Size(36, 13);
            this.booksLocationDescLbl.TabIndex = 9;
            this.booksLocationDescLbl.Text = "[desc]";
            // 
            // libationFilesGb
            // 
            this.libationFilesGb.Controls.Add(this.libationFilesDescLbl);
            this.libationFilesGb.Controls.Add(this.libationFilesCustomBtn);
            this.libationFilesGb.Controls.Add(this.libationFilesCustomTb);
            this.libationFilesGb.Controls.Add(this.libationFilesCustomRb);
            this.libationFilesGb.Controls.Add(this.libationFilesMyDocsRb);
            this.libationFilesGb.Controls.Add(this.libationFilesRootRb);
            this.libationFilesGb.Location = new System.Drawing.Point(12, 139);
            this.libationFilesGb.Name = "libationFilesGb";
            this.libationFilesGb.Size = new System.Drawing.Size(776, 131);
            this.libationFilesGb.TabIndex = 10;
            this.libationFilesGb.TabStop = false;
            this.libationFilesGb.Text = "Libation files";
            // 
            // libationFilesDescLbl
            // 
            this.libationFilesDescLbl.AutoSize = true;
            this.libationFilesDescLbl.Location = new System.Drawing.Point(6, 16);
            this.libationFilesDescLbl.Name = "libationFilesDescLbl";
            this.libationFilesDescLbl.Size = new System.Drawing.Size(36, 13);
            this.libationFilesDescLbl.TabIndex = 0;
            this.libationFilesDescLbl.Text = "[desc]";
            // 
            // libationFilesCustomBtn
            // 
            this.libationFilesCustomBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.libationFilesCustomBtn.Location = new System.Drawing.Point(741, 102);
            this.libationFilesCustomBtn.Name = "libationFilesCustomBtn";
            this.libationFilesCustomBtn.Size = new System.Drawing.Size(35, 23);
            this.libationFilesCustomBtn.TabIndex = 5;
            this.libationFilesCustomBtn.Text = "...";
            this.libationFilesCustomBtn.UseVisualStyleBackColor = true;
            // 
            // libationFilesCustomTb
            // 
            this.libationFilesCustomTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.libationFilesCustomTb.Location = new System.Drawing.Point(29, 104);
            this.libationFilesCustomTb.Name = "libationFilesCustomTb";
            this.libationFilesCustomTb.Size = new System.Drawing.Size(706, 20);
            this.libationFilesCustomTb.TabIndex = 4;
            // 
            // libationFilesCustomRb
            // 
            this.libationFilesCustomRb.AutoSize = true;
            this.libationFilesCustomRb.Location = new System.Drawing.Point(9, 107);
            this.libationFilesCustomRb.Name = "libationFilesCustomRb";
            this.libationFilesCustomRb.Size = new System.Drawing.Size(14, 13);
            this.libationFilesCustomRb.TabIndex = 3;
            this.libationFilesCustomRb.TabStop = true;
            this.libationFilesCustomRb.UseVisualStyleBackColor = true;
            // 
            // libationFilesMyDocsRb
            // 
            this.libationFilesMyDocsRb.AutoSize = true;
            this.libationFilesMyDocsRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.libationFilesMyDocsRb.Location = new System.Drawing.Point(9, 68);
            this.libationFilesMyDocsRb.Name = "libationFilesMyDocsRb";
            this.libationFilesMyDocsRb.Size = new System.Drawing.Size(111, 30);
            this.libationFilesMyDocsRb.TabIndex = 2;
            this.libationFilesMyDocsRb.TabStop = true;
            this.libationFilesMyDocsRb.Text = "[desc]\r\n[myDocs\\Libation]";
            this.libationFilesMyDocsRb.UseVisualStyleBackColor = true;
            // 
            // libationFilesRootRb
            // 
            this.libationFilesRootRb.AutoSize = true;
            this.libationFilesRootRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.libationFilesRootRb.Location = new System.Drawing.Point(9, 32);
            this.libationFilesRootRb.Name = "libationFilesRootRb";
            this.libationFilesRootRb.Size = new System.Drawing.Size(113, 30);
            this.libationFilesRootRb.TabIndex = 1;
            this.libationFilesRootRb.TabStop = true;
            this.libationFilesRootRb.Text = "[desc]\r\n[exeRoot\\Libation]";
            this.libationFilesRootRb.UseVisualStyleBackColor = true;
            // 
            // downloadsInProgressGb
            // 
            this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressLibationFilesRb);
            this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressWinTempRb);
            this.downloadsInProgressGb.Controls.Add(this.downloadsInProgressDescLbl);
            this.downloadsInProgressGb.Location = new System.Drawing.Point(12, 276);
            this.downloadsInProgressGb.Name = "downloadsInProgressGb";
            this.downloadsInProgressGb.Size = new System.Drawing.Size(776, 117);
            this.downloadsInProgressGb.TabIndex = 11;
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
            this.decryptInProgressGb.Location = new System.Drawing.Point(12, 399);
            this.decryptInProgressGb.Name = "decryptInProgressGb";
            this.decryptInProgressGb.Size = new System.Drawing.Size(776, 117);
            this.decryptInProgressGb.TabIndex = 12;
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
            this.decryptInProgressLibationFilesRb.TabIndex = 3;
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
            this.decryptInProgressWinTempRb.TabIndex = 2;
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
            this.decryptInProgressDescLbl.TabIndex = 1;
            this.decryptInProgressDescLbl.Text = "[desc]\r\n[line 2]";
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(612, 522);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 13;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(713, 522);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 14;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AcceptButton = this.saveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(800, 557);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.decryptInProgressGb);
            this.Controls.Add(this.downloadsInProgressGb);
            this.Controls.Add(this.libationFilesGb);
            this.Controls.Add(this.booksLocationDescLbl);
            this.Controls.Add(this.decryptKeyDescLbl);
            this.Controls.Add(this.settingsFileDescLbl);
            this.Controls.Add(this.booksLocationSearchBtn);
            this.Controls.Add(this.booksLocationTb);
            this.Controls.Add(this.booksLocationLbl);
            this.Controls.Add(this.decryptKeyTb);
            this.Controls.Add(this.decryptKeyLbl);
            this.Controls.Add(this.settingsFileTb);
            this.Controls.Add(this.settingsFileLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Settings";
            this.libationFilesGb.ResumeLayout(false);
            this.libationFilesGb.PerformLayout();
            this.downloadsInProgressGb.ResumeLayout(false);
            this.downloadsInProgressGb.PerformLayout();
            this.decryptInProgressGb.ResumeLayout(false);
            this.decryptInProgressGb.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label settingsFileLbl;
        private System.Windows.Forms.TextBox settingsFileTb;
        private System.Windows.Forms.Label decryptKeyLbl;
        private System.Windows.Forms.TextBox decryptKeyTb;
        private System.Windows.Forms.Label booksLocationLbl;
        private System.Windows.Forms.TextBox booksLocationTb;
        private System.Windows.Forms.Button booksLocationSearchBtn;
        private System.Windows.Forms.Label settingsFileDescLbl;
        private System.Windows.Forms.Label decryptKeyDescLbl;
        private System.Windows.Forms.Label booksLocationDescLbl;
        private System.Windows.Forms.GroupBox libationFilesGb;
        private System.Windows.Forms.Button libationFilesCustomBtn;
        private System.Windows.Forms.TextBox libationFilesCustomTb;
        private System.Windows.Forms.RadioButton libationFilesCustomRb;
        private System.Windows.Forms.RadioButton libationFilesMyDocsRb;
        private System.Windows.Forms.RadioButton libationFilesRootRb;
        private System.Windows.Forms.Label libationFilesDescLbl;
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
    }
}