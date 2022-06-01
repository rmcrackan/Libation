namespace Hangover
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
            this.cliTab = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.databaseTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.databaseTab);
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
    }
}