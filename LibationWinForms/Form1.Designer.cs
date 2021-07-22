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
			this.noAccountsYetAddAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scanLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scanLibraryOfAllAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scanLibraryOfSomeAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.liberateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.beginBookBackupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.beginPdfBackupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quickFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.firstFilterIsDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editQuickFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.accountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.basicSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.visibleCountLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.springLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.backupsCountsLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.pdfsCountsLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.addFilterBtn = new System.Windows.Forms.Button();
			this.menuStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridPanel
			// 
			this.gridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridPanel.Location = new System.Drawing.Point(14, 65);
			this.gridPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.gridPanel.Name = "gridPanel";
			this.gridPanel.Size = new System.Drawing.Size(979, 445);
			this.gridPanel.TabIndex = 5;
			// 
			// filterHelpBtn
			// 
			this.filterHelpBtn.Location = new System.Drawing.Point(14, 31);
			this.filterHelpBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
			this.filterBtn.Location = new System.Drawing.Point(905, 31);
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
			this.filterSearchTb.Location = new System.Drawing.Point(217, 33);
			this.filterSearchTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.filterSearchTb.Name = "filterSearchTb";
			this.filterSearchTb.Size = new System.Drawing.Size(681, 23);
			this.filterSearchTb.TabIndex = 1;
			this.filterSearchTb.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.filterSearchTb_KeyPress);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.liberateToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.quickFiltersToolStripMenuItem,
            this.settingsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(1007, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// importToolStripMenuItem
			// 
			this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noAccountsYetAddAccountToolStripMenuItem,
            this.scanLibraryToolStripMenuItem,
            this.scanLibraryOfAllAccountsToolStripMenuItem,
            this.scanLibraryOfSomeAccountsToolStripMenuItem});
			this.importToolStripMenuItem.Name = "importToolStripMenuItem";
			this.importToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
			this.importToolStripMenuItem.Text = "&Import";
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
			// liberateToolStripMenuItem
			// 
			this.liberateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginBookBackupsToolStripMenuItem,
            this.beginPdfBackupsToolStripMenuItem});
			this.liberateToolStripMenuItem.Name = "liberateToolStripMenuItem";
			this.liberateToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
			this.liberateToolStripMenuItem.Text = "&Liberate";
			// 
			// beginBookBackupsToolStripMenuItem
			// 
			this.beginBookBackupsToolStripMenuItem.Name = "beginBookBackupsToolStripMenuItem";
			this.beginBookBackupsToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
			this.beginBookBackupsToolStripMenuItem.Text = "Begin &Book and PDF Backups: {0}";
			this.beginBookBackupsToolStripMenuItem.Click += new System.EventHandler(this.beginBookBackupsToolStripMenuItem_Click);
			// 
			// beginPdfBackupsToolStripMenuItem
			// 
			this.beginPdfBackupsToolStripMenuItem.Name = "beginPdfBackupsToolStripMenuItem";
			this.beginPdfBackupsToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
			this.beginPdfBackupsToolStripMenuItem.Text = "Begin &PDF Only Backups: {0}";
			this.beginPdfBackupsToolStripMenuItem.Click += new System.EventHandler(this.beginPdfBackupsToolStripMenuItem_Click);
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
			this.firstFilterIsDefaultToolStripMenuItem.Name = "firstFilterIsDefaultToolStripMenuItem";
			this.firstFilterIsDefaultToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
			this.firstFilterIsDefaultToolStripMenuItem.Text = "Start Libation with 1st filter &Default";
			this.firstFilterIsDefaultToolStripMenuItem.Click += new System.EventHandler(this.FirstFilterIsDefaultToolStripMenuItem_Click);
			// 
			// editQuickFiltersToolStripMenuItem
			// 
			this.editQuickFiltersToolStripMenuItem.Name = "editQuickFiltersToolStripMenuItem";
			this.editQuickFiltersToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
			this.editQuickFiltersToolStripMenuItem.Text = "&Edit quick filters...";
			this.editQuickFiltersToolStripMenuItem.Click += new System.EventHandler(this.EditQuickFiltersToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(253, 6);
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accountsToolStripMenuItem,
            this.basicSettingsToolStripMenuItem});
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
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.visibleCountLbl,
            this.springLbl,
            this.backupsCountsLbl,
            this.pdfsCountsLbl});
			this.statusStrip1.Location = new System.Drawing.Point(0, 517);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			this.statusStrip1.Size = new System.Drawing.Size(1007, 22);
			this.statusStrip1.TabIndex = 6;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// visibleCountLbl
			// 
			this.visibleCountLbl.Name = "visibleCountLbl";
			this.visibleCountLbl.Size = new System.Drawing.Size(61, 17);
			this.visibleCountLbl.Text = "Visible: {0}";
			// 
			// springLbl
			// 
			this.springLbl.Name = "springLbl";
			this.springLbl.Size = new System.Drawing.Size(375, 17);
			this.springLbl.Spring = true;
			// 
			// backupsCountsLbl
			// 
			this.backupsCountsLbl.Name = "backupsCountsLbl";
			this.backupsCountsLbl.Size = new System.Drawing.Size(336, 17);
			this.backupsCountsLbl.Text = "BACKUPS: No progress: {0}  Encrypted: {1}  Fully backed up: {2}";
			// 
			// pdfsCountsLbl
			// 
			this.pdfsCountsLbl.Name = "pdfsCountsLbl";
			this.pdfsCountsLbl.Size = new System.Drawing.Size(218, 17);
			this.pdfsCountsLbl.Text = "|  PDFs: NOT d/l\'ed: {0}  Downloaded: {1}";
			// 
			// addFilterBtn
			// 
			this.addFilterBtn.Location = new System.Drawing.Point(47, 31);
			this.addFilterBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.addFilterBtn.Name = "addFilterBtn";
			this.addFilterBtn.Size = new System.Drawing.Size(163, 27);
			this.addFilterBtn.TabIndex = 4;
			this.addFilterBtn.Text = "Add To Quick Filters";
			this.addFilterBtn.UseVisualStyleBackColor = true;
			this.addFilterBtn.Click += new System.EventHandler(this.AddFilterBtn_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1007, 539);
			this.Controls.Add(this.filterBtn);
			this.Controls.Add(this.addFilterBtn);
			this.Controls.Add(this.filterSearchTb);
			this.Controls.Add(this.filterHelpBtn);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.gridPanel);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "Form1";
			this.Text = "Libation: Liberate your Library";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel gridPanel;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel springLbl;
		private System.Windows.Forms.ToolStripStatusLabel visibleCountLbl;
		private System.Windows.Forms.ToolStripMenuItem liberateToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel backupsCountsLbl;
		private System.Windows.Forms.ToolStripMenuItem beginBookBackupsToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel pdfsCountsLbl;
		private System.Windows.Forms.ToolStripMenuItem beginPdfBackupsToolStripMenuItem;
		private System.Windows.Forms.TextBox filterSearchTb;
		private System.Windows.Forms.Button filterBtn;
		private System.Windows.Forms.Button filterHelpBtn;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quickFiltersToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem firstFilterIsDefaultToolStripMenuItem;
		private System.Windows.Forms.Button addFilterBtn;
		private System.Windows.Forms.ToolStripMenuItem editQuickFiltersToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem basicSettingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem accountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryOfAllAccountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scanLibraryOfSomeAccountsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem noAccountsYetAddAccountToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportLibraryToolStripMenuItem;
	}
}
