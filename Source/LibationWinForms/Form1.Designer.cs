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
			this.gridPanel = new System.Windows.Forms.Panel();
			this.filterHelpBtn = new System.Windows.Forms.Button();
			this.filterBtn = new System.Windows.Forms.Button();
			this.filterSearchTb = new System.Windows.Forms.TextBox();
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
			this.liberateVisible2ToolStripMenuItem = new LibationWinForms.FormattableToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quickFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.firstFilterIsDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editQuickFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.scanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.visibleBooksToolStripMenuItem = new LibationWinForms.FormattableToolStripMenuItem();
			this.liberateVisibleToolStripMenuItem = new LibationWinForms.FormattableToolStripMenuItem();
			this.replaceTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.setDownloadedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.accountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.basicSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.visibleCountLbl = new LibationWinForms.FormattableToolStripStatusLabel();
			this.springLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.backupsCountsLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.pdfsCountsLbl = new LibationWinForms.FormattableToolStripStatusLabel();
			this.addQuickFilterBtn = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.panel1 = new System.Windows.Forms.Panel();
			this.toggleQueueHideBtn = new System.Windows.Forms.Button();
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
			// gridPanel
			// 
			this.gridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridPanel.Location = new System.Drawing.Point(17, 44);
			this.gridPanel.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.gridPanel.Name = "gridPanel";
			this.gridPanel.Size = new System.Drawing.Size(987, 752);
			this.gridPanel.TabIndex = 5;
			// 
			// filterHelpBtn
			// 
			this.filterHelpBtn.Location = new System.Drawing.Point(17, 4);
			this.filterHelpBtn.Margin = new System.Windows.Forms.Padding(17, 4, 5, 4);
			this.filterHelpBtn.Name = "filterHelpBtn";
			this.filterHelpBtn.Size = new System.Drawing.Size(30, 36);
			this.filterHelpBtn.TabIndex = 3;
			this.filterHelpBtn.Text = "?";
			this.filterHelpBtn.UseVisualStyleBackColor = true;
			this.filterHelpBtn.Click += new System.EventHandler(this.filterHelpBtn_Click);
			// 
			// filterBtn
			// 
			this.filterBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterBtn.Location = new System.Drawing.Point(855, 4);
			this.filterBtn.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.filterBtn.Name = "filterBtn";
			this.filterBtn.Size = new System.Drawing.Size(101, 36);
			this.filterBtn.TabIndex = 2;
			this.filterBtn.Text = "Filter";
			this.filterBtn.UseVisualStyleBackColor = true;
			this.filterBtn.Click += new System.EventHandler(this.filterBtn_Click);
			// 
			// filterSearchTb
			// 
			this.filterSearchTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterSearchTb.Location = new System.Drawing.Point(224, 9);
			this.filterSearchTb.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.filterSearchTb.Name = "filterSearchTb";
			this.filterSearchTb.Size = new System.Drawing.Size(621, 27);
			this.filterSearchTb.TabIndex = 1;
			this.filterSearchTb.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.filterSearchTb_KeyPress);
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
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(1021, 28);
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
            this.removeLibraryBooksToolStripMenuItem});
			this.importToolStripMenuItem.Name = "importToolStripMenuItem";
			this.importToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
			this.importToolStripMenuItem.Text = "&Import";
			// 
			// autoScanLibraryToolStripMenuItem
			// 
			this.autoScanLibraryToolStripMenuItem.Name = "autoScanLibraryToolStripMenuItem";
			this.autoScanLibraryToolStripMenuItem.Size = new System.Drawing.Size(305, 26);
			this.autoScanLibraryToolStripMenuItem.Text = "A&uto Scan Library";
			this.autoScanLibraryToolStripMenuItem.Click += new System.EventHandler(this.autoScanLibraryToolStripMenuItem_Click);
			// 
			// noAccountsYetAddAccountToolStripMenuItem
			// 
			this.noAccountsYetAddAccountToolStripMenuItem.Name = "noAccountsYetAddAccountToolStripMenuItem";
			this.noAccountsYetAddAccountToolStripMenuItem.Size = new System.Drawing.Size(305, 26);
			this.noAccountsYetAddAccountToolStripMenuItem.Text = "No accounts yet. A&dd Account...";
			this.noAccountsYetAddAccountToolStripMenuItem.Click += new System.EventHandler(this.noAccountsYetAddAccountToolStripMenuItem_Click);
			// 
			// scanLibraryToolStripMenuItem
			// 
			this.scanLibraryToolStripMenuItem.Name = "scanLibraryToolStripMenuItem";
			this.scanLibraryToolStripMenuItem.Size = new System.Drawing.Size(305, 26);
			this.scanLibraryToolStripMenuItem.Text = "Scan &Library";
			this.scanLibraryToolStripMenuItem.Click += new System.EventHandler(this.scanLibraryToolStripMenuItem_Click);
			// 
			// scanLibraryOfAllAccountsToolStripMenuItem
			// 
			this.scanLibraryOfAllAccountsToolStripMenuItem.Name = "scanLibraryOfAllAccountsToolStripMenuItem";
			this.scanLibraryOfAllAccountsToolStripMenuItem.Size = new System.Drawing.Size(305, 26);
			this.scanLibraryOfAllAccountsToolStripMenuItem.Text = "Scan Library of &All Accounts";
			this.scanLibraryOfAllAccountsToolStripMenuItem.Click += new System.EventHandler(this.scanLibraryOfAllAccountsToolStripMenuItem_Click);
			// 
			// scanLibraryOfSomeAccountsToolStripMenuItem
			// 
			this.scanLibraryOfSomeAccountsToolStripMenuItem.Name = "scanLibraryOfSomeAccountsToolStripMenuItem";
			this.scanLibraryOfSomeAccountsToolStripMenuItem.Size = new System.Drawing.Size(305, 26);
			this.scanLibraryOfSomeAccountsToolStripMenuItem.Text = "Scan Library of &Some Accounts...";
			this.scanLibraryOfSomeAccountsToolStripMenuItem.Click += new System.EventHandler(this.scanLibraryOfSomeAccountsToolStripMenuItem_Click);
			// 
			// removeLibraryBooksToolStripMenuItem
			// 
			this.removeLibraryBooksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeAllAccountsToolStripMenuItem,
            this.removeSomeAccountsToolStripMenuItem});
			this.removeLibraryBooksToolStripMenuItem.Name = "removeLibraryBooksToolStripMenuItem";
			this.removeLibraryBooksToolStripMenuItem.Size = new System.Drawing.Size(305, 26);
			this.removeLibraryBooksToolStripMenuItem.Text = "Remove Library Books";
			this.removeLibraryBooksToolStripMenuItem.Click += new System.EventHandler(this.removeLibraryBooksToolStripMenuItem_Click);
			// 
			// removeAllAccountsToolStripMenuItem
			// 
			this.removeAllAccountsToolStripMenuItem.Name = "removeAllAccountsToolStripMenuItem";
			this.removeAllAccountsToolStripMenuItem.Size = new System.Drawing.Size(194, 26);
			this.removeAllAccountsToolStripMenuItem.Text = "All Accounts";
			this.removeAllAccountsToolStripMenuItem.Click += new System.EventHandler(this.removeAllAccountsToolStripMenuItem_Click);
			// 
			// removeSomeAccountsToolStripMenuItem
			// 
			this.removeSomeAccountsToolStripMenuItem.Name = "removeSomeAccountsToolStripMenuItem";
			this.removeSomeAccountsToolStripMenuItem.Size = new System.Drawing.Size(194, 26);
			this.removeSomeAccountsToolStripMenuItem.Text = "Some Accounts";
			this.removeSomeAccountsToolStripMenuItem.Click += new System.EventHandler(this.removeSomeAccountsToolStripMenuItem_Click);
			// 
			// liberateToolStripMenuItem
			// 
			this.liberateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginBookBackupsToolStripMenuItem,
            this.beginPdfBackupsToolStripMenuItem,
            this.convertAllM4bToMp3ToolStripMenuItem,
            this.liberateVisible2ToolStripMenuItem});
			this.liberateToolStripMenuItem.Name = "liberateToolStripMenuItem";
			this.liberateToolStripMenuItem.Size = new System.Drawing.Size(77, 24);
			this.liberateToolStripMenuItem.Text = "&Liberate";
			// 
			// beginBookBackupsToolStripMenuItem
			// 
			this.beginBookBackupsToolStripMenuItem.Name = "beginBookBackupsToolStripMenuItem";
			this.beginBookBackupsToolStripMenuItem.Size = new System.Drawing.Size(361, 26);
			this.beginBookBackupsToolStripMenuItem.Text = "Begin &Book and PDF Backups: {0}";
			this.beginBookBackupsToolStripMenuItem.Click += new System.EventHandler(this.beginBookBackupsToolStripMenuItem_Click);
			// 
			// beginPdfBackupsToolStripMenuItem
			// 
			this.beginPdfBackupsToolStripMenuItem.Name = "beginPdfBackupsToolStripMenuItem";
			this.beginPdfBackupsToolStripMenuItem.Size = new System.Drawing.Size(361, 26);
			this.beginPdfBackupsToolStripMenuItem.Text = "Begin &PDF Only Backups: {0}";
			this.beginPdfBackupsToolStripMenuItem.Click += new System.EventHandler(this.beginPdfBackupsToolStripMenuItem_Click);
			// 
			// convertAllM4bToMp3ToolStripMenuItem
			// 
			this.convertAllM4bToMp3ToolStripMenuItem.Name = "convertAllM4bToMp3ToolStripMenuItem";
			this.convertAllM4bToMp3ToolStripMenuItem.Size = new System.Drawing.Size(361, 26);
			this.convertAllM4bToMp3ToolStripMenuItem.Text = "Convert all &M4b to Mp3 [Long-running]...";
			this.convertAllM4bToMp3ToolStripMenuItem.Click += new System.EventHandler(this.convertAllM4bToMp3ToolStripMenuItem_Click);
			// 
			// liberateVisible2ToolStripMenuItem
			// 
			this.liberateVisible2ToolStripMenuItem.Name = "liberateVisible2ToolStripMenuItem";
			this.liberateVisible2ToolStripMenuItem.Size = new System.Drawing.Size(361, 26);
			this.liberateVisible2ToolStripMenuItem.Text = "Liberate &Visible Books: {0}";
			this.liberateVisible2ToolStripMenuItem.Click += new System.EventHandler(this.liberateVisible);
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportLibraryToolStripMenuItem});
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
			this.exportToolStripMenuItem.Text = "E&xport";
			// 
			// exportLibraryToolStripMenuItem
			// 
			this.exportLibraryToolStripMenuItem.Name = "exportLibraryToolStripMenuItem";
			this.exportLibraryToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
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
			this.quickFiltersToolStripMenuItem.Size = new System.Drawing.Size(103, 24);
			this.quickFiltersToolStripMenuItem.Text = "Quick &Filters";
			// 
			// firstFilterIsDefaultToolStripMenuItem
			// 
			this.firstFilterIsDefaultToolStripMenuItem.Name = "firstFilterIsDefaultToolStripMenuItem";
			this.firstFilterIsDefaultToolStripMenuItem.Size = new System.Drawing.Size(324, 26);
			this.firstFilterIsDefaultToolStripMenuItem.Text = "Start Libation with 1st filter &Default";
			this.firstFilterIsDefaultToolStripMenuItem.Click += new System.EventHandler(this.firstFilterIsDefaultToolStripMenuItem_Click);
			// 
			// editQuickFiltersToolStripMenuItem
			// 
			this.editQuickFiltersToolStripMenuItem.Name = "editQuickFiltersToolStripMenuItem";
			this.editQuickFiltersToolStripMenuItem.Size = new System.Drawing.Size(324, 26);
			this.editQuickFiltersToolStripMenuItem.Text = "&Edit quick filters...";
			this.editQuickFiltersToolStripMenuItem.Click += new System.EventHandler(this.editQuickFiltersToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(321, 6);
			// 
			// scanningToolStripMenuItem
			// 
			this.scanningToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.scanningToolStripMenuItem.Enabled = false;
			this.scanningToolStripMenuItem.Image = global::LibationWinForms.Properties.Resources.import_16x16;
			this.scanningToolStripMenuItem.Name = "scanningToolStripMenuItem";
			this.scanningToolStripMenuItem.Size = new System.Drawing.Size(132, 44);
			this.scanningToolStripMenuItem.Text = "Scanning...";
			this.scanningToolStripMenuItem.Visible = false;
			// 
			// visibleBooksToolStripMenuItem
			// 
			this.visibleBooksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.liberateVisibleToolStripMenuItem,
            this.replaceTagsToolStripMenuItem,
            this.setDownloadedToolStripMenuItem,
            this.removeToolStripMenuItem});
			this.visibleBooksToolStripMenuItem.Name = "visibleBooksToolStripMenuItem";
			this.visibleBooksToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
			this.visibleBooksToolStripMenuItem.Text = "&Visible Books: {0}";
			// 
			// liberateVisibleToolStripMenuItem
			// 
			this.liberateVisibleToolStripMenuItem.Name = "liberateVisibleToolStripMenuItem";
			this.liberateVisibleToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
			this.liberateVisibleToolStripMenuItem.Text = "&Liberate: {0}";
			this.liberateVisibleToolStripMenuItem.Click += new System.EventHandler(this.liberateVisible);
			// 
			// replaceTagsToolStripMenuItem
			// 
			this.replaceTagsToolStripMenuItem.Name = "replaceTagsToolStripMenuItem";
			this.replaceTagsToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
			this.replaceTagsToolStripMenuItem.Text = "Replace &Tags...";
			this.replaceTagsToolStripMenuItem.Click += new System.EventHandler(this.replaceTagsToolStripMenuItem_Click);
			// 
			// setDownloadedToolStripMenuItem
			// 
			this.setDownloadedToolStripMenuItem.Name = "setDownloadedToolStripMenuItem";
			this.setDownloadedToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
			this.setDownloadedToolStripMenuItem.Text = "Set \'&Downloaded\' status...";
			this.setDownloadedToolStripMenuItem.Click += new System.EventHandler(this.setDownloadedToolStripMenuItem_Click);
			// 
			// removeToolStripMenuItem
			// 
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new System.Drawing.Size(260, 26);
			this.removeToolStripMenuItem.Text = "&Remove from library...";
			this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accountsToolStripMenuItem,
            this.basicSettingsToolStripMenuItem,
            this.toolStripSeparator2,
            this.aboutToolStripMenuItem});
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(76, 24);
			this.settingsToolStripMenuItem.Text = "&Settings";
			// 
			// accountsToolStripMenuItem
			// 
			this.accountsToolStripMenuItem.Name = "accountsToolStripMenuItem";
			this.accountsToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
			this.accountsToolStripMenuItem.Text = "&Accounts...";
			this.accountsToolStripMenuItem.Click += new System.EventHandler(this.accountsToolStripMenuItem_Click);
			// 
			// basicSettingsToolStripMenuItem
			// 
			this.basicSettingsToolStripMenuItem.Name = "basicSettingsToolStripMenuItem";
			this.basicSettingsToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
			this.basicSettingsToolStripMenuItem.Text = "&Settings...";
			this.basicSettingsToolStripMenuItem.Click += new System.EventHandler(this.basicSettingsToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(158, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
			this.aboutToolStripMenuItem.Text = "A&bout...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.visibleCountLbl,
            this.springLbl,
            this.backupsCountsLbl,
            this.pdfsCountsLbl});
			this.statusStrip1.Location = new System.Drawing.Point(0, 828);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 18, 0);
			this.statusStrip1.Size = new System.Drawing.Size(1021, 26);
			this.statusStrip1.TabIndex = 6;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// visibleCountLbl
			// 
			this.visibleCountLbl.Name = "visibleCountLbl";
			this.visibleCountLbl.Size = new System.Drawing.Size(68, 20);
			this.visibleCountLbl.Text = "Visible: {0}";
			// 
			// springLbl
			// 
			this.springLbl.Name = "springLbl";
			this.springLbl.Size = new System.Drawing.Size(447, 20);
			this.springLbl.Spring = true;
			// 
			// backupsCountsLbl
			// 
			this.backupsCountsLbl.Name = "backupsCountsLbl";
			this.backupsCountsLbl.Size = new System.Drawing.Size(273, 20);
			this.backupsCountsLbl.Text = "[Calculating backed up book quantities]";
			// 
			// pdfsCountsLbl
			// 
			this.pdfsCountsLbl.Name = "pdfsCountsLbl";
			this.pdfsCountsLbl.Size = new System.Drawing.Size(214, 20);
			this.pdfsCountsLbl.Text = "|  PDFs: NOT d/l\'ed: {0}  Downloaded: {1}";
			// 
			// addQuickFilterBtn
			// 
			this.addQuickFilterBtn.Location = new System.Drawing.Point(57, 4);
			this.addQuickFilterBtn.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.addQuickFilterBtn.Name = "addQuickFilterBtn";
			this.addQuickFilterBtn.Size = new System.Drawing.Size(157, 36);
			this.addQuickFilterBtn.TabIndex = 4;
			this.addQuickFilterBtn.Text = "Add To Quick Filters";
			this.addQuickFilterBtn.UseVisualStyleBackColor = true;
			this.addQuickFilterBtn.Click += new System.EventHandler(this.addQuickFilterBtn_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
			this.splitContainer1.Size = new System.Drawing.Size(1407, 854);
			this.splitContainer1.SplitterDistance = 1021;
			this.splitContainer1.SplitterWidth = 9;
			this.splitContainer1.TabIndex = 7;
			// 
			// panel1
			// 
			this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panel1.Controls.Add(this.toggleQueueHideBtn);
			this.panel1.Controls.Add(this.gridPanel);
			this.panel1.Controls.Add(this.addQuickFilterBtn);
			this.panel1.Controls.Add(this.filterHelpBtn);
			this.panel1.Controls.Add(this.filterSearchTb);
			this.panel1.Controls.Add(this.filterBtn);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 28);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1021, 800);
			this.panel1.TabIndex = 7;
			// 
			// hideQueueBtn
			// 
			this.toggleQueueHideBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.toggleQueueHideBtn.Location = new System.Drawing.Point(966, 4);
			this.toggleQueueHideBtn.Margin = new System.Windows.Forms.Padding(5, 4, 17, 4);
			this.toggleQueueHideBtn.Name = "hideQueueBtn";
			this.toggleQueueHideBtn.Size = new System.Drawing.Size(38, 36);
			this.toggleQueueHideBtn.TabIndex = 8;
			this.toggleQueueHideBtn.Text = "❱❱❱";
			this.toggleQueueHideBtn.UseVisualStyleBackColor = true;
			this.toggleQueueHideBtn.Click += new System.EventHandler(this.ToggleQueueHideBtn_Click);
			// 
			// processBookQueue1
			// 
			this.processBookQueue1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.processBookQueue1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processBookQueue1.Location = new System.Drawing.Point(0, 0);
			this.processBookQueue1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.processBookQueue1.Name = "processBookQueue1";
			this.processBookQueue1.Size = new System.Drawing.Size(377, 854);
			this.processBookQueue1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1407, 854);
			this.Controls.Add(this.splitContainer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
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

		private System.Windows.Forms.Panel gridPanel;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel springLbl;
		private LibationWinForms.FormattableToolStripStatusLabel visibleCountLbl;
		private System.Windows.Forms.ToolStripMenuItem liberateToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel backupsCountsLbl;
		private LibationWinForms.FormattableToolStripMenuItem beginBookBackupsToolStripMenuItem;
		private LibationWinForms.FormattableToolStripStatusLabel pdfsCountsLbl;
		private LibationWinForms.FormattableToolStripMenuItem beginPdfBackupsToolStripMenuItem;
		private System.Windows.Forms.TextBox filterSearchTb;
		private System.Windows.Forms.Button filterBtn;
		private System.Windows.Forms.Button filterHelpBtn;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quickFiltersToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem firstFilterIsDefaultToolStripMenuItem;
		private System.Windows.Forms.Button addQuickFilterBtn;
		private System.Windows.Forms.ToolStripMenuItem editQuickFiltersToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem basicSettingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem accountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryOfAllAccountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryOfSomeAccountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem noAccountsYetAddAccountToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertAllM4bToMp3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeLibraryBooksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeSomeAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoScanLibraryToolStripMenuItem;
        private LibationWinForms.FormattableToolStripMenuItem visibleBooksToolStripMenuItem;
        private LibationWinForms.FormattableToolStripMenuItem liberateVisibleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setDownloadedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private LibationWinForms.FormattableToolStripMenuItem liberateVisible2ToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private LibationWinForms.ProcessQueue.ProcessQueueControl processBookQueue1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button toggleQueueHideBtn;
	}
}
