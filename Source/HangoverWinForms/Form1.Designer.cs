namespace HangoverWinForms
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.databaseTab = new System.Windows.Forms.TabPage();
            this.sqlExecuteBtn = new System.Windows.Forms.Button();
            this.sqlResultsTb = new System.Windows.Forms.TextBox();
            this.sqlTb = new System.Windows.Forms.TextBox();
            this.sqlLbl = new System.Windows.Forms.Label();
            this.databaseFileLbl = new System.Windows.Forms.Label();
            this.deletedTab = new System.Windows.Forms.TabPage();
            this.deletedCheckedLbl = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.saveBtn = new System.Windows.Forms.Button();
            this.uncheckAllBtn = new System.Windows.Forms.Button();
            this.checkAllBtn = new System.Windows.Forms.Button();
            this.deletedCbl = new System.Windows.Forms.CheckedListBox();
            this.cliTab = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.databaseTab.SuspendLayout();
            this.deletedTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.databaseTab);
            this.tabControl1.Controls.Add(this.deletedTab);
            this.tabControl1.Controls.Add(this.cliTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // databaseTab
            // 
            this.databaseTab.Controls.Add(this.sqlExecuteBtn);
            this.databaseTab.Controls.Add(this.sqlResultsTb);
            this.databaseTab.Controls.Add(this.sqlTb);
            this.databaseTab.Controls.Add(this.sqlLbl);
            this.databaseTab.Controls.Add(this.databaseFileLbl);
            this.databaseTab.Location = new System.Drawing.Point(4, 24);
            this.databaseTab.Name = "databaseTab";
            this.databaseTab.Padding = new System.Windows.Forms.Padding(3);
            this.databaseTab.Size = new System.Drawing.Size(792, 422);
            this.databaseTab.TabIndex = 0;
            this.databaseTab.Text = "Database";
            this.databaseTab.UseVisualStyleBackColor = true;
            // 
            // sqlExecuteBtn
            // 
            this.sqlExecuteBtn.Location = new System.Drawing.Point(8, 153);
            this.sqlExecuteBtn.Name = "sqlExecuteBtn";
            this.sqlExecuteBtn.Size = new System.Drawing.Size(75, 23);
            this.sqlExecuteBtn.TabIndex = 3;
            this.sqlExecuteBtn.Text = "Execute";
            this.sqlExecuteBtn.UseVisualStyleBackColor = true;
            this.sqlExecuteBtn.Click += new System.EventHandler(this.sqlExecuteBtn_Click);
            // 
            // sqlResultsTb
            // 
            this.sqlResultsTb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlResultsTb.Location = new System.Drawing.Point(8, 182);
            this.sqlResultsTb.Multiline = true;
            this.sqlResultsTb.Name = "sqlResultsTb";
            this.sqlResultsTb.ReadOnly = true;
            this.sqlResultsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sqlResultsTb.Size = new System.Drawing.Size(776, 234);
            this.sqlResultsTb.TabIndex = 4;
            // 
            // sqlTb
            // 
            this.sqlTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlTb.Location = new System.Drawing.Point(8, 48);
            this.sqlTb.Multiline = true;
            this.sqlTb.Name = "sqlTb";
            this.sqlTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sqlTb.Size = new System.Drawing.Size(778, 99);
            this.sqlTb.TabIndex = 2;
            // 
            // sqlLbl
            // 
            this.sqlLbl.AutoSize = true;
            this.sqlLbl.Location = new System.Drawing.Point(6, 30);
            this.sqlLbl.Name = "sqlLbl";
            this.sqlLbl.Size = new System.Drawing.Size(144, 15);
            this.sqlLbl.TabIndex = 1;
            this.sqlLbl.Text = "SQL (database command)";
            // 
            // databaseFileLbl
            // 
            this.databaseFileLbl.AutoSize = true;
            this.databaseFileLbl.Location = new System.Drawing.Point(6, 3);
            this.databaseFileLbl.Name = "databaseFileLbl";
            this.databaseFileLbl.Size = new System.Drawing.Size(80, 15);
            this.databaseFileLbl.TabIndex = 0;
            this.databaseFileLbl.Text = "Database file: ";
            // 
            // deletedTab
            // 
            this.deletedTab.Controls.Add(this.deletedCheckedLbl);
            this.deletedTab.Controls.Add(this.label1);
            this.deletedTab.Controls.Add(this.saveBtn);
            this.deletedTab.Controls.Add(this.uncheckAllBtn);
            this.deletedTab.Controls.Add(this.checkAllBtn);
            this.deletedTab.Controls.Add(this.deletedCbl);
            this.deletedTab.Location = new System.Drawing.Point(4, 24);
            this.deletedTab.Name = "deletedTab";
            this.deletedTab.Padding = new System.Windows.Forms.Padding(3);
            this.deletedTab.Size = new System.Drawing.Size(792, 422);
            this.deletedTab.TabIndex = 2;
            this.deletedTab.Text = "Deleted Books";
            this.deletedTab.UseVisualStyleBackColor = true;
            // 
            // deletedCheckedLbl
            // 
            this.deletedCheckedLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deletedCheckedLbl.AutoSize = true;
            this.deletedCheckedLbl.Location = new System.Drawing.Point(233, 395);
            this.deletedCheckedLbl.Name = "deletedCheckedLbl";
            this.deletedCheckedLbl.Size = new System.Drawing.Size(104, 15);
            this.deletedCheckedLbl.TabIndex = 6;
            this.deletedCheckedLbl.Text = "Checked: {0} of {1}";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "To restore deleted book, check box and save";
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(709, 391);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 5;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // uncheckAllBtn
            // 
            this.uncheckAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uncheckAllBtn.Location = new System.Drawing.Point(129, 391);
            this.uncheckAllBtn.Name = "uncheckAllBtn";
            this.uncheckAllBtn.Size = new System.Drawing.Size(98, 23);
            this.uncheckAllBtn.TabIndex = 4;
            this.uncheckAllBtn.Text = "Uncheck All";
            this.uncheckAllBtn.UseVisualStyleBackColor = true;
            this.uncheckAllBtn.Click += new System.EventHandler(this.uncheckAllBtn_Click);
            // 
            // checkAllBtn
            // 
            this.checkAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkAllBtn.Location = new System.Drawing.Point(8, 391);
            this.checkAllBtn.Name = "checkAllBtn";
            this.checkAllBtn.Size = new System.Drawing.Size(98, 23);
            this.checkAllBtn.TabIndex = 3;
            this.checkAllBtn.Text = "Check All";
            this.checkAllBtn.UseVisualStyleBackColor = true;
            this.checkAllBtn.Click += new System.EventHandler(this.checkAllBtn_Click);
            // 
            // deletedCbl
            // 
            this.deletedCbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deletedCbl.FormattingEnabled = true;
            this.deletedCbl.Location = new System.Drawing.Point(8, 21);
            this.deletedCbl.Name = "deletedCbl";
            this.deletedCbl.Size = new System.Drawing.Size(776, 364);
            this.deletedCbl.TabIndex = 2;
            this.deletedCbl.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.deletedCbl_ItemCheck);
            // 
            // cliTab
            // 
            this.cliTab.Location = new System.Drawing.Point(4, 24);
            this.cliTab.Name = "cliTab";
            this.cliTab.Size = new System.Drawing.Size(792, 422);
            this.cliTab.TabIndex = 1;
            this.cliTab.Text = "Command Line Interface";
            this.cliTab.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Hangover: Libation debug and recovery tool";
            this.tabControl1.ResumeLayout(false);
            this.databaseTab.ResumeLayout(false);
            this.databaseTab.PerformLayout();
            this.deletedTab.ResumeLayout(false);
            this.deletedTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TabControl tabControl1;
        private TabPage databaseTab;
        private Label databaseFileLbl;
        private TextBox sqlResultsTb;
        private TextBox sqlTb;
        private Label sqlLbl;
        private Button sqlExecuteBtn;
        private TabPage cliTab;
        private TabPage deletedTab;
        private CheckedListBox deletedCbl;
        private Label label1;
        private Button saveBtn;
        private Button uncheckAllBtn;
        private Button checkAllBtn;
        private Label deletedCheckedLbl;
    }
}