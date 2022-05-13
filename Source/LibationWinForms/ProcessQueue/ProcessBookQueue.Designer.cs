namespace LibationWinForms.ProcessQueue
{
	partial class ProcessBookQueue
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.virtualFlowControl2 = new LibationWinForms.ProcessQueue.VirtualFlowControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnCleanFinished = new System.Windows.Forms.Button();
			this.cancelAllBtn = new System.Windows.Forms.Button();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.panel2 = new System.Windows.Forms.Panel();
			this.clearLogBtn = new System.Windows.Forms.Button();
			this.logMeTbox = new System.Windows.Forms.TextBox();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.statusStrip1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 486);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(404, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(287, 17);
			this.toolStripStatusLabel1.Spring = true;
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(405, 486);
			this.tabControl1.TabIndex = 3;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.virtualFlowControl2);
			this.tabPage1.Controls.Add(this.panel1);
			this.tabPage1.Location = new System.Drawing.Point(4, 24);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(397, 458);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Process Queue";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// virtualFlowControl2
			// 
			this.virtualFlowControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.virtualFlowControl2.Location = new System.Drawing.Point(3, 3);
			this.virtualFlowControl2.Name = "virtualFlowControl2";
			this.virtualFlowControl2.Size = new System.Drawing.Size(391, 422);
			this.virtualFlowControl2.TabIndex = 3;
			this.virtualFlowControl2.VirtualControlCount = 0;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panel1.Controls.Add(this.btnCleanFinished);
			this.panel1.Controls.Add(this.cancelAllBtn);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(3, 425);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(391, 30);
			this.panel1.TabIndex = 2;
			// 
			// btnCleanFinished
			// 
			this.btnCleanFinished.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCleanFinished.Location = new System.Drawing.Point(298, 3);
			this.btnCleanFinished.Name = "btnCleanFinished";
			this.btnCleanFinished.Size = new System.Drawing.Size(90, 23);
			this.btnCleanFinished.TabIndex = 3;
			this.btnCleanFinished.Text = "Clear Finished";
			this.btnCleanFinished.UseVisualStyleBackColor = true;
			this.btnCleanFinished.Click += new System.EventHandler(this.btnCleanFinished_Click);
			// 
			// cancelAllBtn
			// 
			this.cancelAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelAllBtn.Location = new System.Drawing.Point(3, 3);
			this.cancelAllBtn.Name = "cancelAllBtn";
			this.cancelAllBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelAllBtn.TabIndex = 2;
			this.cancelAllBtn.Text = "Cancel All";
			this.cancelAllBtn.UseVisualStyleBackColor = true;
			this.cancelAllBtn.Click += new System.EventHandler(this.cancelAllBtn_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.panel2);
			this.tabPage2.Controls.Add(this.logMeTbox);
			this.tabPage2.Location = new System.Drawing.Point(4, 24);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(397, 458);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Log";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panel2.Controls.Add(this.clearLogBtn);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(3, 425);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(391, 30);
			this.panel2.TabIndex = 1;
			// 
			// clearLogBtn
			// 
			this.clearLogBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.clearLogBtn.Location = new System.Drawing.Point(3, 3);
			this.clearLogBtn.Name = "clearLogBtn";
			this.clearLogBtn.Size = new System.Drawing.Size(75, 23);
			this.clearLogBtn.TabIndex = 0;
			this.clearLogBtn.Text = "Clear Log";
			this.clearLogBtn.UseVisualStyleBackColor = true;
			this.clearLogBtn.Click += new System.EventHandler(this.clearLogBtn_Click);
			// 
			// logMeTbox
			// 
			this.logMeTbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.logMeTbox.Location = new System.Drawing.Point(3, 3);
			this.logMeTbox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.logMeTbox.MaxLength = 10000000;
			this.logMeTbox.Multiline = true;
			this.logMeTbox.Name = "logMeTbox";
			this.logMeTbox.ReadOnly = true;
			this.logMeTbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.logMeTbox.Size = new System.Drawing.Size(346, 419);
			this.logMeTbox.TabIndex = 0;
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 24);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(397, 458);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "tabPage3";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// ProcessBookQueue
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.statusStrip1);
			this.Name = "ProcessBookQueue";
			this.Size = new System.Drawing.Size(404, 508);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TextBox logMeTbox;
		private System.Windows.Forms.Button btnCleanFinished;
		private System.Windows.Forms.Button cancelAllBtn;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button clearLogBtn;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.TabPage tabPage3;
		private VirtualFlowControl virtualFlowControl2;
	}
}
