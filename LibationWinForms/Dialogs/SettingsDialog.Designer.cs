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
			this.importEpisodesCb = new System.Windows.Forms.CheckBox();
			this.downloadEpisodesCb = new System.Windows.Forms.CheckBox();
			this.badBookGb = new System.Windows.Forms.GroupBox();
			this.badBookIgnoreRb = new System.Windows.Forms.RadioButton();
			this.badBookRetryRb = new System.Windows.Forms.RadioButton();
			this.badBookAbortRb = new System.Windows.Forms.RadioButton();
			this.badBookAskRb = new System.Windows.Forms.RadioButton();
			this.decryptAndConvertGb = new System.Windows.Forms.GroupBox();
			this.stripAudibleBrandingCbox = new System.Windows.Forms.CheckBox();
			this.splitFilesByChapterCbox = new System.Windows.Forms.CheckBox();
			this.allowLibationFixupCbox = new System.Windows.Forms.CheckBox();
			this.convertLossyRb = new System.Windows.Forms.RadioButton();
			this.convertLosslessRb = new System.Windows.Forms.RadioButton();
			this.inProgressSelectControl = new LibationWinForms.Dialogs.DirectorySelectControl();
			this.logsBtn = new System.Windows.Forms.Button();
			this.booksSelectControl = new LibationWinForms.Dialogs.DirectoryOrCustomSelectControl();
			this.loggingLevelLbl = new System.Windows.Forms.Label();
			this.loggingLevelCb = new System.Windows.Forms.ComboBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tab1ImportantSettings = new System.Windows.Forms.TabPage();
			this.booksGb = new System.Windows.Forms.GroupBox();
			this.tab2ImportLibrary = new System.Windows.Forms.TabPage();
			this.showImportedStatsCb = new System.Windows.Forms.CheckBox();
			this.tab3DownloadDecrypt = new System.Windows.Forms.TabPage();
			this.inProgressFilesGb = new System.Windows.Forms.GroupBox();
			this.customFileNamingGb = new System.Windows.Forms.GroupBox();
			this.chapterFileTemplateBtn = new System.Windows.Forms.Button();
			this.chapterFileTemplateTb = new System.Windows.Forms.TextBox();
			this.chapterFileTemplateLbl = new System.Windows.Forms.Label();
			this.fileTemplateBtn = new System.Windows.Forms.Button();
			this.fileTemplateTb = new System.Windows.Forms.TextBox();
			this.fileTemplateLbl = new System.Windows.Forms.Label();
			this.folderTemplateBtn = new System.Windows.Forms.Button();
			this.folderTemplateTb = new System.Windows.Forms.TextBox();
			this.folderTemplateLbl = new System.Windows.Forms.Label();
			this.badBookGb.SuspendLayout();
			this.decryptAndConvertGb.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tab1ImportantSettings.SuspendLayout();
			this.booksGb.SuspendLayout();
			this.tab2ImportLibrary.SuspendLayout();
			this.tab3DownloadDecrypt.SuspendLayout();
			this.inProgressFilesGb.SuspendLayout();
			this.customFileNamingGb.SuspendLayout();
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
			this.inProgressDescLbl.Location = new System.Drawing.Point(7, 19);
			this.inProgressDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.inProgressDescLbl.Name = "inProgressDescLbl";
			this.inProgressDescLbl.Size = new System.Drawing.Size(100, 45);
			this.inProgressDescLbl.TabIndex = 18;
			this.inProgressDescLbl.Text = "[in progress desc]\r\n[line 2]\r\n[line 3]";
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(714, 523);
			this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 98;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(832, 523);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 99;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// importEpisodesCb
			// 
			this.importEpisodesCb.AutoSize = true;
			this.importEpisodesCb.Location = new System.Drawing.Point(6, 31);
			this.importEpisodesCb.Name = "importEpisodesCb";
			this.importEpisodesCb.Size = new System.Drawing.Size(146, 19);
			this.importEpisodesCb.TabIndex = 2;
			this.importEpisodesCb.Text = "[import episodes desc]";
			this.importEpisodesCb.UseVisualStyleBackColor = true;
			// 
			// downloadEpisodesCb
			// 
			this.downloadEpisodesCb.AutoSize = true;
			this.downloadEpisodesCb.Location = new System.Drawing.Point(6, 56);
			this.downloadEpisodesCb.Name = "downloadEpisodesCb";
			this.downloadEpisodesCb.Size = new System.Drawing.Size(163, 19);
			this.downloadEpisodesCb.TabIndex = 3;
			this.downloadEpisodesCb.Text = "[download episodes desc]";
			this.downloadEpisodesCb.UseVisualStyleBackColor = true;
			// 
			// badBookGb
			// 
			this.badBookGb.Controls.Add(this.badBookIgnoreRb);
			this.badBookGb.Controls.Add(this.badBookRetryRb);
			this.badBookGb.Controls.Add(this.badBookAbortRb);
			this.badBookGb.Controls.Add(this.badBookAskRb);
			this.badBookGb.Location = new System.Drawing.Point(371, 6);
			this.badBookGb.Name = "badBookGb";
			this.badBookGb.Size = new System.Drawing.Size(524, 168);
			this.badBookGb.TabIndex = 13;
			this.badBookGb.TabStop = false;
			this.badBookGb.Text = "[bad book desc]";
			// 
			// badBookIgnoreRb
			// 
			this.badBookIgnoreRb.AutoSize = true;
			this.badBookIgnoreRb.Location = new System.Drawing.Point(6, 124);
			this.badBookIgnoreRb.Name = "badBookIgnoreRb";
			this.badBookIgnoreRb.Size = new System.Drawing.Size(94, 19);
			this.badBookIgnoreRb.TabIndex = 17;
			this.badBookIgnoreRb.TabStop = true;
			this.badBookIgnoreRb.Text = "[ignore desc]";
			this.badBookIgnoreRb.UseVisualStyleBackColor = true;
			// 
			// badBookRetryRb
			// 
			this.badBookRetryRb.AutoSize = true;
			this.badBookRetryRb.Location = new System.Drawing.Point(6, 90);
			this.badBookRetryRb.Name = "badBookRetryRb";
			this.badBookRetryRb.Size = new System.Drawing.Size(84, 19);
			this.badBookRetryRb.TabIndex = 16;
			this.badBookRetryRb.TabStop = true;
			this.badBookRetryRb.Text = "[retry desc]";
			this.badBookRetryRb.UseVisualStyleBackColor = true;
			// 
			// badBookAbortRb
			// 
			this.badBookAbortRb.AutoSize = true;
			this.badBookAbortRb.Location = new System.Drawing.Point(6, 56);
			this.badBookAbortRb.Name = "badBookAbortRb";
			this.badBookAbortRb.Size = new System.Drawing.Size(88, 19);
			this.badBookAbortRb.TabIndex = 15;
			this.badBookAbortRb.TabStop = true;
			this.badBookAbortRb.Text = "[abort desc]";
			this.badBookAbortRb.UseVisualStyleBackColor = true;
			// 
			// badBookAskRb
			// 
			this.badBookAskRb.AutoSize = true;
			this.badBookAskRb.Location = new System.Drawing.Point(6, 22);
			this.badBookAskRb.Name = "badBookAskRb";
			this.badBookAskRb.Size = new System.Drawing.Size(77, 19);
			this.badBookAskRb.TabIndex = 14;
			this.badBookAskRb.TabStop = true;
			this.badBookAskRb.Text = "[ask desc]";
			this.badBookAskRb.UseVisualStyleBackColor = true;
			// 
			// decryptAndConvertGb
			// 
			this.decryptAndConvertGb.Controls.Add(this.stripAudibleBrandingCbox);
			this.decryptAndConvertGb.Controls.Add(this.splitFilesByChapterCbox);
			this.decryptAndConvertGb.Controls.Add(this.allowLibationFixupCbox);
			this.decryptAndConvertGb.Controls.Add(this.convertLossyRb);
			this.decryptAndConvertGb.Controls.Add(this.convertLosslessRb);
			this.decryptAndConvertGb.Location = new System.Drawing.Point(6, 6);
			this.decryptAndConvertGb.Name = "decryptAndConvertGb";
			this.decryptAndConvertGb.Size = new System.Drawing.Size(359, 168);
			this.decryptAndConvertGb.TabIndex = 9;
			this.decryptAndConvertGb.TabStop = false;
			this.decryptAndConvertGb.Text = "Decrypt and convert";
			// 
			// stripAudibleBrandingCbox
			// 
			this.stripAudibleBrandingCbox.AutoSize = true;
			this.stripAudibleBrandingCbox.Location = new System.Drawing.Point(6, 72);
			this.stripAudibleBrandingCbox.Name = "stripAudibleBrandingCbox";
			this.stripAudibleBrandingCbox.Size = new System.Drawing.Size(174, 19);
			this.stripAudibleBrandingCbox.TabIndex = 13;
			this.stripAudibleBrandingCbox.Text = "[StripAudibleBranding desc]";
			this.stripAudibleBrandingCbox.UseVisualStyleBackColor = true;
			// 
			// splitFilesByChapterCbox
			// 
			this.splitFilesByChapterCbox.AutoSize = true;
			this.splitFilesByChapterCbox.Location = new System.Drawing.Point(6, 48);
			this.splitFilesByChapterCbox.Name = "splitFilesByChapterCbox";
			this.splitFilesByChapterCbox.Size = new System.Drawing.Size(162, 19);
			this.splitFilesByChapterCbox.TabIndex = 13;
			this.splitFilesByChapterCbox.Text = "[SplitFilesByChapter desc]";
			this.splitFilesByChapterCbox.UseVisualStyleBackColor = true;
			// 
			// allowLibationFixupCbox
			// 
			this.allowLibationFixupCbox.AutoSize = true;
			this.allowLibationFixupCbox.Checked = true;
			this.allowLibationFixupCbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.allowLibationFixupCbox.Location = new System.Drawing.Point(6, 23);
			this.allowLibationFixupCbox.Name = "allowLibationFixupCbox";
			this.allowLibationFixupCbox.Size = new System.Drawing.Size(163, 19);
			this.allowLibationFixupCbox.TabIndex = 10;
			this.allowLibationFixupCbox.Text = "[AllowLibationFixup desc]";
			this.allowLibationFixupCbox.UseVisualStyleBackColor = true;
			this.allowLibationFixupCbox.CheckedChanged += new System.EventHandler(this.allowLibationFixupCbox_CheckedChanged);
			// 
			// convertLossyRb
			// 
			this.convertLossyRb.AutoSize = true;
			this.convertLossyRb.Location = new System.Drawing.Point(6, 143);
			this.convertLossyRb.Name = "convertLossyRb";
			this.convertLossyRb.Size = new System.Drawing.Size(329, 19);
			this.convertLossyRb.TabIndex = 12;
			this.convertLossyRb.Text = "Download my books as .MP3 files (transcode if necessary)";
			this.convertLossyRb.UseVisualStyleBackColor = true;
			// 
			// convertLosslessRb
			// 
			this.convertLosslessRb.AutoSize = true;
			this.convertLosslessRb.Checked = true;
			this.convertLosslessRb.Location = new System.Drawing.Point(6, 118);
			this.convertLosslessRb.Name = "convertLosslessRb";
			this.convertLosslessRb.Size = new System.Drawing.Size(335, 19);
			this.convertLosslessRb.TabIndex = 11;
			this.convertLosslessRb.TabStop = true;
			this.convertLosslessRb.Text = "Download my books in the original audio format (Lossless)";
			this.convertLosslessRb.UseVisualStyleBackColor = true;
			// 
			// inProgressSelectControl
			// 
			this.inProgressSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inProgressSelectControl.Location = new System.Drawing.Point(7, 68);
			this.inProgressSelectControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.inProgressSelectControl.Name = "inProgressSelectControl";
			this.inProgressSelectControl.Size = new System.Drawing.Size(875, 52);
			this.inProgressSelectControl.TabIndex = 19;
			// 
			// logsBtn
			// 
			this.logsBtn.Location = new System.Drawing.Point(256, 169);
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
			this.booksSelectControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.booksSelectControl.Name = "booksSelectControl";
			this.booksSelectControl.Size = new System.Drawing.Size(876, 87);
			this.booksSelectControl.TabIndex = 2;
			// 
			// loggingLevelLbl
			// 
			this.loggingLevelLbl.AutoSize = true;
			this.loggingLevelLbl.Location = new System.Drawing.Point(6, 172);
			this.loggingLevelLbl.Name = "loggingLevelLbl";
			this.loggingLevelLbl.Size = new System.Drawing.Size(78, 15);
			this.loggingLevelLbl.TabIndex = 3;
			this.loggingLevelLbl.Text = "Logging level";
			// 
			// loggingLevelCb
			// 
			this.loggingLevelCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.loggingLevelCb.FormattingEnabled = true;
			this.loggingLevelCb.Location = new System.Drawing.Point(90, 169);
			this.loggingLevelCb.Name = "loggingLevelCb";
			this.loggingLevelCb.Size = new System.Drawing.Size(129, 23);
			this.loggingLevelCb.TabIndex = 4;
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tab1ImportantSettings);
			this.tabControl.Controls.Add(this.tab2ImportLibrary);
			this.tabControl.Controls.Add(this.tab3DownloadDecrypt);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(909, 505);
			this.tabControl.TabIndex = 100;
			// 
			// tab1ImportantSettings
			// 
			this.tab1ImportantSettings.Controls.Add(this.booksGb);
			this.tab1ImportantSettings.Controls.Add(this.logsBtn);
			this.tab1ImportantSettings.Controls.Add(this.loggingLevelCb);
			this.tab1ImportantSettings.Controls.Add(this.loggingLevelLbl);
			this.tab1ImportantSettings.Location = new System.Drawing.Point(4, 24);
			this.tab1ImportantSettings.Name = "tab1ImportantSettings";
			this.tab1ImportantSettings.Padding = new System.Windows.Forms.Padding(3);
			this.tab1ImportantSettings.Size = new System.Drawing.Size(901, 459);
			this.tab1ImportantSettings.TabIndex = 0;
			this.tab1ImportantSettings.Text = "Important settings";
			this.tab1ImportantSettings.UseVisualStyleBackColor = true;
			// 
			// booksGb
			// 
			this.booksGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksGb.Controls.Add(this.booksSelectControl);
			this.booksGb.Controls.Add(this.booksLocationDescLbl);
			this.booksGb.Location = new System.Drawing.Point(6, 6);
			this.booksGb.Name = "booksGb";
			this.booksGb.Size = new System.Drawing.Size(889, 129);
			this.booksGb.TabIndex = 0;
			this.booksGb.TabStop = false;
			this.booksGb.Text = "Books location";
			// 
			// tab2ImportLibrary
			// 
			this.tab2ImportLibrary.Controls.Add(this.showImportedStatsCb);
			this.tab2ImportLibrary.Controls.Add(this.importEpisodesCb);
			this.tab2ImportLibrary.Controls.Add(this.downloadEpisodesCb);
			this.tab2ImportLibrary.Location = new System.Drawing.Point(4, 24);
			this.tab2ImportLibrary.Name = "tab2ImportLibrary";
			this.tab2ImportLibrary.Padding = new System.Windows.Forms.Padding(3);
			this.tab2ImportLibrary.Size = new System.Drawing.Size(901, 459);
			this.tab2ImportLibrary.TabIndex = 1;
			this.tab2ImportLibrary.Text = "Import library";
			this.tab2ImportLibrary.UseVisualStyleBackColor = true;
			// 
			// showImportedStatsCb
			// 
			this.showImportedStatsCb.AutoSize = true;
			this.showImportedStatsCb.Location = new System.Drawing.Point(6, 6);
			this.showImportedStatsCb.Name = "showImportedStatsCb";
			this.showImportedStatsCb.Size = new System.Drawing.Size(168, 19);
			this.showImportedStatsCb.TabIndex = 1;
			this.showImportedStatsCb.Text = "[show imported stats desc]";
			this.showImportedStatsCb.UseVisualStyleBackColor = true;
			// 
			// tab3DownloadDecrypt
			// 
			this.tab3DownloadDecrypt.Controls.Add(this.inProgressFilesGb);
			this.tab3DownloadDecrypt.Controls.Add(this.customFileNamingGb);
			this.tab3DownloadDecrypt.Controls.Add(this.decryptAndConvertGb);
			this.tab3DownloadDecrypt.Controls.Add(this.badBookGb);
			this.tab3DownloadDecrypt.Location = new System.Drawing.Point(4, 24);
			this.tab3DownloadDecrypt.Name = "tab3DownloadDecrypt";
			this.tab3DownloadDecrypt.Padding = new System.Windows.Forms.Padding(3);
			this.tab3DownloadDecrypt.Size = new System.Drawing.Size(901, 477);
			this.tab3DownloadDecrypt.TabIndex = 2;
			this.tab3DownloadDecrypt.Text = "Download/Decrypt";
			this.tab3DownloadDecrypt.UseVisualStyleBackColor = true;
			// 
			// inProgressFilesGb
			// 
			this.inProgressFilesGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inProgressFilesGb.Controls.Add(this.inProgressDescLbl);
			this.inProgressFilesGb.Controls.Add(this.inProgressSelectControl);
			this.inProgressFilesGb.Location = new System.Drawing.Point(7, 343);
			this.inProgressFilesGb.Name = "inProgressFilesGb";
			this.inProgressFilesGb.Size = new System.Drawing.Size(888, 128);
			this.inProgressFilesGb.TabIndex = 21;
			this.inProgressFilesGb.TabStop = false;
			this.inProgressFilesGb.Text = "In progress files";
			// 
			// customFileNamingGb
			// 
			this.customFileNamingGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.customFileNamingGb.Controls.Add(this.chapterFileTemplateBtn);
			this.customFileNamingGb.Controls.Add(this.chapterFileTemplateTb);
			this.customFileNamingGb.Controls.Add(this.chapterFileTemplateLbl);
			this.customFileNamingGb.Controls.Add(this.fileTemplateBtn);
			this.customFileNamingGb.Controls.Add(this.fileTemplateTb);
			this.customFileNamingGb.Controls.Add(this.fileTemplateLbl);
			this.customFileNamingGb.Controls.Add(this.folderTemplateBtn);
			this.customFileNamingGb.Controls.Add(this.folderTemplateTb);
			this.customFileNamingGb.Controls.Add(this.folderTemplateLbl);
			this.customFileNamingGb.Location = new System.Drawing.Point(7, 180);
			this.customFileNamingGb.Name = "customFileNamingGb";
			this.customFileNamingGb.Size = new System.Drawing.Size(888, 157);
			this.customFileNamingGb.TabIndex = 20;
			this.customFileNamingGb.TabStop = false;
			this.customFileNamingGb.Text = "Custom file naming";
			// 
			// chapterFileTemplateBtn
			// 
			this.chapterFileTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chapterFileTemplateBtn.Location = new System.Drawing.Point(808, 124);
			this.chapterFileTemplateBtn.Name = "chapterFileTemplateBtn";
			this.chapterFileTemplateBtn.Size = new System.Drawing.Size(75, 23);
			this.chapterFileTemplateBtn.TabIndex = 8;
			this.chapterFileTemplateBtn.Text = "Edit...";
			this.chapterFileTemplateBtn.UseVisualStyleBackColor = true;
			this.chapterFileTemplateBtn.Click += new System.EventHandler(this.chapterFileTemplateBtn_Click);
			// 
			// chapterFileTemplateTb
			// 
			this.chapterFileTemplateTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chapterFileTemplateTb.Location = new System.Drawing.Point(6, 125);
			this.chapterFileTemplateTb.Name = "chapterFileTemplateTb";
			this.chapterFileTemplateTb.ReadOnly = true;
			this.chapterFileTemplateTb.Size = new System.Drawing.Size(796, 23);
			this.chapterFileTemplateTb.TabIndex = 7;
			// 
			// chapterFileTemplateLbl
			// 
			this.chapterFileTemplateLbl.AutoSize = true;
			this.chapterFileTemplateLbl.Location = new System.Drawing.Point(6, 107);
			this.chapterFileTemplateLbl.Name = "chapterFileTemplateLbl";
			this.chapterFileTemplateLbl.Size = new System.Drawing.Size(123, 15);
			this.chapterFileTemplateLbl.TabIndex = 6;
			this.chapterFileTemplateLbl.Text = "[folder template desc]";
			// 
			// fileTemplateBtn
			// 
			this.fileTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fileTemplateBtn.Location = new System.Drawing.Point(808, 80);
			this.fileTemplateBtn.Name = "fileTemplateBtn";
			this.fileTemplateBtn.Size = new System.Drawing.Size(75, 23);
			this.fileTemplateBtn.TabIndex = 5;
			this.fileTemplateBtn.Text = "Edit...";
			this.fileTemplateBtn.UseVisualStyleBackColor = true;
			this.fileTemplateBtn.Click += new System.EventHandler(this.fileTemplateBtn_Click);
			// 
			// fileTemplateTb
			// 
			this.fileTemplateTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fileTemplateTb.Location = new System.Drawing.Point(6, 81);
			this.fileTemplateTb.Name = "fileTemplateTb";
			this.fileTemplateTb.ReadOnly = true;
			this.fileTemplateTb.Size = new System.Drawing.Size(796, 23);
			this.fileTemplateTb.TabIndex = 4;
			// 
			// fileTemplateLbl
			// 
			this.fileTemplateLbl.AutoSize = true;
			this.fileTemplateLbl.Location = new System.Drawing.Point(6, 63);
			this.fileTemplateLbl.Name = "fileTemplateLbl";
			this.fileTemplateLbl.Size = new System.Drawing.Size(123, 15);
			this.fileTemplateLbl.TabIndex = 3;
			this.fileTemplateLbl.Text = "[folder template desc]";
			// 
			// folderTemplateBtn
			// 
			this.folderTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.folderTemplateBtn.Location = new System.Drawing.Point(807, 36);
			this.folderTemplateBtn.Name = "folderTemplateBtn";
			this.folderTemplateBtn.Size = new System.Drawing.Size(75, 23);
			this.folderTemplateBtn.TabIndex = 2;
			this.folderTemplateBtn.Text = "Edit...";
			this.folderTemplateBtn.UseVisualStyleBackColor = true;
			this.folderTemplateBtn.Click += new System.EventHandler(this.folderTemplateBtn_Click);
			// 
			// folderTemplateTb
			// 
			this.folderTemplateTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.folderTemplateTb.Location = new System.Drawing.Point(5, 37);
			this.folderTemplateTb.Name = "folderTemplateTb";
			this.folderTemplateTb.ReadOnly = true;
			this.folderTemplateTb.Size = new System.Drawing.Size(796, 23);
			this.folderTemplateTb.TabIndex = 1;
			// 
			// folderTemplateLbl
			// 
			this.folderTemplateLbl.AutoSize = true;
			this.folderTemplateLbl.Location = new System.Drawing.Point(5, 19);
			this.folderTemplateLbl.Name = "folderTemplateLbl";
			this.folderTemplateLbl.Size = new System.Drawing.Size(123, 15);
			this.folderTemplateLbl.TabIndex = 0;
			this.folderTemplateLbl.Text = "[folder template desc]";
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(933, 566);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "SettingsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Settings";
			this.Load += new System.EventHandler(this.SettingsDialog_Load);
			this.badBookGb.ResumeLayout(false);
			this.badBookGb.PerformLayout();
			this.decryptAndConvertGb.ResumeLayout(false);
			this.decryptAndConvertGb.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tab1ImportantSettings.ResumeLayout(false);
			this.tab1ImportantSettings.PerformLayout();
			this.booksGb.ResumeLayout(false);
			this.booksGb.PerformLayout();
			this.tab2ImportLibrary.ResumeLayout(false);
			this.tab2ImportLibrary.PerformLayout();
			this.tab3DownloadDecrypt.ResumeLayout(false);
			this.inProgressFilesGb.ResumeLayout(false);
			this.inProgressFilesGb.PerformLayout();
			this.customFileNamingGb.ResumeLayout(false);
			this.customFileNamingGb.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label booksLocationDescLbl;
		private System.Windows.Forms.Label inProgressDescLbl;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.CheckBox allowLibationFixupCbox;
		private DirectoryOrCustomSelectControl booksSelectControl;
		private DirectorySelectControl inProgressSelectControl;
        private System.Windows.Forms.RadioButton convertLossyRb;
        private System.Windows.Forms.RadioButton convertLosslessRb;
		private System.Windows.Forms.Button logsBtn;
		private System.Windows.Forms.Label loggingLevelLbl;
		private System.Windows.Forms.ComboBox loggingLevelCb;
		private System.Windows.Forms.GroupBox decryptAndConvertGb;
		private System.Windows.Forms.GroupBox badBookGb;
		private System.Windows.Forms.RadioButton badBookRetryRb;
		private System.Windows.Forms.RadioButton badBookAbortRb;
		private System.Windows.Forms.RadioButton badBookAskRb;
		private System.Windows.Forms.RadioButton badBookIgnoreRb;
		private System.Windows.Forms.CheckBox downloadEpisodesCb;
		private System.Windows.Forms.CheckBox importEpisodesCb;
        private System.Windows.Forms.CheckBox splitFilesByChapterCbox;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tab1ImportantSettings;
		private System.Windows.Forms.GroupBox booksGb;
		private System.Windows.Forms.TabPage tab2ImportLibrary;
		private System.Windows.Forms.TabPage tab3DownloadDecrypt;
		private System.Windows.Forms.GroupBox inProgressFilesGb;
		private System.Windows.Forms.GroupBox customFileNamingGb;
		private System.Windows.Forms.Button chapterFileTemplateBtn;
		private System.Windows.Forms.TextBox chapterFileTemplateTb;
		private System.Windows.Forms.Label chapterFileTemplateLbl;
		private System.Windows.Forms.Button fileTemplateBtn;
		private System.Windows.Forms.TextBox fileTemplateTb;
		private System.Windows.Forms.Label fileTemplateLbl;
		private System.Windows.Forms.Button folderTemplateBtn;
		private System.Windows.Forms.TextBox folderTemplateTb;
		private System.Windows.Forms.Label folderTemplateLbl;
        private System.Windows.Forms.CheckBox showImportedStatsCb;
		private System.Windows.Forms.CheckBox stripAudibleBrandingCbox;
	}
}