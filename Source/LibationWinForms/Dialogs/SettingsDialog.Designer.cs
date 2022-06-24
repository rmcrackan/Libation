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
			this.saveEpisodesToSeriesFolderCbox = new System.Windows.Forms.CheckBox();
			this.tab2ImportLibrary = new System.Windows.Forms.TabPage();
			this.autoDownloadEpisodesCb = new System.Windows.Forms.CheckBox();
			this.autoScanCb = new System.Windows.Forms.CheckBox();
			this.showImportedStatsCb = new System.Windows.Forms.CheckBox();
			this.tab3DownloadDecrypt = new System.Windows.Forms.TabPage();
			this.inProgressFilesGb = new System.Windows.Forms.GroupBox();
			this.customFileNamingGb = new System.Windows.Forms.GroupBox();
			this.editCharreplacementBtn = new System.Windows.Forms.Button();
			this.chapterFileTemplateBtn = new System.Windows.Forms.Button();
			this.chapterFileTemplateTb = new System.Windows.Forms.TextBox();
			this.chapterFileTemplateLbl = new System.Windows.Forms.Label();
			this.fileTemplateBtn = new System.Windows.Forms.Button();
			this.fileTemplateTb = new System.Windows.Forms.TextBox();
			this.fileTemplateLbl = new System.Windows.Forms.Label();
			this.folderTemplateBtn = new System.Windows.Forms.Button();
			this.folderTemplateTb = new System.Windows.Forms.TextBox();
			this.folderTemplateLbl = new System.Windows.Forms.Label();
			this.tab4AudioFileOptions = new System.Windows.Forms.TabPage();
			this.chapterTitleTemplateGb = new System.Windows.Forms.GroupBox();
			this.chapterTitleTemplateBtn = new System.Windows.Forms.Button();
			this.chapterTitleTemplateTb = new System.Windows.Forms.TextBox();
			this.lameOptionsGb = new System.Windows.Forms.GroupBox();
			this.lameDownsampleMonoCbox = new System.Windows.Forms.CheckBox();
			this.lameBitrateGb = new System.Windows.Forms.GroupBox();
			this.LameMatchSourceBRCbox = new System.Windows.Forms.CheckBox();
			this.lameConstantBitrateCbox = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lameBitrateTb = new System.Windows.Forms.TrackBar();
			this.label1 = new System.Windows.Forms.Label();
			this.lameQualityGb = new System.Windows.Forms.GroupBox();
			this.label19 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lameVBRQualityTb = new System.Windows.Forms.TrackBar();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lameTargetQualityRb = new System.Windows.Forms.RadioButton();
			this.lameTargetBitrateRb = new System.Windows.Forms.RadioButton();
			this.stripUnabridgedCbox = new System.Windows.Forms.CheckBox();
			this.retainAaxFileCbox = new System.Windows.Forms.CheckBox();
			this.downloadCoverArtCbox = new System.Windows.Forms.CheckBox();
			this.createCueSheetCbox = new System.Windows.Forms.CheckBox();
			this.badBookGb.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tab1ImportantSettings.SuspendLayout();
			this.booksGb.SuspendLayout();
			this.tab2ImportLibrary.SuspendLayout();
			this.tab3DownloadDecrypt.SuspendLayout();
			this.inProgressFilesGb.SuspendLayout();
			this.customFileNamingGb.SuspendLayout();
			this.tab4AudioFileOptions.SuspendLayout();
			this.chapterTitleTemplateGb.SuspendLayout();
			this.lameOptionsGb.SuspendLayout();
			this.lameBitrateGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lameBitrateTb)).BeginInit();
			this.lameQualityGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lameVBRQualityTb)).BeginInit();
			this.groupBox2.SuspendLayout();
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
			this.saveBtn.Location = new System.Drawing.Point(667, 461);
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
			this.cancelBtn.Location = new System.Drawing.Point(785, 461);
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
			this.importEpisodesCb.Location = new System.Drawing.Point(6, 56);
			this.importEpisodesCb.Name = "importEpisodesCb";
			this.importEpisodesCb.Size = new System.Drawing.Size(146, 19);
			this.importEpisodesCb.TabIndex = 3;
			this.importEpisodesCb.Text = "[import episodes desc]";
			this.importEpisodesCb.UseVisualStyleBackColor = true;
			// 
			// downloadEpisodesCb
			// 
			this.downloadEpisodesCb.AutoSize = true;
			this.downloadEpisodesCb.Location = new System.Drawing.Point(6, 81);
			this.downloadEpisodesCb.Name = "downloadEpisodesCb";
			this.downloadEpisodesCb.Size = new System.Drawing.Size(163, 19);
			this.downloadEpisodesCb.TabIndex = 4;
			this.downloadEpisodesCb.Text = "[download episodes desc]";
			this.downloadEpisodesCb.UseVisualStyleBackColor = true;
			// 
			// badBookGb
			// 
			this.badBookGb.Controls.Add(this.badBookIgnoreRb);
			this.badBookGb.Controls.Add(this.badBookRetryRb);
			this.badBookGb.Controls.Add(this.badBookAbortRb);
			this.badBookGb.Controls.Add(this.badBookAskRb);
			this.badBookGb.Location = new System.Drawing.Point(7, 6);
			this.badBookGb.Name = "badBookGb";
			this.badBookGb.Size = new System.Drawing.Size(888, 76);
			this.badBookGb.TabIndex = 13;
			this.badBookGb.TabStop = false;
			this.badBookGb.Text = "[bad book desc]";
			// 
			// badBookIgnoreRb
			// 
			this.badBookIgnoreRb.AutoSize = true;
			this.badBookIgnoreRb.Location = new System.Drawing.Point(384, 47);
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
			this.badBookRetryRb.Location = new System.Drawing.Point(5, 47);
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
			this.badBookAbortRb.Location = new System.Drawing.Point(384, 22);
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
			// stripAudibleBrandingCbox
			// 
			this.stripAudibleBrandingCbox.AutoSize = true;
			this.stripAudibleBrandingCbox.Location = new System.Drawing.Point(19, 168);
			this.stripAudibleBrandingCbox.Name = "stripAudibleBrandingCbox";
			this.stripAudibleBrandingCbox.Size = new System.Drawing.Size(143, 34);
			this.stripAudibleBrandingCbox.TabIndex = 13;
			this.stripAudibleBrandingCbox.Text = "[StripAudibleBranding\r\ndesc]";
			this.stripAudibleBrandingCbox.UseVisualStyleBackColor = true;
			// 
			// splitFilesByChapterCbox
			// 
			this.splitFilesByChapterCbox.AutoSize = true;
			this.splitFilesByChapterCbox.Location = new System.Drawing.Point(19, 118);
			this.splitFilesByChapterCbox.Name = "splitFilesByChapterCbox";
			this.splitFilesByChapterCbox.Size = new System.Drawing.Size(162, 19);
			this.splitFilesByChapterCbox.TabIndex = 13;
			this.splitFilesByChapterCbox.Text = "[SplitFilesByChapter desc]";
			this.splitFilesByChapterCbox.UseVisualStyleBackColor = true;
			this.splitFilesByChapterCbox.CheckedChanged += new System.EventHandler(this.splitFilesByChapterCbox_CheckedChanged);
			// 
			// allowLibationFixupCbox
			// 
			this.allowLibationFixupCbox.AutoSize = true;
			this.allowLibationFixupCbox.Checked = true;
			this.allowLibationFixupCbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.allowLibationFixupCbox.Location = new System.Drawing.Point(19, 18);
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
			this.convertLossyRb.Location = new System.Drawing.Point(19, 232);
			this.convertLossyRb.Name = "convertLossyRb";
			this.convertLossyRb.Size = new System.Drawing.Size(329, 19);
			this.convertLossyRb.TabIndex = 12;
			this.convertLossyRb.Text = "Download my books as .MP3 files (transcode if necessary)";
			this.convertLossyRb.UseVisualStyleBackColor = true;
			this.convertLossyRb.CheckedChanged += new System.EventHandler(this.convertFormatRb_CheckedChanged);
			// 
			// convertLosslessRb
			// 
			this.convertLosslessRb.AutoSize = true;
			this.convertLosslessRb.Checked = true;
			this.convertLosslessRb.Location = new System.Drawing.Point(19, 207);
			this.convertLosslessRb.Name = "convertLosslessRb";
			this.convertLosslessRb.Size = new System.Drawing.Size(335, 19);
			this.convertLosslessRb.TabIndex = 11;
			this.convertLosslessRb.TabStop = true;
			this.convertLosslessRb.Text = "Download my books in the original audio format (Lossless)";
			this.convertLosslessRb.UseVisualStyleBackColor = true;
			this.convertLosslessRb.CheckedChanged += new System.EventHandler(this.convertFormatRb_CheckedChanged);
			// 
			// inProgressSelectControl
			// 
			this.inProgressSelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inProgressSelectControl.Location = new System.Drawing.Point(7, 68);
			this.inProgressSelectControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.inProgressSelectControl.Name = "inProgressSelectControl";
			this.inProgressSelectControl.Size = new System.Drawing.Size(828, 52);
			this.inProgressSelectControl.TabIndex = 19;
			// 
			// logsBtn
			// 
			this.logsBtn.Location = new System.Drawing.Point(256, 198);
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
			this.booksSelectControl.Size = new System.Drawing.Size(829, 87);
			this.booksSelectControl.TabIndex = 2;
			// 
			// loggingLevelLbl
			// 
			this.loggingLevelLbl.AutoSize = true;
			this.loggingLevelLbl.Location = new System.Drawing.Point(6, 201);
			this.loggingLevelLbl.Name = "loggingLevelLbl";
			this.loggingLevelLbl.Size = new System.Drawing.Size(78, 15);
			this.loggingLevelLbl.TabIndex = 3;
			this.loggingLevelLbl.Text = "Logging level";
			// 
			// loggingLevelCb
			// 
			this.loggingLevelCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.loggingLevelCb.FormattingEnabled = true;
			this.loggingLevelCb.Location = new System.Drawing.Point(90, 198);
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
			this.tabControl.Controls.Add(this.tab4AudioFileOptions);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(862, 443);
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
			this.tab1ImportantSettings.Size = new System.Drawing.Size(854, 415);
			this.tab1ImportantSettings.TabIndex = 0;
			this.tab1ImportantSettings.Text = "Important settings";
			this.tab1ImportantSettings.UseVisualStyleBackColor = true;
			// 
			// booksGb
			// 
			this.booksGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksGb.Controls.Add(this.saveEpisodesToSeriesFolderCbox);
			this.booksGb.Controls.Add(this.booksSelectControl);
			this.booksGb.Controls.Add(this.booksLocationDescLbl);
			this.booksGb.Location = new System.Drawing.Point(6, 6);
			this.booksGb.Name = "booksGb";
			this.booksGb.Size = new System.Drawing.Size(842, 156);
			this.booksGb.TabIndex = 0;
			this.booksGb.TabStop = false;
			this.booksGb.Text = "Books location";
			// 
			// saveEpisodesToSeriesFolderCbox
			// 
			this.saveEpisodesToSeriesFolderCbox.AutoSize = true;
			this.saveEpisodesToSeriesFolderCbox.Location = new System.Drawing.Point(7, 131);
			this.saveEpisodesToSeriesFolderCbox.Name = "saveEpisodesToSeriesFolderCbox";
			this.saveEpisodesToSeriesFolderCbox.Size = new System.Drawing.Size(191, 19);
			this.saveEpisodesToSeriesFolderCbox.TabIndex = 3;
			this.saveEpisodesToSeriesFolderCbox.Text = "[Save Episodes To Series Folder]";
			this.saveEpisodesToSeriesFolderCbox.UseVisualStyleBackColor = true;
			// 
			// tab2ImportLibrary
			// 
			this.tab2ImportLibrary.Controls.Add(this.autoDownloadEpisodesCb);
			this.tab2ImportLibrary.Controls.Add(this.autoScanCb);
			this.tab2ImportLibrary.Controls.Add(this.showImportedStatsCb);
			this.tab2ImportLibrary.Controls.Add(this.importEpisodesCb);
			this.tab2ImportLibrary.Controls.Add(this.downloadEpisodesCb);
			this.tab2ImportLibrary.Location = new System.Drawing.Point(4, 24);
			this.tab2ImportLibrary.Name = "tab2ImportLibrary";
			this.tab2ImportLibrary.Padding = new System.Windows.Forms.Padding(3);
			this.tab2ImportLibrary.Size = new System.Drawing.Size(854, 415);
			this.tab2ImportLibrary.TabIndex = 1;
			this.tab2ImportLibrary.Text = "Import library";
			this.tab2ImportLibrary.UseVisualStyleBackColor = true;
			// 
			// autoDownloadEpisodesCb
			// 
			this.autoDownloadEpisodesCb.AutoSize = true;
			this.autoDownloadEpisodesCb.Location = new System.Drawing.Point(6, 106);
			this.autoDownloadEpisodesCb.Name = "autoDownloadEpisodesCb";
			this.autoDownloadEpisodesCb.Size = new System.Drawing.Size(190, 19);
			this.autoDownloadEpisodesCb.TabIndex = 5;
			this.autoDownloadEpisodesCb.Text = "[auto download episodes desc]";
			this.autoDownloadEpisodesCb.UseVisualStyleBackColor = true;
			// 
			// autoScanCb
			// 
			this.autoScanCb.AutoSize = true;
			this.autoScanCb.Location = new System.Drawing.Point(6, 6);
			this.autoScanCb.Name = "autoScanCb";
			this.autoScanCb.Size = new System.Drawing.Size(112, 19);
			this.autoScanCb.TabIndex = 1;
			this.autoScanCb.Text = "[auto scan desc]";
			this.autoScanCb.UseVisualStyleBackColor = true;
			// 
			// showImportedStatsCb
			// 
			this.showImportedStatsCb.AutoSize = true;
			this.showImportedStatsCb.Location = new System.Drawing.Point(6, 31);
			this.showImportedStatsCb.Name = "showImportedStatsCb";
			this.showImportedStatsCb.Size = new System.Drawing.Size(168, 19);
			this.showImportedStatsCb.TabIndex = 2;
			this.showImportedStatsCb.Text = "[show imported stats desc]";
			this.showImportedStatsCb.UseVisualStyleBackColor = true;
			// 
			// tab3DownloadDecrypt
			// 
			this.tab3DownloadDecrypt.Controls.Add(this.inProgressFilesGb);
			this.tab3DownloadDecrypt.Controls.Add(this.customFileNamingGb);
			this.tab3DownloadDecrypt.Controls.Add(this.badBookGb);
			this.tab3DownloadDecrypt.Location = new System.Drawing.Point(4, 24);
			this.tab3DownloadDecrypt.Name = "tab3DownloadDecrypt";
			this.tab3DownloadDecrypt.Padding = new System.Windows.Forms.Padding(3);
			this.tab3DownloadDecrypt.Size = new System.Drawing.Size(854, 415);
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
			this.inProgressFilesGb.Location = new System.Drawing.Point(6, 281);
			this.inProgressFilesGb.Name = "inProgressFilesGb";
			this.inProgressFilesGb.Size = new System.Drawing.Size(841, 128);
			this.inProgressFilesGb.TabIndex = 21;
			this.inProgressFilesGb.TabStop = false;
			this.inProgressFilesGb.Text = "In progress files";
			// 
			// customFileNamingGb
			// 
			this.customFileNamingGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.customFileNamingGb.Controls.Add(this.editCharreplacementBtn);
			this.customFileNamingGb.Controls.Add(this.chapterFileTemplateBtn);
			this.customFileNamingGb.Controls.Add(this.chapterFileTemplateTb);
			this.customFileNamingGb.Controls.Add(this.chapterFileTemplateLbl);
			this.customFileNamingGb.Controls.Add(this.fileTemplateBtn);
			this.customFileNamingGb.Controls.Add(this.fileTemplateTb);
			this.customFileNamingGb.Controls.Add(this.fileTemplateLbl);
			this.customFileNamingGb.Controls.Add(this.folderTemplateBtn);
			this.customFileNamingGb.Controls.Add(this.folderTemplateTb);
			this.customFileNamingGb.Controls.Add(this.folderTemplateLbl);
			this.customFileNamingGb.Location = new System.Drawing.Point(7, 88);
			this.customFileNamingGb.Name = "customFileNamingGb";
			this.customFileNamingGb.Size = new System.Drawing.Size(841, 187);
			this.customFileNamingGb.TabIndex = 20;
			this.customFileNamingGb.TabStop = false;
			this.customFileNamingGb.Text = "Custom file naming";
			// 
			// editCharreplacementBtn
			// 
			this.editCharreplacementBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.editCharreplacementBtn.Location = new System.Drawing.Point(5, 158);
			this.editCharreplacementBtn.Name = "editCharreplacementBtn";
			this.editCharreplacementBtn.Size = new System.Drawing.Size(387, 23);
			this.editCharreplacementBtn.TabIndex = 8;
			this.editCharreplacementBtn.Text = "[edit char replacement desc]";
			this.editCharreplacementBtn.UseVisualStyleBackColor = true;
			this.editCharreplacementBtn.Click += new System.EventHandler(this.editCharreplacementBtn_Click);
			// 
			// chapterFileTemplateBtn
			// 
			this.chapterFileTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chapterFileTemplateBtn.Location = new System.Drawing.Point(761, 124);
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
			this.chapterFileTemplateTb.Size = new System.Drawing.Size(749, 23);
			this.chapterFileTemplateTb.TabIndex = 7;
			// 
			// chapterFileTemplateLbl
			// 
			this.chapterFileTemplateLbl.AutoSize = true;
			this.chapterFileTemplateLbl.Location = new System.Drawing.Point(6, 107);
			this.chapterFileTemplateLbl.Name = "chapterFileTemplateLbl";
			this.chapterFileTemplateLbl.Size = new System.Drawing.Size(132, 15);
			this.chapterFileTemplateLbl.TabIndex = 6;
			this.chapterFileTemplateLbl.Text = "[chapter template desc]";
			// 
			// fileTemplateBtn
			// 
			this.fileTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fileTemplateBtn.Location = new System.Drawing.Point(761, 80);
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
			this.fileTemplateTb.Size = new System.Drawing.Size(749, 23);
			this.fileTemplateTb.TabIndex = 4;
			// 
			// fileTemplateLbl
			// 
			this.fileTemplateLbl.AutoSize = true;
			this.fileTemplateLbl.Location = new System.Drawing.Point(6, 63);
			this.fileTemplateLbl.Name = "fileTemplateLbl";
			this.fileTemplateLbl.Size = new System.Drawing.Size(108, 15);
			this.fileTemplateLbl.TabIndex = 3;
			this.fileTemplateLbl.Text = "[file template desc]";
			// 
			// folderTemplateBtn
			// 
			this.folderTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.folderTemplateBtn.Location = new System.Drawing.Point(760, 36);
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
			this.folderTemplateTb.Size = new System.Drawing.Size(749, 23);
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
			// tab4AudioFileOptions
			// 
			this.tab4AudioFileOptions.Controls.Add(this.chapterTitleTemplateGb);
			this.tab4AudioFileOptions.Controls.Add(this.lameOptionsGb);
			this.tab4AudioFileOptions.Controls.Add(this.convertLossyRb);
			this.tab4AudioFileOptions.Controls.Add(this.stripAudibleBrandingCbox);
			this.tab4AudioFileOptions.Controls.Add(this.convertLosslessRb);
			this.tab4AudioFileOptions.Controls.Add(this.stripUnabridgedCbox);
			this.tab4AudioFileOptions.Controls.Add(this.splitFilesByChapterCbox);
			this.tab4AudioFileOptions.Controls.Add(this.retainAaxFileCbox);
			this.tab4AudioFileOptions.Controls.Add(this.downloadCoverArtCbox);
			this.tab4AudioFileOptions.Controls.Add(this.createCueSheetCbox);
			this.tab4AudioFileOptions.Controls.Add(this.allowLibationFixupCbox);
			this.tab4AudioFileOptions.Location = new System.Drawing.Point(4, 24);
			this.tab4AudioFileOptions.Name = "tab4AudioFileOptions";
			this.tab4AudioFileOptions.Padding = new System.Windows.Forms.Padding(3);
			this.tab4AudioFileOptions.Size = new System.Drawing.Size(854, 415);
			this.tab4AudioFileOptions.TabIndex = 3;
			this.tab4AudioFileOptions.Text = "Audio File Options";
			this.tab4AudioFileOptions.UseVisualStyleBackColor = true;
			// 
			// chapterTitleTemplateGb
			// 
			this.chapterTitleTemplateGb.Controls.Add(this.chapterTitleTemplateBtn);
			this.chapterTitleTemplateGb.Controls.Add(this.chapterTitleTemplateTb);
			this.chapterTitleTemplateGb.Location = new System.Drawing.Point(6, 335);
			this.chapterTitleTemplateGb.Name = "chapterTitleTemplateGb";
			this.chapterTitleTemplateGb.Size = new System.Drawing.Size(842, 54);
			this.chapterTitleTemplateGb.TabIndex = 18;
			this.chapterTitleTemplateGb.TabStop = false;
			this.chapterTitleTemplateGb.Text = "[chapter title template desc]";
			// 
			// chapterTitleTemplateBtn
			// 
			this.chapterTitleTemplateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chapterTitleTemplateBtn.Location = new System.Drawing.Point(764, 22);
			this.chapterTitleTemplateBtn.Name = "chapterTitleTemplateBtn";
			this.chapterTitleTemplateBtn.Size = new System.Drawing.Size(75, 23);
			this.chapterTitleTemplateBtn.TabIndex = 17;
			this.chapterTitleTemplateBtn.Text = "Edit...";
			this.chapterTitleTemplateBtn.UseVisualStyleBackColor = true;
			this.chapterTitleTemplateBtn.Click += new System.EventHandler(this.chapterTitleTemplateBtn_Click);
			// 
			// chapterTitleTemplateTb
			// 
			this.chapterTitleTemplateTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chapterTitleTemplateTb.Location = new System.Drawing.Point(6, 22);
			this.chapterTitleTemplateTb.Name = "chapterTitleTemplateTb";
			this.chapterTitleTemplateTb.ReadOnly = true;
			this.chapterTitleTemplateTb.Size = new System.Drawing.Size(752, 23);
			this.chapterTitleTemplateTb.TabIndex = 16;
			// 
			// lameOptionsGb
			// 
			this.lameOptionsGb.Controls.Add(this.lameDownsampleMonoCbox);
			this.lameOptionsGb.Controls.Add(this.lameBitrateGb);
			this.lameOptionsGb.Controls.Add(this.label1);
			this.lameOptionsGb.Controls.Add(this.lameQualityGb);
			this.lameOptionsGb.Controls.Add(this.groupBox2);
			this.lameOptionsGb.Location = new System.Drawing.Point(415, 6);
			this.lameOptionsGb.Name = "lameOptionsGb";
			this.lameOptionsGb.Size = new System.Drawing.Size(433, 323);
			this.lameOptionsGb.TabIndex = 14;
			this.lameOptionsGb.TabStop = false;
			this.lameOptionsGb.Text = "Mp3 Encoding Options";
			// 
			// lameDownsampleMonoCbox
			// 
			this.lameDownsampleMonoCbox.AutoSize = true;
			this.lameDownsampleMonoCbox.Location = new System.Drawing.Point(234, 35);
			this.lameDownsampleMonoCbox.Name = "lameDownsampleMonoCbox";
			this.lameDownsampleMonoCbox.Size = new System.Drawing.Size(184, 34);
			this.lameDownsampleMonoCbox.TabIndex = 1;
			this.lameDownsampleMonoCbox.Text = "Downsample stereo to mono?\r\n(Recommended)\r\n";
			this.lameDownsampleMonoCbox.UseVisualStyleBackColor = true;
			// 
			// lameBitrateGb
			// 
			this.lameBitrateGb.Controls.Add(this.LameMatchSourceBRCbox);
			this.lameBitrateGb.Controls.Add(this.lameConstantBitrateCbox);
			this.lameBitrateGb.Controls.Add(this.label7);
			this.lameBitrateGb.Controls.Add(this.label6);
			this.lameBitrateGb.Controls.Add(this.label5);
			this.lameBitrateGb.Controls.Add(this.label4);
			this.lameBitrateGb.Controls.Add(this.label11);
			this.lameBitrateGb.Controls.Add(this.label3);
			this.lameBitrateGb.Controls.Add(this.lameBitrateTb);
			this.lameBitrateGb.Location = new System.Drawing.Point(6, 84);
			this.lameBitrateGb.Name = "lameBitrateGb";
			this.lameBitrateGb.Size = new System.Drawing.Size(421, 101);
			this.lameBitrateGb.TabIndex = 0;
			this.lameBitrateGb.TabStop = false;
			this.lameBitrateGb.Text = "Bitrate";
			// 
			// LameMatchSourceBRCbox
			// 
			this.LameMatchSourceBRCbox.AutoSize = true;
			this.LameMatchSourceBRCbox.Location = new System.Drawing.Point(260, 77);
			this.LameMatchSourceBRCbox.Name = "LameMatchSourceBRCbox";
			this.LameMatchSourceBRCbox.Size = new System.Drawing.Size(140, 19);
			this.LameMatchSourceBRCbox.TabIndex = 3;
			this.LameMatchSourceBRCbox.Text = "Match source bitrate?";
			this.LameMatchSourceBRCbox.UseVisualStyleBackColor = true;
			this.LameMatchSourceBRCbox.CheckedChanged += new System.EventHandler(this.LameMatchSourceBRCbox_CheckedChanged);
			// 
			// lameConstantBitrateCbox
			// 
			this.lameConstantBitrateCbox.AutoSize = true;
			this.lameConstantBitrateCbox.Location = new System.Drawing.Point(6, 77);
			this.lameConstantBitrateCbox.Name = "lameConstantBitrateCbox";
			this.lameConstantBitrateCbox.Size = new System.Drawing.Size(216, 19);
			this.lameConstantBitrateCbox.TabIndex = 2;
			this.lameConstantBitrateCbox.Text = "Restrict encoder to constant bitrate?";
			this.lameConstantBitrateCbox.UseVisualStyleBackColor = true;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label7.Location = new System.Drawing.Point(390, 52);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(25, 15);
			this.label7.TabIndex = 1;
			this.label7.Text = "320";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label6.Location = new System.Drawing.Point(309, 52);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(25, 15);
			this.label6.TabIndex = 1;
			this.label6.Text = "256";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label5.Location = new System.Drawing.Point(228, 52);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(25, 15);
			this.label5.TabIndex = 1;
			this.label5.Text = "192";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label4.Location = new System.Drawing.Point(147, 52);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(25, 15);
			this.label4.TabIndex = 1;
			this.label4.Text = "128";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label11.Location = new System.Drawing.Point(10, 52);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(19, 15);
			this.label11.TabIndex = 1;
			this.label11.Text = "16";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.label3.Location = new System.Drawing.Point(71, 52);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(19, 15);
			this.label3.TabIndex = 1;
			this.label3.Text = "64";
			// 
			// lameBitrateTb
			// 
			this.lameBitrateTb.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.lameBitrateTb.LargeChange = 32;
			this.lameBitrateTb.Location = new System.Drawing.Point(6, 22);
			this.lameBitrateTb.Maximum = 320;
			this.lameBitrateTb.Minimum = 16;
			this.lameBitrateTb.Name = "lameBitrateTb";
			this.lameBitrateTb.Size = new System.Drawing.Size(409, 45);
			this.lameBitrateTb.SmallChange = 8;
			this.lameBitrateTb.TabIndex = 0;
			this.lameBitrateTb.TickFrequency = 16;
			this.lameBitrateTb.Value = 64;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Enabled = false;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(6, 298);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(172, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Using L.A.M.E. encoding engine";
			// 
			// lameQualityGb
			// 
			this.lameQualityGb.Controls.Add(this.label19);
			this.lameQualityGb.Controls.Add(this.label18);
			this.lameQualityGb.Controls.Add(this.label17);
			this.lameQualityGb.Controls.Add(this.label16);
			this.lameQualityGb.Controls.Add(this.label12);
			this.lameQualityGb.Controls.Add(this.label15);
			this.lameQualityGb.Controls.Add(this.label9);
			this.lameQualityGb.Controls.Add(this.label8);
			this.lameQualityGb.Controls.Add(this.label13);
			this.lameQualityGb.Controls.Add(this.label10);
			this.lameQualityGb.Controls.Add(this.label14);
			this.lameQualityGb.Controls.Add(this.label2);
			this.lameQualityGb.Controls.Add(this.lameVBRQualityTb);
			this.lameQualityGb.Location = new System.Drawing.Point(6, 186);
			this.lameQualityGb.Name = "lameQualityGb";
			this.lameQualityGb.Size = new System.Drawing.Size(421, 109);
			this.lameQualityGb.TabIndex = 0;
			this.lameQualityGb.TabStop = false;
			this.lameQualityGb.Text = "Quality";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(349, 52);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(20, 15);
			this.label19.TabIndex = 1;
			this.label19.Text = "V8";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(307, 52);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(20, 15);
			this.label18.TabIndex = 1;
			this.label18.Text = "V7";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(265, 52);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(20, 15);
			this.label17.TabIndex = 1;
			this.label17.Text = "V6";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(223, 52);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(20, 15);
			this.label16.TabIndex = 1;
			this.label16.Text = "V5";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(182, 52);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(20, 15);
			this.label12.TabIndex = 1;
			this.label12.Text = "V4";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(140, 52);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(20, 15);
			this.label15.TabIndex = 1;
			this.label15.Text = "V3";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(97, 52);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(20, 15);
			this.label9.TabIndex = 1;
			this.label9.Text = "V2";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(391, 52);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(20, 15);
			this.label8.TabIndex = 1;
			this.label8.Text = "V9";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(376, 81);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(39, 15);
			this.label13.TabIndex = 1;
			this.label13.Text = "Lower";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 81);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(43, 15);
			this.label10.TabIndex = 1;
			this.label10.Text = "Higher";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(56, 52);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(20, 15);
			this.label14.TabIndex = 1;
			this.label14.Text = "V1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 52);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(20, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "V0";
			// 
			// lameVBRQualityTb
			// 
			this.lameVBRQualityTb.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.lameVBRQualityTb.LargeChange = 1;
			this.lameVBRQualityTb.Location = new System.Drawing.Point(10, 22);
			this.lameVBRQualityTb.Maximum = 9;
			this.lameVBRQualityTb.Name = "lameVBRQualityTb";
			this.lameVBRQualityTb.Size = new System.Drawing.Size(405, 45);
			this.lameVBRQualityTb.TabIndex = 0;
			this.lameVBRQualityTb.Value = 9;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lameTargetQualityRb);
			this.groupBox2.Controls.Add(this.lameTargetBitrateRb);
			this.groupBox2.Location = new System.Drawing.Point(6, 22);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(222, 56);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Target";
			// 
			// lameTargetQualityRb
			// 
			this.lameTargetQualityRb.AutoSize = true;
			this.lameTargetQualityRb.Location = new System.Drawing.Point(138, 23);
			this.lameTargetQualityRb.Name = "lameTargetQualityRb";
			this.lameTargetQualityRb.Size = new System.Drawing.Size(63, 19);
			this.lameTargetQualityRb.TabIndex = 0;
			this.lameTargetQualityRb.TabStop = true;
			this.lameTargetQualityRb.Text = "Quality";
			this.lameTargetQualityRb.UseVisualStyleBackColor = true;
			this.lameTargetQualityRb.CheckedChanged += new System.EventHandler(this.lameTargetRb_CheckedChanged);
			// 
			// lameTargetBitrateRb
			// 
			this.lameTargetBitrateRb.AutoSize = true;
			this.lameTargetBitrateRb.Location = new System.Drawing.Point(6, 23);
			this.lameTargetBitrateRb.Name = "lameTargetBitrateRb";
			this.lameTargetBitrateRb.Size = new System.Drawing.Size(59, 19);
			this.lameTargetBitrateRb.TabIndex = 0;
			this.lameTargetBitrateRb.TabStop = true;
			this.lameTargetBitrateRb.Text = "Bitrate";
			this.lameTargetBitrateRb.UseVisualStyleBackColor = true;
			this.lameTargetBitrateRb.CheckedChanged += new System.EventHandler(this.lameTargetRb_CheckedChanged);
			// 
			// stripUnabridgedCbox
			// 
			this.stripUnabridgedCbox.AutoSize = true;
			this.stripUnabridgedCbox.Location = new System.Drawing.Point(19, 143);
			this.stripUnabridgedCbox.Name = "stripUnabridgedCbox";
			this.stripUnabridgedCbox.Size = new System.Drawing.Size(147, 19);
			this.stripUnabridgedCbox.TabIndex = 13;
			this.stripUnabridgedCbox.Text = "[StripUnabridged desc]";
			this.stripUnabridgedCbox.UseVisualStyleBackColor = true;
			// 
			// retainAaxFileCbox
			// 
			this.retainAaxFileCbox.AutoSize = true;
			this.retainAaxFileCbox.Location = new System.Drawing.Point(19, 93);
			this.retainAaxFileCbox.Name = "retainAaxFileCbox";
			this.retainAaxFileCbox.Size = new System.Drawing.Size(132, 19);
			this.retainAaxFileCbox.TabIndex = 10;
			this.retainAaxFileCbox.Text = "[RetainAaxFile desc]";
			this.retainAaxFileCbox.UseVisualStyleBackColor = true;
			this.retainAaxFileCbox.CheckedChanged += new System.EventHandler(this.allowLibationFixupCbox_CheckedChanged);
			// 
			// downloadCoverArtCbox
			// 
			this.downloadCoverArtCbox.AutoSize = true;
			this.downloadCoverArtCbox.Checked = true;
			this.downloadCoverArtCbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.downloadCoverArtCbox.Location = new System.Drawing.Point(19, 68);
			this.downloadCoverArtCbox.Name = "downloadCoverArtCbox";
			this.downloadCoverArtCbox.Size = new System.Drawing.Size(162, 19);
			this.downloadCoverArtCbox.TabIndex = 10;
			this.downloadCoverArtCbox.Text = "[DownloadCoverArt desc]";
			this.downloadCoverArtCbox.UseVisualStyleBackColor = true;
			this.downloadCoverArtCbox.CheckedChanged += new System.EventHandler(this.allowLibationFixupCbox_CheckedChanged);
			// 
			// createCueSheetCbox
			// 
			this.createCueSheetCbox.AutoSize = true;
			this.createCueSheetCbox.Checked = true;
			this.createCueSheetCbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.createCueSheetCbox.Location = new System.Drawing.Point(19, 43);
			this.createCueSheetCbox.Name = "createCueSheetCbox";
			this.createCueSheetCbox.Size = new System.Drawing.Size(145, 19);
			this.createCueSheetCbox.TabIndex = 10;
			this.createCueSheetCbox.Text = "[CreateCueSheet desc]";
			this.createCueSheetCbox.UseVisualStyleBackColor = true;
			this.createCueSheetCbox.CheckedChanged += new System.EventHandler(this.allowLibationFixupCbox_CheckedChanged);
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(886, 504);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Settings";
			this.Load += new System.EventHandler(this.SettingsDialog_Load);
			this.badBookGb.ResumeLayout(false);
			this.badBookGb.PerformLayout();
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
			this.tab4AudioFileOptions.ResumeLayout(false);
			this.tab4AudioFileOptions.PerformLayout();
			this.chapterTitleTemplateGb.ResumeLayout(false);
			this.chapterTitleTemplateGb.PerformLayout();
			this.lameOptionsGb.ResumeLayout(false);
			this.lameOptionsGb.PerformLayout();
			this.lameBitrateGb.ResumeLayout(false);
			this.lameBitrateGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lameBitrateTb)).EndInit();
			this.lameQualityGb.ResumeLayout(false);
			this.lameQualityGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lameVBRQualityTb)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
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
		private System.Windows.Forms.TabPage tab4AudioFileOptions;
		private System.Windows.Forms.CheckBox retainAaxFileCbox;
		private System.Windows.Forms.CheckBox stripUnabridgedCbox;
		private System.Windows.Forms.GroupBox lameOptionsGb;
		private System.Windows.Forms.CheckBox lameDownsampleMonoCbox;
		private System.Windows.Forms.GroupBox lameBitrateGb;
		private System.Windows.Forms.TrackBar lameBitrateTb;
		private System.Windows.Forms.GroupBox lameQualityGb;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton lameTargetQualityRb;
		private System.Windows.Forms.RadioButton lameTargetBitrateRb;
		private System.Windows.Forms.CheckBox lameConstantBitrateCbox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TrackBar lameVBRQualityTb;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox LameMatchSourceBRCbox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.CheckBox createCueSheetCbox;
		private System.Windows.Forms.CheckBox autoScanCb;
		private System.Windows.Forms.CheckBox downloadCoverArtCbox;
		private System.Windows.Forms.CheckBox autoDownloadEpisodesCb;
		private System.Windows.Forms.CheckBox saveEpisodesToSeriesFolderCbox;
		private System.Windows.Forms.GroupBox chapterTitleTemplateGb;
		private System.Windows.Forms.Button chapterTitleTemplateBtn;
		private System.Windows.Forms.TextBox chapterTitleTemplateTb;
		private System.Windows.Forms.Button editCharreplacementBtn;
	}
}