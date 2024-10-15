namespace LibationWinForms.Dialogs
{
    partial class EditQuickFilters
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
            cancelBtn = new System.Windows.Forms.Button();
            saveBtn = new System.Windows.Forms.Button();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            Original = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Delete = new DisableButtonColumn();
            FilterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Filter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            MoveUp = new DisableButtonColumn();
            MoveDown = new DisableButtonColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // cancelBtn
            // 
            cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelBtn.Location = new System.Drawing.Point(1248, 726);
            cancelBtn.Margin = new System.Windows.Forms.Padding(5);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new System.Drawing.Size(131, 40);
            cancelBtn.TabIndex = 2;
            cancelBtn.Text = "Cancel";
            cancelBtn.UseVisualStyleBackColor = true;
            cancelBtn.Click += cancelBtn_Click;
            // 
            // saveBtn
            // 
            saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            saveBtn.Location = new System.Drawing.Point(1071, 726);
            saveBtn.Margin = new System.Windows.Forms.Padding(5);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new System.Drawing.Size(131, 40);
            saveBtn.TabIndex = 1;
            saveBtn.Text = "Save";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += saveBtn_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Original, Delete, FilterName, Filter, MoveUp, MoveDown });
            dataGridView1.Location = new System.Drawing.Point(21, 21);
            dataGridView1.Margin = new System.Windows.Forms.Padding(5);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 72;
            dataGridView1.Size = new System.Drawing.Size(1358, 695);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            dataGridView1.DefaultValuesNeeded += dataGridView1_DefaultValuesNeeded;
            // 
            // Original
            // 
            Original.HeaderText = "Original";
            Original.MinimumWidth = 9;
            Original.Name = "Original";
            Original.ReadOnly = true;
            Original.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Original.Visible = false;
            Original.Width = 150;
            // 
            // Delete
            // 
            Delete.HeaderText = "Delete";
            Delete.MinimumWidth = 9;
            Delete.Name = "Delete";
            Delete.ReadOnly = true;
            Delete.Text = "x";
            Delete.Width = 127;
            // 
            // FilterName
            // 
            FilterName.HeaderText = "Name";
            FilterName.MinimumWidth = 300;
            FilterName.Name = "FilterName";
            FilterName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            FilterName.Width = 300;
            // 
            // Filter
            // 
            Filter.HeaderText = "Filter";
            Filter.MinimumWidth = 400;
            Filter.Name = "Filter";
            Filter.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Filter.Width = 400;
            // 
            // MoveUp
            // 
            MoveUp.HeaderText = "Move Up";
            MoveUp.MinimumWidth = 9;
            MoveUp.Name = "MoveUp";
            MoveUp.ReadOnly = true;
            MoveUp.Text = "^";
            MoveUp.Width = 169;
            // 
            // MoveDown
            // 
            MoveDown.HeaderText = "Move Down";
            MoveDown.MinimumWidth = 9;
            MoveDown.Name = "MoveDown";
            MoveDown.ReadOnly = true;
            MoveDown.Text = "v";
            MoveDown.Width = 215;
            // 
            // EditQuickFilters
            // 
            AcceptButton = saveBtn;
            AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = cancelBtn;
            ClientSize = new System.Drawing.Size(1400, 788);
            Controls.Add(dataGridView1);
            Controls.Add(cancelBtn);
            Controls.Add(saveBtn);
            Margin = new System.Windows.Forms.Padding(5);
            Name = "EditQuickFilters";
            Text = "Edit Quick Filters";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Original;
        private DisableButtonColumn Delete;
        private System.Windows.Forms.DataGridViewTextBoxColumn FilterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Filter;
        private DisableButtonColumn MoveUp;
        private DisableButtonColumn MoveDown;
    }
}