namespace LibationWinForms.ProcessQueue
{
	partial class SidebarControl
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SidebarControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            queueNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
            completedNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
            errorNumberLbl = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            runningTimeLbl = new System.Windows.Forms.ToolStripStatusLabel();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            panel3 = new System.Windows.Forms.Panel();
            virtualFlowControl2 = new VirtualFlowControl();
            panel1 = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            numericUpDown1 = new NumericUpDownSuffix();
            btnCleanFinished = new System.Windows.Forms.Button();
            cancelAllBtn = new System.Windows.Forms.Button();
            tabPage2 = new System.Windows.Forms.TabPage();
            logDGV = new System.Windows.Forms.DataGridView();
            timestampColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            logEntryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            panel4 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            logCopyBtn = new System.Windows.Forms.Button();
            clearLogBtn = new System.Windows.Forms.Button();
            playlistPage = new System.Windows.Forms.TabPage();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            playlistDataGridView = new LibationWinForms.GridView.DataGridViewEx();
            currentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            seriesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            bookColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            upButton = new System.Windows.Forms.Button();
            downButton = new System.Windows.Forms.Button();
            counterTimer = new System.Windows.Forms.Timer(components);
            statusStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logDGV).BeginInit();
            panel2.SuspendLayout();
            playlistPage.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)playlistDataGridView).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripProgressBar1, queueNumberLbl, completedNumberLbl, errorNumberLbl, toolStripStatusLabel1, runningTimeLbl });
            statusStrip1.Location = new System.Drawing.Point(0, 832);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 24, 0);
            statusStrip1.Size = new System.Drawing.Size(707, 57);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "baseStatusStrip";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new System.Drawing.Size(175, 47);
            // 
            // queueNumberLbl
            // 
            queueNumberLbl.Image = (System.Drawing.Image)resources.GetObject("queueNumberLbl.Image");
            queueNumberLbl.Name = "queueNumberLbl";
            queueNumberLbl.Size = new System.Drawing.Size(110, 48);
            queueNumberLbl.Text = "[Q#]";
            // 
            // completedNumberLbl
            // 
            completedNumberLbl.Image = (System.Drawing.Image)resources.GetObject("completedNumberLbl.Image");
            completedNumberLbl.Name = "completedNumberLbl";
            completedNumberLbl.Size = new System.Drawing.Size(125, 48);
            completedNumberLbl.Text = "[DL#]";
            // 
            // errorNumberLbl
            // 
            errorNumberLbl.Image = (System.Drawing.Image)resources.GetObject("errorNumberLbl.Image");
            errorNumberLbl.Name = "errorNumberLbl";
            errorNumberLbl.Size = new System.Drawing.Size(145, 48);
            errorNumberLbl.Text = "[ERR#]";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(81, 48);
            toolStripStatusLabel1.Spring = true;
            // 
            // runningTimeLbl
            // 
            runningTimeLbl.AutoSize = false;
            runningTimeLbl.Name = "runningTimeLbl";
            runningTimeLbl.Size = new System.Drawing.Size(41, 48);
            runningTimeLbl.Text = "[TIME]";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(playlistPage);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(707, 832);
            tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(panel3);
            tabPage1.Controls.Add(virtualFlowControl2);
            tabPage1.Controls.Add(panel1);
            tabPage1.Location = new System.Drawing.Point(4, 57);
            tabPage1.Margin = new System.Windows.Forms.Padding(5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(5);
            tabPage1.Size = new System.Drawing.Size(699, 771);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Process Queue";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel3.Location = new System.Drawing.Point(5, 715);
            panel3.Margin = new System.Windows.Forms.Padding(5);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(689, 9);
            panel3.TabIndex = 4;
            // 
            // virtualFlowControl2
            // 
            virtualFlowControl2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            virtualFlowControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            virtualFlowControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            virtualFlowControl2.Location = new System.Drawing.Point(5, 5);
            virtualFlowControl2.Margin = new System.Windows.Forms.Padding(5);
            virtualFlowControl2.Name = "virtualFlowControl2";
            virtualFlowControl2.Size = new System.Drawing.Size(689, 719);
            virtualFlowControl2.TabIndex = 3;
            virtualFlowControl2.VirtualControlCount = 0;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.Control;
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(label1);
            panel1.Controls.Add(numericUpDown1);
            panel1.Controls.Add(btnCleanFinished);
            panel1.Controls.Add(cancelAllBtn);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(5, 724);
            panel1.Margin = new System.Windows.Forms.Padding(5);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(689, 42);
            panel1.TabIndex = 2;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(267, 7);
            label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(158, 48);
            label1.TabIndex = 5;
            label1.Text = "DL Limit:";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            numericUpDown1.DecimalPlaces = 1;
            numericUpDown1.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDown1.Location = new System.Drawing.Point(372, 0);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(5);
            numericUpDown1.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(147, 55);
            numericUpDown1.Suffix = " MB/s";
            numericUpDown1.TabIndex = 4;
            numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            numericUpDown1.ThousandsSeparator = true;
            numericUpDown1.Value = new decimal(new int[] { 999, 0, 0, 0 });
            numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // btnCleanFinished
            // 
            btnCleanFinished.Dock = System.Windows.Forms.DockStyle.Right;
            btnCleanFinished.Location = new System.Drawing.Point(529, 0);
            btnCleanFinished.Margin = new System.Windows.Forms.Padding(5);
            btnCleanFinished.Name = "btnCleanFinished";
            btnCleanFinished.Size = new System.Drawing.Size(158, 40);
            btnCleanFinished.TabIndex = 3;
            btnCleanFinished.Text = "Clear Finished";
            btnCleanFinished.UseVisualStyleBackColor = true;
            btnCleanFinished.Click += btnClearFinished_Click;
            // 
            // cancelAllBtn
            // 
            cancelAllBtn.Dock = System.Windows.Forms.DockStyle.Left;
            cancelAllBtn.Location = new System.Drawing.Point(0, 0);
            cancelAllBtn.Margin = new System.Windows.Forms.Padding(5);
            cancelAllBtn.Name = "cancelAllBtn";
            cancelAllBtn.Size = new System.Drawing.Size(131, 40);
            cancelAllBtn.TabIndex = 2;
            cancelAllBtn.Text = "Cancel All";
            cancelAllBtn.UseVisualStyleBackColor = true;
            cancelAllBtn.Click += cancelAllBtn_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(logDGV);
            tabPage2.Controls.Add(panel4);
            tabPage2.Controls.Add(panel2);
            tabPage2.Location = new System.Drawing.Point(4, 57);
            tabPage2.Margin = new System.Windows.Forms.Padding(5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(5);
            tabPage2.Size = new System.Drawing.Size(699, 771);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Log";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // logDGV
            // 
            logDGV.AllowUserToAddRows = false;
            logDGV.AllowUserToDeleteRows = false;
            logDGV.AllowUserToOrderColumns = true;
            logDGV.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            logDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            logDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { timestampColumn, logEntryColumn });
            logDGV.Dock = System.Windows.Forms.DockStyle.Fill;
            logDGV.Location = new System.Drawing.Point(5, 5);
            logDGV.Margin = new System.Windows.Forms.Padding(5);
            logDGV.Name = "logDGV";
            logDGV.RowHeadersVisible = false;
            logDGV.RowHeadersWidth = 72;
            logDGV.RowTemplate.Height = 40;
            logDGV.Size = new System.Drawing.Size(689, 710);
            logDGV.TabIndex = 3;
            logDGV.Resize += LogDGV_Resize;
            // 
            // timestampColumn
            // 
            timestampColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            timestampColumn.HeaderText = "Timestamp";
            timestampColumn.MinimumWidth = 9;
            timestampColumn.Name = "timestampColumn";
            timestampColumn.ReadOnly = true;
            timestampColumn.Width = 234;
            // 
            // logEntryColumn
            // 
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            logEntryColumn.DefaultCellStyle = dataGridViewCellStyle2;
            logEntryColumn.HeaderText = "Log";
            logEntryColumn.MinimumWidth = 9;
            logEntryColumn.Name = "logEntryColumn";
            logEntryColumn.ReadOnly = true;
            logEntryColumn.Width = 175;
            // 
            // panel4
            // 
            panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel4.Location = new System.Drawing.Point(5, 715);
            panel4.Margin = new System.Windows.Forms.Padding(5);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(689, 9);
            panel4.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = System.Drawing.SystemColors.Control;
            panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel2.Controls.Add(logCopyBtn);
            panel2.Controls.Add(clearLogBtn);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(5, 724);
            panel2.Margin = new System.Windows.Forms.Padding(5);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(689, 42);
            panel2.TabIndex = 1;
            // 
            // logCopyBtn
            // 
            logCopyBtn.Dock = System.Windows.Forms.DockStyle.Left;
            logCopyBtn.Location = new System.Drawing.Point(0, 0);
            logCopyBtn.Margin = new System.Windows.Forms.Padding(5);
            logCopyBtn.Name = "logCopyBtn";
            logCopyBtn.Size = new System.Drawing.Size(100, 40);
            logCopyBtn.TabIndex = 1;
            logCopyBtn.Text = "Copy";
            logCopyBtn.UseVisualStyleBackColor = true;
            logCopyBtn.Click += LogCopyBtn_Click;
            // 
            // clearLogBtn
            // 
            clearLogBtn.Dock = System.Windows.Forms.DockStyle.Right;
            clearLogBtn.Location = new System.Drawing.Point(582, 0);
            clearLogBtn.Margin = new System.Windows.Forms.Padding(5);
            clearLogBtn.Name = "clearLogBtn";
            clearLogBtn.Size = new System.Drawing.Size(105, 40);
            clearLogBtn.TabIndex = 0;
            clearLogBtn.Text = "Clear";
            clearLogBtn.UseVisualStyleBackColor = true;
            clearLogBtn.Click += clearLogBtn_Click;
            // 
            // playlistPage
            // 
            playlistPage.Controls.Add(tableLayoutPanel1);
            playlistPage.Location = new System.Drawing.Point(4, 57);
            playlistPage.Name = "playlistPage";
            playlistPage.Padding = new System.Windows.Forms.Padding(3);
            playlistPage.Size = new System.Drawing.Size(699, 771);
            playlistPage.TabIndex = 2;
            playlistPage.Text = "Playlist";
            playlistPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(playlistDataGridView, 0, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(693, 765);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // playlistDataGridView
            // 
            playlistDataGridView.AllowUserToAddRows = false;
            playlistDataGridView.AllowUserToDeleteRows = false;
            playlistDataGridView.AllowUserToOrderColumns = true;
            playlistDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            playlistDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            playlistDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            playlistDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { currentColumn, seriesColumn, bookColumn });
            playlistDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            playlistDataGridView.Location = new System.Drawing.Point(1, 1);
            playlistDataGridView.Margin = new System.Windows.Forms.Padding(0);
            playlistDataGridView.Name = "playlistDataGridView";
            playlistDataGridView.RowHeadersVisible = false;
            playlistDataGridView.RowHeadersWidth = 72;
            playlistDataGridView.RowTemplate.Height = 40;
            playlistDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            playlistDataGridView.Size = new System.Drawing.Size(584, 763);
            playlistDataGridView.StandardTab = true;
            playlistDataGridView.TabIndex = 5;
            // 
            // currentColumn
            // 
            currentColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            currentColumn.DataPropertyName = "IsCurrentStr";
            currentColumn.Frozen = true;
            currentColumn.HeaderText = " ";
            currentColumn.MinimumWidth = 45;
            currentColumn.Name = "currentColumn";
            currentColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            currentColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            currentColumn.Width = 45;
            // 
            // seriesColumn
            // 
            seriesColumn.DataPropertyName = "Series";
            seriesColumn.HeaderText = "Series";
            seriesColumn.MinimumWidth = 9;
            seriesColumn.Name = "seriesColumn";
            seriesColumn.ReadOnly = true;
            seriesColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            seriesColumn.Width = 250;
            // 
            // bookColumn
            // 
            bookColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            bookColumn.DataPropertyName = "Title";
            bookColumn.HeaderText = "Title";
            bookColumn.MinimumWidth = 250;
            bookColumn.Name = "bookColumn";
            bookColumn.ReadOnly = true;
            bookColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            bookColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            bookColumn.Width = 250;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(upButton);
            flowLayoutPanel1.Controls.Add(downButton);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanel1.Location = new System.Drawing.Point(589, 4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            flowLayoutPanel1.Size = new System.Drawing.Size(100, 757);
            flowLayoutPanel1.TabIndex = 6;
            // 
            // upButton
            // 
            upButton.Location = new System.Drawing.Point(8, 3);
            upButton.Name = "upButton";
            upButton.Size = new System.Drawing.Size(80, 80);
            upButton.TabIndex = 0;
            upButton.Text = "▲";
            upButton.UseVisualStyleBackColor = true;
            // 
            // downButton
            // 
            downButton.Location = new System.Drawing.Point(8, 89);
            downButton.Name = "downButton";
            downButton.Size = new System.Drawing.Size(80, 80);
            downButton.TabIndex = 1;
            downButton.Text = "▼";
            downButton.UseVisualStyleBackColor = true;
            // 
            // counterTimer
            // 
            counterTimer.Interval = 950;
            counterTimer.Tick += CounterTimer_Tick;
            // 
            // SidebarControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            Margin = new System.Windows.Forms.Padding(5);
            Name = "SidebarControl";
            Size = new System.Drawing.Size(707, 889);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)logDGV).EndInit();
            panel2.ResumeLayout(false);
            playlistPage.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)playlistDataGridView).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
		private System.Windows.Forms.Timer counterTimer;
		private System.Windows.Forms.DataGridView logDGV;
		private System.Windows.Forms.DataGridViewTextBoxColumn timestampColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn logEntryColumn;
		private System.Windows.Forms.Button logCopyBtn;
		private NumericUpDownSuffix numericUpDown1;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage playlistPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private LibationWinForms.GridView.DataGridViewEx playlistDataGridView;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn seriesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn bookColumn;
    }
}
