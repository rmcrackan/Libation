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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.warningsLbl = new System.Windows.Forms.Label();
			this.exampleLbl = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(714, 345);
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
			this.cancelBtn.Location = new System.Drawing.Point(832, 345);
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
			this.listView1.Size = new System.Drawing.Size(328, 283);
			this.listView1.TabIndex = 3;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Tag";
			this.columnHeader1.Width = 137;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Description";
			this.columnHeader2.Width = 170;
			// 
			// richTextBox1
			// 
			this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.richTextBox1.Location = new System.Drawing.Point(346, 74);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(574, 185);
			this.richTextBox1.TabIndex = 5;
			this.richTextBox1.Text = "";
			// 
			// warningsLbl
			// 
			this.warningsLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.warningsLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.warningsLbl.ForeColor = System.Drawing.Color.Firebrick;
			this.warningsLbl.Location = new System.Drawing.Point(346, 262);
			this.warningsLbl.Name = "warningsLbl";
			this.warningsLbl.Size = new System.Drawing.Size(574, 77);
			this.warningsLbl.TabIndex = 6;
			this.warningsLbl.Text = "[warnings]";
			// 
			// exampleLbl
			// 
			this.exampleLbl.AutoSize = true;
			this.exampleLbl.Location = new System.Drawing.Point(346, 56);
			this.exampleLbl.Name = "exampleLbl";
			this.exampleLbl.Size = new System.Drawing.Size(55, 15);
			this.exampleLbl.TabIndex = 4;
			this.exampleLbl.Text = "Example:";
			// 
			// EditTemplateDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(933, 388);
			this.Controls.Add(this.exampleLbl);
			this.Controls.Add(this.warningsLbl);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.resetToDefaultBtn);
			this.Controls.Add(this.templateLbl);
			this.Controls.Add(this.templateTb);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
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
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Label warningsLbl;
		private System.Windows.Forms.Label exampleLbl;
	}
}