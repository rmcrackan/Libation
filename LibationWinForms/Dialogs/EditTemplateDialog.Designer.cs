namespace LibationWinForms.Dialogs
{
	partial class EditTemplateDialog
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
			this.cancelBtn = new System.Windows.Forms.Button();
			this.templateTb = new System.Windows.Forms.TextBox();
			this.templateLbl = new System.Windows.Forms.Label();
			this.resetToDefaultBtn = new System.Windows.Forms.Button();
			this.outputTb = new System.Windows.Forms.TextBox();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(714, 496);
			this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 98;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(832, 496);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 99;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// templateTb
			// 
			this.templateTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.templateTb.Location = new System.Drawing.Point(12, 27);
			this.templateTb.Name = "templateTb";
			this.templateTb.Size = new System.Drawing.Size(779, 23);
			this.templateTb.TabIndex = 1;
			this.templateTb.TextChanged += new System.EventHandler(this.templateTb_TextChanged);
			// 
			// templateLbl
			// 
			this.templateLbl.AutoSize = true;
			this.templateLbl.Location = new System.Drawing.Point(12, 9);
			this.templateLbl.Name = "templateLbl";
			this.templateLbl.Size = new System.Drawing.Size(89, 15);
			this.templateLbl.TabIndex = 0;
			this.templateLbl.Text = "[template desc]";
			// 
			// resetToDefaultBtn
			// 
			this.resetToDefaultBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.resetToDefaultBtn.Location = new System.Drawing.Point(797, 26);
			this.resetToDefaultBtn.Name = "resetToDefaultBtn";
			this.resetToDefaultBtn.Size = new System.Drawing.Size(124, 23);
			this.resetToDefaultBtn.TabIndex = 2;
			this.resetToDefaultBtn.Text = "Reset to default";
			this.resetToDefaultBtn.UseVisualStyleBackColor = true;
			this.resetToDefaultBtn.Click += new System.EventHandler(this.resetToDefaultBtn_Click);
			// 
			// outputTb
			// 
			this.outputTb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputTb.Location = new System.Drawing.Point(346, 56);
			this.outputTb.Multiline = true;
			this.outputTb.Name = "outputTb";
			this.outputTb.ReadOnly = true;
			this.outputTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.outputTb.Size = new System.Drawing.Size(574, 434);
			this.outputTb.TabIndex = 4;
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(12, 56);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(328, 434);
			this.listView1.TabIndex = 100;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Tag";
			this.columnHeader1.Width = 90;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Description";
			this.columnHeader2.Width = 230;
			// 
			// EditTemplateDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(933, 539);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.outputTb);
			this.Controls.Add(this.resetToDefaultBtn);
			this.Controls.Add(this.templateLbl);
			this.Controls.Add(this.templateTb);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "EditTemplateDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Template";
			this.Load += new System.EventHandler(this.EditTemplateDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.TextBox templateTb;
		private System.Windows.Forms.Label templateLbl;
		private System.Windows.Forms.Button resetToDefaultBtn;
		private System.Windows.Forms.TextBox outputTb;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
	}
}