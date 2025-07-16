namespace LibationWinForms.ProcessQueue
{
	partial class ProcessQueueControl
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessQueueControl));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.queueNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.completedNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.errorNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.runningTimeLbl = new System.Windows.Forms.ToolStripStatusLabel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.panel3 = new System.Windows.Forms.Panel();
			this.virtualFlowControl2 = new LibationWinForms.ProcessQueue.VirtualFlowControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new LibationWinForms.ProcessQueue.NumericUpDownSuffix();
			this.btnCleanFinished = new System.Windows.Forms.Button();
			this.cancelAllBtn = new System.Windows.Forms.Button();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.logDGV = new System.Windows.Forms.DataGridView();
			this.timestampColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.logEntryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.logCopyBtn = new System.Windows.Forms.Button();
			this.clearLogBtn = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.tabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.logDGV)).BeginInit();
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
            this.toolStripStatusLabel1,
            this.runningTimeLbl});
			this.statusStrip1.Location = new System.Drawing.Point(0, 483);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(404, 25);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "baseStatusStrip";
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
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(77, 20);
			this.toolStripStatusLabel1.Spring = true;
			// 
			// runningTimeLbl
			// 
			this.runningTimeLbl.AutoSize = false;
			this.runningTimeLbl.Name = "runningTimeLbl";
			this.runningTimeLbl.Size = new System.Drawing.Size(41, 20);
			this.runningTimeLbl.Text = "[TIME]";
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
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.numericUpDown1);
			this.panel1.Controls.Add(this.btnCleanFinished);
			this.panel1.Controls.Add(this.cancelAllBtn);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(3, 427);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(390, 25);
			this.panel1.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(148, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 15);
			this.label1.TabIndex = 5;
			this.label1.Text = "DL Limit:";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.numericUpDown1.DecimalPlaces = 1;
			this.numericUpDown1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDown1.Location = new System.Drawing.Point(208, 0);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(84, 23);
			this.numericUpDown1.Suffix = " MB/s";
			this.numericUpDown1.TabIndex = 4;
			this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numericUpDown1.ThousandsSeparator = true;
			this.numericUpDown1.Value = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
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
			this.btnCleanFinished.Click += new System.EventHandler(this.btnClearFinished_Click);
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
			this.tabPage2.Controls.Add(this.logDGV);
			this.tabPage2.Controls.Add(this.panel4);
			this.tabPage2.Controls.Add(this.panel2);
			this.tabPage2.Location = new System.Drawing.Point(4, 24);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(396, 455);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Log";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// logDGV
			// 
			this.logDGV.AllowUserToAddRows = false;
			this.logDGV.AllowUserToDeleteRows = false;
			this.logDGV.AllowUserToOrderColumns = true;
			this.logDGV.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.logDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.logDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.timestampColumn,
            this.logEntryColumn});
			this.logDGV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logDGV.Location = new System.Drawing.Point(3, 3);
			this.logDGV.Name = "logDGV";
			this.logDGV.RowHeadersVisible = false;
			this.logDGV.RowTemplate.Height = 40;
			this.logDGV.Size = new System.Drawing.Size(390, 419);
			this.logDGV.TabIndex = 3;
			this.logDGV.Resize += new System.EventHandler(this.LogDGV_Resize);
			// 
			// timestampColumn
			// 
			this.timestampColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.timestampColumn.HeaderText = "Timestamp";
			this.timestampColumn.Name = "timestampColumn";
			this.timestampColumn.ReadOnly = true;
			this.timestampColumn.Width = 91;
			// 
			// logEntryColumn
			// 
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.logEntryColumn.DefaultCellStyle = dataGridViewCellStyle1;
			this.logEntryColumn.HeaderText = "Log";
			this.logEntryColumn.Name = "logEntryColumn";
			this.logEntryColumn.ReadOnly = true;
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
			this.panel2.Controls.Add(this.logCopyBtn);
			this.panel2.Controls.Add(this.clearLogBtn);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(3, 427);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(390, 25);
			this.panel2.TabIndex = 1;
			// 
			// logCopyBtn
			// 
			this.logCopyBtn.Dock = System.Windows.Forms.DockStyle.Left;
			this.logCopyBtn.Location = new System.Drawing.Point(0, 0);
			this.logCopyBtn.Name = "logCopyBtn";
			this.logCopyBtn.Size = new System.Drawing.Size(57, 23);
			this.logCopyBtn.TabIndex = 1;
			this.logCopyBtn.Text = "Copy";
			this.logCopyBtn.UseVisualStyleBackColor = true;
			this.logCopyBtn.Click += new System.EventHandler(this.LogCopyBtn_Click);
			// 
			// clearLogBtn
			// 
			this.clearLogBtn.Dock = System.Windows.Forms.DockStyle.Right;
			this.clearLogBtn.Location = new System.Drawing.Point(328, 0);
			this.clearLogBtn.Name = "clearLogBtn";
			this.clearLogBtn.Size = new System.Drawing.Size(60, 23);
			this.clearLogBtn.TabIndex = 0;
			this.clearLogBtn.Text = "Clear";
			this.clearLogBtn.UseVisualStyleBackColor = true;
			this.clearLogBtn.Click += new System.EventHandler(this.clearLogBtn_Click);
			// 
			// ProcessQueueControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.statusStrip1);
			this.Name = "ProcessQueueControl";
			this.Size = new System.Drawing.Size(404, 508);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.tabPage2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.logDGV)).EndInit();
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
		private System.Windows.Forms.ToolStripStatusLabel runningTimeLbl;
		private System.Windows.Forms.DataGridView logDGV;
		private System.Windows.Forms.DataGridViewTextBoxColumn timestampColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn logEntryColumn;
		private System.Windows.Forms.Button logCopyBtn;
		private NumericUpDownSuffix numericUpDown1;
		private System.Windows.Forms.Label label1;
	}
}
