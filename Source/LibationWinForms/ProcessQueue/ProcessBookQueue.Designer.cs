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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessBookQueue));
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.queueNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.completedNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.errorNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.panel3 = new System.Windows.Forms.Panel();
			this.virtualFlowControl2 = new LibationWinForms.ProcessQueue.VirtualFlowControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnCleanFinished = new System.Windows.Forms.Button();
			this.cancelAllBtn = new System.Windows.Forms.Button();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.clearLogBtn = new System.Windows.Forms.Button();
			this.logMeTbox = new System.Windows.Forms.TextBox();
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
			this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.queueNumberLbl,
            this.completedNumberLbl,
            this.errorNumberLbl,
            this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 483);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(404, 25);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 19);
			// 
			// queueNumberLbl
			// 
			this.queueNumberLbl.Image = ((System.Drawing.Image)(resources.GetObject("queueNumberLbl.Image")));
			this.queueNumberLbl.Name = "queueNumberLbl";
			this.queueNumberLbl.Size = new System.Drawing.Size(51, 20);
			this.queueNumberLbl.Text = "[Q#]";
			// 
			// completedNumberLbl
			// 
			this.completedNumberLbl.Image = ((System.Drawing.Image)(resources.GetObject("completedNumberLbl.Image")));
			this.completedNumberLbl.Name = "completedNumberLbl";
			this.completedNumberLbl.Size = new System.Drawing.Size(56, 20);
			this.completedNumberLbl.Text = "[DL#]";
			// 
			// errorNumberLbl
			// 
			this.errorNumberLbl.Image = ((System.Drawing.Image)(resources.GetObject("errorNumberLbl.Image")));
			this.errorNumberLbl.Name = "errorNumberLbl";
			this.errorNumberLbl.Size = new System.Drawing.Size(62, 20);
			this.errorNumberLbl.Text = "[ERR#]";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 20);
			this.toolStripStatusLabel1.Spring = true;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(404, 483);
			this.tabControl1.TabIndex = 3;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.panel3);
			this.tabPage1.Controls.Add(this.virtualFlowControl2);
			this.tabPage1.Controls.Add(this.panel1);
			this.tabPage1.Location = new System.Drawing.Point(4, 24);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(396, 455);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Process Queue";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// panel3
			// 
			this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new System.Drawing.Point(3, 422);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(390, 5);
			this.panel3.TabIndex = 4;
			// 
			// virtualFlowControl2
			// 
			this.virtualFlowControl2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.virtualFlowControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.virtualFlowControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.virtualFlowControl2.Location = new System.Drawing.Point(3, 3);
			this.virtualFlowControl2.Name = "virtualFlowControl2";
			this.virtualFlowControl2.Size = new System.Drawing.Size(390, 424);
			this.virtualFlowControl2.TabIndex = 3;
			this.virtualFlowControl2.VirtualControlCount = 0;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Control;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.btnCleanFinished);
			this.panel1.Controls.Add(this.cancelAllBtn);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(3, 427);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(390, 25);
			this.panel1.TabIndex = 2;
			// 
			// btnCleanFinished
			// 
			this.btnCleanFinished.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnCleanFinished.Location = new System.Drawing.Point(298, 0);
			this.btnCleanFinished.Name = "btnCleanFinished";
			this.btnCleanFinished.Size = new System.Drawing.Size(90, 23);
			this.btnCleanFinished.TabIndex = 3;
			this.btnCleanFinished.Text = "Clear Finished";
			this.btnCleanFinished.UseVisualStyleBackColor = true;
			this.btnCleanFinished.Click += new System.EventHandler(this.btnCleanFinished_Click);
			// 
			// cancelAllBtn
			// 
			this.cancelAllBtn.Dock = System.Windows.Forms.DockStyle.Left;
			this.cancelAllBtn.Location = new System.Drawing.Point(0, 0);
			this.cancelAllBtn.Name = "cancelAllBtn";
			this.cancelAllBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelAllBtn.TabIndex = 2;
			this.cancelAllBtn.Text = "Cancel All";
			this.cancelAllBtn.UseVisualStyleBackColor = true;
			this.cancelAllBtn.Click += new System.EventHandler(this.cancelAllBtn_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.panel4);
			this.tabPage2.Controls.Add(this.panel2);
			this.tabPage2.Controls.Add(this.logMeTbox);
			this.tabPage2.Location = new System.Drawing.Point(4, 24);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(396, 455);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Log";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// panel4
			// 
			this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel4.Location = new System.Drawing.Point(3, 422);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(390, 5);
			this.panel4.TabIndex = 2;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.SystemColors.Control;
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.clearLogBtn);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(3, 427);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(390, 25);
			this.panel2.TabIndex = 1;
			// 
			// clearLogBtn
			// 
			this.clearLogBtn.Dock = System.Windows.Forms.DockStyle.Left;
			this.clearLogBtn.Location = new System.Drawing.Point(0, 0);
			this.clearLogBtn.Name = "clearLogBtn";
			this.clearLogBtn.Size = new System.Drawing.Size(75, 23);
			this.clearLogBtn.TabIndex = 0;
			this.clearLogBtn.Text = "Clear Log";
			this.clearLogBtn.UseVisualStyleBackColor = true;
			this.clearLogBtn.Click += new System.EventHandler(this.clearLogBtn_Click);
			// 
			// logMeTbox
			// 
			this.logMeTbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logMeTbox.Location = new System.Drawing.Point(3, 3);
			this.logMeTbox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.logMeTbox.MaxLength = 10000000;
			this.logMeTbox.Multiline = true;
			this.logMeTbox.Name = "logMeTbox";
			this.logMeTbox.ReadOnly = true;
			this.logMeTbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.logMeTbox.Size = new System.Drawing.Size(390, 449);
			this.logMeTbox.TabIndex = 0;
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
		private VirtualFlowControl virtualFlowControl2;
		private System.Windows.Forms.ToolStripStatusLabel queueNumberLbl;
		private System.Windows.Forms.ToolStripStatusLabel completedNumberLbl;
		private System.Windows.Forms.ToolStripStatusLabel errorNumberLbl;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
	}
}
