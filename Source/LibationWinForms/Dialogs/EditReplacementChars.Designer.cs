namespace LibationWinForms.Dialogs
{
	partial class EditReplacementChars
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
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.charToReplaceCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.replacementStringCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.descriptionCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.defaultsBtn = new System.Windows.Forms.Button();
			this.loFiDefaultsBtn = new System.Windows.Forms.Button();
			this.saveBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.charToReplaceCol,
            this.replacementStringCol,
            this.descriptionCol});
			this.dataGridView1.Location = new System.Drawing.Point(12, 12);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowTemplate.Height = 25;
			this.dataGridView1.Size = new System.Drawing.Size(416, 393);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
			this.dataGridView1.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView1_CellValidating);
			this.dataGridView1.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dataGridView1_UserDeletingRow);
			this.dataGridView1.Resize += new System.EventHandler(this.dataGridView1_Resize);
			// 
			// charToReplaceCol
			// 
			this.charToReplaceCol.HeaderText = "Char to Replace";
			this.charToReplaceCol.MinimumWidth = 70;
			this.charToReplaceCol.Name = "charToReplaceCol";
			this.charToReplaceCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.charToReplaceCol.Width = 70;
			// 
			// replacementStringCol
			// 
			this.replacementStringCol.HeaderText = "Replacement String";
			this.replacementStringCol.MinimumWidth = 85;
			this.replacementStringCol.Name = "replacementStringCol";
			this.replacementStringCol.Width = 85;
			// 
			// descriptionCol
			// 
			this.descriptionCol.HeaderText = "Description";
			this.descriptionCol.MinimumWidth = 100;
			this.descriptionCol.Name = "descriptionCol";
			this.descriptionCol.Width = 200;
			// 
			// defaultsBtn
			// 
			this.defaultsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.defaultsBtn.Location = new System.Drawing.Point(12, 430);
			this.defaultsBtn.Name = "defaultsBtn";
			this.defaultsBtn.Size = new System.Drawing.Size(64, 25);
			this.defaultsBtn.TabIndex = 1;
			this.defaultsBtn.Text = "Defaults";
			this.defaultsBtn.UseVisualStyleBackColor = true;
			this.defaultsBtn.Click += new System.EventHandler(this.defaultsBtn_Click);
			// 
			// loFiDefaultsBtn
			// 
			this.loFiDefaultsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.loFiDefaultsBtn.Location = new System.Drawing.Point(82, 430);
			this.loFiDefaultsBtn.Name = "loFiDefaultsBtn";
			this.loFiDefaultsBtn.Size = new System.Drawing.Size(84, 25);
			this.loFiDefaultsBtn.TabIndex = 1;
			this.loFiDefaultsBtn.Text = "LoFi Defaults";
			this.loFiDefaultsBtn.UseVisualStyleBackColor = true;
			this.loFiDefaultsBtn.Click += new System.EventHandler(this.loFiDefaultsBtn_Click);
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(346, 430);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(82, 25);
			this.saveBtn.TabIndex = 1;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.Location = new System.Drawing.Point(258, 430);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(82, 25);
			this.cancelBtn.TabIndex = 1;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// EditReplacementChars
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(440, 467);
			this.Controls.Add(this.loFiDefaultsBtn);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.Controls.Add(this.defaultsBtn);
			this.Controls.Add(this.dataGridView1);
			this.Name = "EditReplacementChars";
			this.Text = "Character Replacements";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn charToReplaceCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn replacementStringCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn descriptionCol;
		private System.Windows.Forms.Button defaultsBtn;
		private System.Windows.Forms.Button loFiDefaultsBtn;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
	}
}