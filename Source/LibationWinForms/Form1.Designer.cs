namespace LibationWinForms
{
	partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.filterHelpBtn = new System.Windows.Forms.Button();
            this.filterBtn = new System.Windows.Forms.Button();
            this.filterSearchTb = new ClearableTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoScanLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noAccountsYetAddAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanLibraryOfAllAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanLibraryOfSomeAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeLibraryBooksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeSomeAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liberateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beginBookBackupsToolStripMenuItem = new LibationWinForms.FormattableToolStripMenuItem();
            this.beginPdfBackupsToolStripMenuItem = new LibationWinForms.FormattableToolStripMenuItem();
            this.convertAllM4bToMp3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liberateVisibleToolStripMenuItem_LiberateMenu = new LibationWinForms.FormattableToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quickFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstFilterIsDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editQuickFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.scanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visibleBooksToolStripMenuItem = new LibationWinForms.FormattableToolStripMenuItem();
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu = new LibationWinForms.FormattableToolStripMenuItem();
            this.replaceTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setBookDownloadedManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPdfDownloadedManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDownloadedAutoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.openTrashBinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.launchHangoverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.locateAudiobooksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.basicSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.upgradePb = new System.Windows.Forms.ToolStripProgressBar();
			this.upgradeLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.visibleCountLbl = new LibationWinForms.FormattableToolStripStatusLabel();
            this.springLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.backupsCountsLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.addQuickFilterBtn = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.productsDisplay = new LibationWinForms.GridView.ProductsDisplay();
            this.toggleQueueHideBtn = new System.Windows.Forms.Button();
            this.doneRemovingBtn = new System.Windows.Forms.Button();
            this.removeBooksBtn = new System.Windows.Forms.Button();
            this.processBookQueue1 = new LibationWinForms.ProcessQueue.ProcessQueueControl();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // filterHelpBtn
            // 
            this.filterHelpBtn.Location = new System.Drawing.Point(15, 3);
            this.filterHelpBtn.Margin = new System.Windows.Forms.Padding(15, 3, 4, 3);
            this.filterHelpBtn.Name = "filterHelpBtn";
            this.filterHelpBtn.Size = new System.Drawing.Size(26, 27);
            this.filterHelpBtn.TabIndex = 3;
            this.filterHelpBtn.Text = "?";
            this.filterHelpBtn.UseVisualStyleBackColor = true;
            this.filterHelpBtn.Click += new System.EventHandler(this.filterHelpBtn_Click);
            // 
            // filterBtn
            // 
            this.filterBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.filterBtn.Location = new System.Drawing.Point(884, 3);
            this.filterBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.filterBtn.Name = "filterBtn";
            this.filterBtn.Size = new System.Drawing.Size(88, 27);
            this.filterBtn.TabIndex = 2;
            this.filterBtn.Text = "Filter";
            this.filterBtn.UseVisualStyleBackColor = true;
            this.filterBtn.Click += new System.EventHandler(this.filterBtn_Click);
            // 
            // filterSearchTb
            // 
            this.filterSearchTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterSearchTb.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.filterSearchTb.Location = new System.Drawing.Point(195, 5);
            this.filterSearchTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.filterSearchTb.Name = "filterSearchTb";
            this.filterSearchTb.Size = new System.Drawing.Size(681, 25);
            this.filterSearchTb.TabIndex = 1;
            this.filterSearchTb.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.filterSearchTb_KeyPress);
            this.filterSearchTb.TextCleared += filterSearchTb_TextCleared;
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.liberateToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.quickFiltersToolStripMenuItem,
            this.scanningToolStripMenuItem,
            this.visibleBooksToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1025, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoScanLibraryToolStripMenuItem,
            this.noAccountsYetAddAccountToolStripMenuItem,
            this.scanLibraryToolStripMenuItem,
            this.scanLibraryOfAllAccountsToolStripMenuItem,
            this.scanLibraryOfSomeAccountsToolStripMenuItem,
            this.removeLibraryBooksToolStripMenuItem,
            this.toolStripSeparator3,
            this.locateAudiobooksToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.importToolStripMenuItem.Text = "&Import";
            // 
            // autoScanLibraryToolStripMenuItem
            // 
            this.autoScanLibraryToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.autoScanLibraryToolStripMenuItem.Name = "autoScanLibraryToolStripMenuItem";
            this.autoScanLibraryToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.autoScanLibraryToolStripMenuItem.Text = "A&uto Scan Library";
            this.autoScanLibraryToolStripMenuItem.Click += new System.EventHandler(this.autoScanLibraryToolStripMenuItem_Click);
            // 
            // noAccountsYetAddAccountToolStripMenuItem
            // 
            this.noAccountsYetAddAccountToolStripMenuItem.Name = "noAccountsYetAddAccountToolStripMenuItem";
            this.noAccountsYetAddAccountToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.noAccountsYetAddAccountToolStripMenuItem.Text = "No accounts yet. A&dd Account...";
            this.noAccountsYetAddAccountToolStripMenuItem.Click += new System.EventHandler(this.noAccountsYetAddAccountToolStripMenuItem_Click);
            // 
            // scanLibraryToolStripMenuItem
            // 
            this.scanLibraryToolStripMenuItem.Name = "scanLibraryToolStripMenuItem";
            this.scanLibraryToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.scanLibraryToolStripMenuItem.Text = "Scan &Library";
            this.scanLibraryToolStripMenuItem.Click += new System.EventHandler(this.scanLibraryToolStripMenuItem_Click);
            // 
            // scanLibraryOfAllAccountsToolStripMenuItem
            // 
            this.scanLibraryOfAllAccountsToolStripMenuItem.Name = "scanLibraryOfAllAccountsToolStripMenuItem";
            this.scanLibraryOfAllAccountsToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.scanLibraryOfAllAccountsToolStripMenuItem.Text = "Scan Library of &All Accounts";
            this.scanLibraryOfAllAccountsToolStripMenuItem.Click += new System.EventHandler(this.scanLibraryOfAllAccountsToolStripMenuItem_Click);
            // 
            // scanLibraryOfSomeAccountsToolStripMenuItem
            // 
            this.scanLibraryOfSomeAccountsToolStripMenuItem.Name = "scanLibraryOfSomeAccountsToolStripMenuItem";
            this.scanLibraryOfSomeAccountsToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.scanLibraryOfSomeAccountsToolStripMenuItem.Text = "Scan Library of &Some Accounts...";
            this.scanLibraryOfSomeAccountsToolStripMenuItem.Click += new System.EventHandler(this.scanLibraryOfSomeAccountsToolStripMenuItem_Click);
            // 
            // removeLibraryBooksToolStripMenuItem
            // 
            this.removeLibraryBooksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeAllAccountsToolStripMenuItem,
            this.removeSomeAccountsToolStripMenuItem});
            this.removeLibraryBooksToolStripMenuItem.Name = "removeLibraryBooksToolStripMenuItem";
            this.removeLibraryBooksToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.removeLibraryBooksToolStripMenuItem.Text = "Remove Library Books";
            this.removeLibraryBooksToolStripMenuItem.Click += new System.EventHandler(this.removeLibraryBooksToolStripMenuItem_Click);
            // 
            // removeAllAccountsToolStripMenuItem
            // 
            this.removeAllAccountsToolStripMenuItem.Name = "removeAllAccountsToolStripMenuItem";
            this.removeAllAccountsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.removeAllAccountsToolStripMenuItem.Text = "All Accounts";
            this.removeAllAccountsToolStripMenuItem.Click += new System.EventHandler(this.removeAllAccountsToolStripMenuItem_Click);
            // 
            // removeSomeAccountsToolStripMenuItem
            // 
            this.removeSomeAccountsToolStripMenuItem.Name = "removeSomeAccountsToolStripMenuItem";
            this.removeSomeAccountsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.removeSomeAccountsToolStripMenuItem.Text = "Some Accounts";
            this.removeSomeAccountsToolStripMenuItem.Click += new System.EventHandler(this.removeSomeAccountsToolStripMenuItem_Click);
            // 
            // liberateToolStripMenuItem
            // 
            this.liberateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginBookBackupsToolStripMenuItem,
            this.beginPdfBackupsToolStripMenuItem,
            this.convertAllM4bToMp3ToolStripMenuItem,
            this.liberateVisibleToolStripMenuItem_LiberateMenu});
            this.liberateToolStripMenuItem.Name = "liberateToolStripMenuItem";
            this.liberateToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.liberateToolStripMenuItem.Text = "&Liberate";
            // 
            // beginBookBackupsToolStripMenuItem
            // 
            this.beginBookBackupsToolStripMenuItem.FormatText = "Begin &Book and PDF Backups: {0}";
            this.beginBookBackupsToolStripMenuItem.Name = "beginBookBackupsToolStripMenuItem";
            this.beginBookBackupsToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.beginBookBackupsToolStripMenuItem.Text = "Begin &Book and PDF Backups: {0}";
            this.beginBookBackupsToolStripMenuItem.Click += new System.EventHandler(this.beginBookBackupsToolStripMenuItem_Click);
            // 
            // beginPdfBackupsToolStripMenuItem
            // 
            this.beginPdfBackupsToolStripMenuItem.FormatText = "Begin &PDF Only Backups: {0}";
            this.beginPdfBackupsToolStripMenuItem.Name = "beginPdfBackupsToolStripMenuItem";
            this.beginPdfBackupsToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.beginPdfBackupsToolStripMenuItem.Text = "Begin &PDF Only Backups: {0}";
            this.beginPdfBackupsToolStripMenuItem.Click += new System.EventHandler(this.beginPdfBackupsToolStripMenuItem_Click);
            // 
            // convertAllM4bToMp3ToolStripMenuItem
            // 
            this.convertAllM4bToMp3ToolStripMenuItem.Name = "convertAllM4bToMp3ToolStripMenuItem";
            this.convertAllM4bToMp3ToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.convertAllM4bToMp3ToolStripMenuItem.Text = "Convert all &M4b to Mp3 [Long-running]...";
            this.convertAllM4bToMp3ToolStripMenuItem.Click += new System.EventHandler(this.convertAllM4bToMp3ToolStripMenuItem_Click);
            // 
            // liberateVisibleToolStripMenuItem_LiberateMenu
            // 
            this.liberateVisibleToolStripMenuItem_LiberateMenu.FormatText = "Liberate &Visible Books: {0}";
            this.liberateVisibleToolStripMenuItem_LiberateMenu.Name = "liberateVisibleToolStripMenuItem_LiberateMenu";
            this.liberateVisibleToolStripMenuItem_LiberateMenu.Size = new System.Drawing.Size(293, 22);
            this.liberateVisibleToolStripMenuItem_LiberateMenu.Text = "Liberate &Visible Books: {0}";
            this.liberateVisibleToolStripMenuItem_LiberateMenu.Click += new System.EventHandler(this.liberateVisible);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportLibraryToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.exportToolStripMenuItem.Text = "E&xport";
            // 
            // exportLibraryToolStripMenuItem
            // 
            this.exportLibraryToolStripMenuItem.Name = "exportLibraryToolStripMenuItem";
            this.exportLibraryToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.exportLibraryToolStripMenuItem.Text = "E&xport Library...";
            this.exportLibraryToolStripMenuItem.Click += new System.EventHandler(this.exportLibraryToolStripMenuItem_Click);
            // 
            // quickFiltersToolStripMenuItem
            // 
            this.quickFiltersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.firstFilterIsDefaultToolStripMenuItem,
            this.editQuickFiltersToolStripMenuItem,
            this.toolStripSeparator1});
            this.quickFiltersToolStripMenuItem.Name = "quickFiltersToolStripMenuItem";
            this.quickFiltersToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.quickFiltersToolStripMenuItem.Text = "Quick &Filters";
            // 
            // firstFilterIsDefaultToolStripMenuItem
            // 
            this.firstFilterIsDefaultToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.firstFilterIsDefaultToolStripMenuItem.Name = "firstFilterIsDefaultToolStripMenuItem";
            this.firstFilterIsDefaultToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.firstFilterIsDefaultToolStripMenuItem.Text = "Start Libation with 1st filter &Default";
            this.firstFilterIsDefaultToolStripMenuItem.Click += new System.EventHandler(this.firstFilterIsDefaultToolStripMenuItem_Click);
            // 
            // editQuickFiltersToolStripMenuItem
            // 
            this.editQuickFiltersToolStripMenuItem.Name = "editQuickFiltersToolStripMenuItem";
            this.editQuickFiltersToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.editQuickFiltersToolStripMenuItem.Text = "&Edit quick filters...";
            this.editQuickFiltersToolStripMenuItem.Click += new System.EventHandler(this.editQuickFiltersToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(253, 6);
            // 
            // scanningToolStripMenuItem
            // 
            this.scanningToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.scanningToolStripMenuItem.Enabled = false;
            this.scanningToolStripMenuItem.Image = global::LibationWinForms.Properties.Resources.import_16x16;
            this.scanningToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.scanningToolStripMenuItem.Name = "scanningToolStripMenuItem";
            this.scanningToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.scanningToolStripMenuItem.Text = "Scanning...";
            this.scanningToolStripMenuItem.Visible = false;
            // 
            // visibleBooksToolStripMenuItem
            // 
            this.visibleBooksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu,
            this.replaceTagsToolStripMenuItem,
            this.setBookDownloadedManualToolStripMenuItem,
            this.setPdfDownloadedManualToolStripMenuItem,
            this.setDownloadedAutoToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.visibleBooksToolStripMenuItem.FormatText = "&Visible Books: {0}";
            this.visibleBooksToolStripMenuItem.Name = "visibleBooksToolStripMenuItem";
            this.visibleBooksToolStripMenuItem.Size = new System.Drawing.Size(108, 20);
            this.visibleBooksToolStripMenuItem.Text = "&Visible Books: {0}";
            // 
            // liberateVisibleToolStripMenuItem_VisibleBooksMenu
            // 
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu.FormatText = "&Liberate: {0}";
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu.Name = "liberateVisibleToolStripMenuItem_VisibleBooksMenu";
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu.Size = new System.Drawing.Size(314, 22);
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu.Text = "&Liberate: {0}";
            this.liberateVisibleToolStripMenuItem_VisibleBooksMenu.Click += new System.EventHandler(this.liberateVisible);
            // 
            // replaceTagsToolStripMenuItem
            // 
            this.replaceTagsToolStripMenuItem.Name = "replaceTagsToolStripMenuItem";
            this.replaceTagsToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.replaceTagsToolStripMenuItem.Text = "Replace &Tags...";
            this.replaceTagsToolStripMenuItem.Click += new System.EventHandler(this.replaceTagsToolStripMenuItem_Click);
            // 
            // setBookDownloadedManualToolStripMenuItem
            // 
            this.setBookDownloadedManualToolStripMenuItem.Name = "setBookDownloadedManualToolStripMenuItem";
            this.setBookDownloadedManualToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.setBookDownloadedManualToolStripMenuItem.Text = "Set book \'&Downloaded\' status manually...";
            this.setBookDownloadedManualToolStripMenuItem.Click += new System.EventHandler(this.setBookDownloadedManualToolStripMenuItem_Click);
            // 
            // setPdfDownloadedManualToolStripMenuItem
            // 
            this.setPdfDownloadedManualToolStripMenuItem.Name = "setPdfDownloadedManualToolStripMenuItem";
            this.setPdfDownloadedManualToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.setPdfDownloadedManualToolStripMenuItem.Text = "Set &PDF \'Downloaded\' status manually...";
            this.setPdfDownloadedManualToolStripMenuItem.Click += new System.EventHandler(this.setPdfDownloadedManualToolStripMenuItem_Click);
            // 
            // setDownloadedAutoToolStripMenuItem
            // 
            this.setDownloadedAutoToolStripMenuItem.Name = "setDownloadedAutoToolStripMenuItem";
            this.setDownloadedAutoToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.setDownloadedAutoToolStripMenuItem.Text = "Set book \'Downloaded\' status &automatically...";
            this.setDownloadedAutoToolStripMenuItem.Click += new System.EventHandler(this.setDownloadedAutoToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.removeToolStripMenuItem.Text = "&Remove from library...";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accountsToolStripMenuItem,
            this.basicSettingsToolStripMenuItem,
			this.toolStripSeparator4,
			this.openTrashBinToolStripMenuItem,
			this.launchHangoverToolStripMenuItem,
			this.toolStripSeparator2,
            this.tourToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "&Settings";
            // 
            // accountsToolStripMenuItem
            // 
            this.accountsToolStripMenuItem.Name = "accountsToolStripMenuItem";
            this.accountsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.accountsToolStripMenuItem.Text = "&Accounts...";
            this.accountsToolStripMenuItem.Click += new System.EventHandler(this.accountsToolStripMenuItem_Click);
            // 
            // basicSettingsToolStripMenuItem
            // 
            this.basicSettingsToolStripMenuItem.Name = "basicSettingsToolStripMenuItem";
            this.basicSettingsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.basicSettingsToolStripMenuItem.Text = "&Settings...";
            this.basicSettingsToolStripMenuItem.Click += new System.EventHandler(this.basicSettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(130, 6);
            // 
            // tourToolStripMenuItem
            // 
            this.tourToolStripMenuItem.Name = "tourToolStripMenuItem";
            this.tourToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.tourToolStripMenuItem.Text = "Take a Guided &Tour of Libation";
            this.tourToolStripMenuItem.Click += new System.EventHandler(this.tourToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.aboutToolStripMenuItem.Text = "A&bout...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.upgradeLbl,
            this.upgradePb,
			this.visibleCountLbl,
            this.springLbl,
            this.backupsCountsLbl});
            this.statusStrip1.Location = new System.Drawing.Point(0, 618);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(1025, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
			// 
			// upgradePb
			// 
			this.upgradePb.Name = "upgradePb";
			this.upgradePb.Size = new System.Drawing.Size(100, 16);
			// 
			// upgradeLbl
			// 
			this.upgradeLbl.Name = "upgradeLbl";
			this.upgradeLbl.Size = new System.Drawing.Size(66, 17);
			this.upgradeLbl.Text = "Upgrading:";
			// 
			// visibleCountLbl
			// 
			this.visibleCountLbl.FormatText = "Visible: {0}";
            this.visibleCountLbl.Name = "visibleCountLbl";
            this.visibleCountLbl.Size = new System.Drawing.Size(61, 17);
            this.visibleCountLbl.Text = "Visible: {0}";
            // 
            // springLbl
            // 
            this.springLbl.Name = "springLbl";
            this.springLbl.Size = new System.Drawing.Size(511, 17);
            this.springLbl.Spring = true;
            // 
            // backupsCountsLbl
            // 
            this.backupsCountsLbl.Name = "backupsCountsLbl";
            this.backupsCountsLbl.Size = new System.Drawing.Size(218, 17);
            this.backupsCountsLbl.Text = "[Calculating backed up book quantities]";
            // 
            // addQuickFilterBtn
            // 
            this.addQuickFilterBtn.Location = new System.Drawing.Point(50, 3);
            this.addQuickFilterBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.addQuickFilterBtn.Name = "addQuickFilterBtn";
            this.addQuickFilterBtn.Size = new System.Drawing.Size(137, 27);
            this.addQuickFilterBtn.TabIndex = 4;
            this.addQuickFilterBtn.Text = "Add To Quick Filters";
            this.addQuickFilterBtn.UseVisualStyleBackColor = true;
            this.addQuickFilterBtn.Click += new System.EventHandler(this.addQuickFilterBtn_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            this.splitContainer1.Panel1.Controls.Add(this.statusStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.processBookQueue1);
            this.splitContainer1.Size = new System.Drawing.Size(1463, 640);
            this.splitContainer1.SplitterDistance = 1025;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.productsDisplay);
            this.panel1.Controls.Add(this.toggleQueueHideBtn);
            this.panel1.Controls.Add(this.doneRemovingBtn);
            this.panel1.Controls.Add(this.removeBooksBtn);
            this.panel1.Controls.Add(this.addQuickFilterBtn);
            this.panel1.Controls.Add(this.filterHelpBtn);
            this.panel1.Controls.Add(this.filterSearchTb);
            this.panel1.Controls.Add(this.filterBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1025, 594);
            this.panel1.TabIndex = 7;
            // 
            // productsDisplay
            // 
            this.productsDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.productsDisplay.AutoScroll = true;
            this.productsDisplay.Location = new System.Drawing.Point(15, 36);
            this.productsDisplay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.productsDisplay.Name = "productsDisplay";
            this.productsDisplay.Size = new System.Drawing.Size(999, 555);
            this.productsDisplay.TabIndex = 9;
            this.productsDisplay.VisibleCountChanged += new System.EventHandler<int>(this.productsDisplay_VisibleCountChanged);
            this.productsDisplay.RemovableCountChanged += new System.EventHandler<int>(this.productsDisplay_RemovableCountChanged);
            this.productsDisplay.LiberateClicked += new System.EventHandler<DataLayer.LibraryBook>(this.ProductsDisplay_LiberateClicked);
            this.productsDisplay.LiberateSeriesClicked += new System.EventHandler<LibationUiBase.GridView.ISeriesEntry>(this.ProductsDisplay_LiberateSeriesClicked);
            this.productsDisplay.ConvertToMp3Clicked += new System.EventHandler<DataLayer.LibraryBook>(this.ProductsDisplay_ConvertToMp3Clicked);
            this.productsDisplay.InitialLoaded += new System.EventHandler(this.productsDisplay_InitialLoaded);
            // 
            // toggleQueueHideBtn
            // 
            this.toggleQueueHideBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toggleQueueHideBtn.Location = new System.Drawing.Point(977, 3);
            this.toggleQueueHideBtn.Margin = new System.Windows.Forms.Padding(4, 3, 15, 3);
            this.toggleQueueHideBtn.Name = "toggleQueueHideBtn";
            this.toggleQueueHideBtn.Size = new System.Drawing.Size(33, 27);
            this.toggleQueueHideBtn.TabIndex = 8;
            this.toggleQueueHideBtn.Text = "❱❱❱";
            this.toggleQueueHideBtn.UseVisualStyleBackColor = true;
            this.toggleQueueHideBtn.Click += new System.EventHandler(this.ToggleQueueHideBtn_Click);
            // 
            // doneRemovingBtn
            // 
            this.doneRemovingBtn.Location = new System.Drawing.Point(406, 3);
            this.doneRemovingBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.doneRemovingBtn.Name = "doneRemovingBtn";
            this.doneRemovingBtn.Size = new System.Drawing.Size(145, 27);
            this.doneRemovingBtn.TabIndex = 4;
            this.doneRemovingBtn.Text = "Done Removing Books";
            this.doneRemovingBtn.UseVisualStyleBackColor = true;
            this.doneRemovingBtn.Visible = false;
            this.doneRemovingBtn.Click += new System.EventHandler(this.doneRemovingBtn_Click);
            // 
            // removeBooksBtn
            // 
            this.removeBooksBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.removeBooksBtn.Location = new System.Drawing.Point(206, 3);
            this.removeBooksBtn.Margin = new System.Windows.Forms.Padding(15, 3, 4, 3);
            this.removeBooksBtn.Name = "removeBooksBtn";
            this.removeBooksBtn.Size = new System.Drawing.Size(192, 27);
            this.removeBooksBtn.TabIndex = 4;
            this.removeBooksBtn.Text = "Remove # Books from Libation";
            this.removeBooksBtn.UseVisualStyleBackColor = true;
            this.removeBooksBtn.Visible = false;
            this.removeBooksBtn.Click += new System.EventHandler(this.removeBooksBtn_Click);
            // 
            // processBookQueue1
            // 
            this.processBookQueue1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.processBookQueue1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.processBookQueue1.Location = new System.Drawing.Point(0, 0);
            this.processBookQueue1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.processBookQueue1.Name = "processBookQueue1";
            this.processBookQueue1.Size = new System.Drawing.Size(430, 640);
            this.processBookQueue1.TabIndex = 0;
			// 
			// locateAudiobooksToolStripMenuItem
			// 
			this.locateAudiobooksToolStripMenuItem.Name = "locateAudiobooksToolStripMenuItem";
			this.locateAudiobooksToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.locateAudiobooksToolStripMenuItem.Text = "L&ocate Audiobooks";
			this.locateAudiobooksToolStripMenuItem.Click += new System.EventHandler(this.locateAudiobooksToolStripMenuItem_Click);
			// 
			// openTrashBinToolStripMenuItem
			// 
			this.openTrashBinToolStripMenuItem.Name = "openTrashBinToolStripMenuItem";
			this.openTrashBinToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.openTrashBinToolStripMenuItem.Text = "Trash Bin";
			this.openTrashBinToolStripMenuItem.Click += new System.EventHandler(this.openTrashBinToolStripMenuItem_Click);
			// 
			// launchHangoverToolStripMenuItem
			// 
			this.launchHangoverToolStripMenuItem.Name = "launchHangoverToolStripMenuItem";
			this.launchHangoverToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.launchHangoverToolStripMenuItem.Text = "Launch &Hangover";
			this.launchHangoverToolStripMenuItem.Click += new System.EventHandler(this.launchHangoverToolStripMenuItem_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(244, 6);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1463, 640);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "Libation: Liberate your Library";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.MenuStrip menuStrip1;
		public System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel springLbl;
		private LibationWinForms.FormattableToolStripStatusLabel visibleCountLbl;
		private System.Windows.Forms.ToolStripMenuItem liberateToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel backupsCountsLbl;
		private LibationWinForms.FormattableToolStripMenuItem beginBookBackupsToolStripMenuItem;
		private LibationWinForms.FormattableToolStripMenuItem beginPdfBackupsToolStripMenuItem;
		public ClearableTextBox filterSearchTb;
		public System.Windows.Forms.Button filterBtn;
		public System.Windows.Forms.Button filterHelpBtn;
		public System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		public System.Windows.Forms.ToolStripMenuItem scanLibraryToolStripMenuItem;
		public System.Windows.Forms.ToolStripMenuItem quickFiltersToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem firstFilterIsDefaultToolStripMenuItem;
		public System.Windows.Forms.Button addQuickFilterBtn;
		public System.Windows.Forms.ToolStripMenuItem editQuickFiltersToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		public System.Windows.Forms.ToolStripMenuItem basicSettingsToolStripMenuItem;
		public System.Windows.Forms.ToolStripMenuItem accountsToolStripMenuItem;
		public System.Windows.Forms.ToolStripMenuItem scanLibraryOfAllAccountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryOfSomeAccountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem noAccountsYetAddAccountToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertAllM4bToMp3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeLibraryBooksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeSomeAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoScanLibraryToolStripMenuItem;
        private LibationWinForms.FormattableToolStripMenuItem visibleBooksToolStripMenuItem;
        private LibationWinForms.FormattableToolStripMenuItem liberateVisibleToolStripMenuItem_VisibleBooksMenu;
        private System.Windows.Forms.ToolStripMenuItem replaceTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setBookDownloadedManualToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setDownloadedAutoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem locateAudiobooksToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem openTrashBinToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem launchHangoverToolStripMenuItem;
        private LibationWinForms.FormattableToolStripMenuItem liberateVisibleToolStripMenuItem_LiberateMenu;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private LibationWinForms.ProcessQueue.ProcessQueueControl processBookQueue1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button toggleQueueHideBtn;
		public LibationWinForms.GridView.ProductsDisplay productsDisplay;
		private System.Windows.Forms.Button removeBooksBtn;
		private System.Windows.Forms.Button doneRemovingBtn;
        private System.Windows.Forms.ToolStripMenuItem setPdfDownloadedManualToolStripMenuItem;
		public System.Windows.Forms.ToolStripProgressBar upgradePb;
		public System.Windows.Forms.ToolStripStatusLabel upgradeLbl;
	}
}
