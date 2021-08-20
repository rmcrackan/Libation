namespace LibationWinForms.Dialogs
{
    partial class BookDetailsDialog
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
			this.saveBtn = new System.Windows.Forms.Button();
			this.newTagsTb = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.coverPb = new System.Windows.Forms.PictureBox();
			this.detailsTb = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cancelBtn = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.coverPb)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(481, 247);
			this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 3;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// newTagsTb
			// 
			this.newTagsTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.newTagsTb.Location = new System.Drawing.Point(7, 40);
			this.newTagsTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.newTagsTb.Name = "newTagsTb";
			this.newTagsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.newTagsTb.Size = new System.Drawing.Size(661, 23);
			this.newTagsTb.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 19);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(458, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Tags are separated by a space. Each tag can contain letters, numbers, and undersc" +
    "ores";
			// 
			// coverPb
			// 
			this.coverPb.Location = new System.Drawing.Point(12, 12);
			this.coverPb.Name = "coverPb";
			this.coverPb.Size = new System.Drawing.Size(80, 80);
			this.coverPb.TabIndex = 3;
			this.coverPb.TabStop = false;
			// 
			// detailsTb
			// 
			this.detailsTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.detailsTb.Location = new System.Drawing.Point(98, 12);
			this.detailsTb.Multiline = true;
			this.detailsTb.Name = "detailsTb";
			this.detailsTb.ReadOnly = true;
			this.detailsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.detailsTb.Size = new System.Drawing.Size(589, 151);
			this.detailsTb.TabIndex = 0;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.newTagsTb);
			this.groupBox1.Location = new System.Drawing.Point(12, 169);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(675, 73);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Edit Tags";
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.Location = new System.Drawing.Point(599, 247);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 4;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// BookDetailsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(699, 286);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.detailsTb);
			this.Controls.Add(this.coverPb);
			this.Controls.Add(this.saveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BookDetailsDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Book Details";
			((System.ComponentModel.ISupportInitialize)(this.coverPb)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.TextBox newTagsTb;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox coverPb;
		private System.Windows.Forms.TextBox detailsTb;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cancelBtn;
	}
}