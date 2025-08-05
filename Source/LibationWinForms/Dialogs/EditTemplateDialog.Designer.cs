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
			saveBtn = new System.Windows.Forms.Button();
			cancelBtn = new System.Windows.Forms.Button();
			templateTb = new System.Windows.Forms.TextBox();
			templateLbl = new System.Windows.Forms.Label();
			resetToDefaultBtn = new System.Windows.Forms.Button();
			listView1 = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			richTextBox1 = new System.Windows.Forms.RichTextBox();
			warningsLbl = new System.Windows.Forms.Label();
			exampleLbl = new System.Windows.Forms.Label();
			llblGoToWiki = new System.Windows.Forms.LinkLabel();
			SuspendLayout();
			// 
			// saveBtn
			// 
			saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveBtn.Location = new System.Drawing.Point(714, 345);
			saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			saveBtn.Name = "saveBtn";
			saveBtn.Size = new System.Drawing.Size(88, 27);
			saveBtn.TabIndex = 98;
			saveBtn.Text = "Save";
			saveBtn.UseVisualStyleBackColor = true;
			saveBtn.Click += saveBtn_Click;
			// 
			// cancelBtn
			// 
			cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelBtn.Location = new System.Drawing.Point(832, 345);
			cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cancelBtn.Name = "cancelBtn";
			cancelBtn.Size = new System.Drawing.Size(88, 27);
			cancelBtn.TabIndex = 99;
			cancelBtn.Text = "Cancel";
			cancelBtn.UseVisualStyleBackColor = true;
			cancelBtn.Click += cancelBtn_Click;
			// 
			// templateTb
			// 
			templateTb.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			templateTb.Location = new System.Drawing.Point(12, 27);
			templateTb.Name = "templateTb";
			templateTb.Size = new System.Drawing.Size(779, 23);
			templateTb.TabIndex = 1;
			templateTb.TextChanged += templateTb_TextChanged;
			// 
			// templateLbl
			// 
			templateLbl.AutoSize = true;
			templateLbl.Location = new System.Drawing.Point(12, 9);
			templateLbl.Name = "templateLbl";
			templateLbl.Size = new System.Drawing.Size(89, 15);
			templateLbl.TabIndex = 0;
			templateLbl.Text = "[template desc]";
			// 
			// resetToDefaultBtn
			// 
			resetToDefaultBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			resetToDefaultBtn.Location = new System.Drawing.Point(797, 26);
			resetToDefaultBtn.Name = "resetToDefaultBtn";
			resetToDefaultBtn.Size = new System.Drawing.Size(124, 23);
			resetToDefaultBtn.TabIndex = 2;
			resetToDefaultBtn.Text = "Reset to default";
			resetToDefaultBtn.UseVisualStyleBackColor = true;
			resetToDefaultBtn.Click += resetToDefaultBtn_Click;
			// 
			// listView1
			// 
			listView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader2 });
			listView1.FullRowSelect = true;
			listView1.GridLines = true;
			listView1.Location = new System.Drawing.Point(12, 56);
			listView1.MultiSelect = false;
			listView1.Name = "listView1";
			listView1.Size = new System.Drawing.Size(328, 283);
			listView1.TabIndex = 3;
			listView1.UseCompatibleStateImageBehavior = false;
			listView1.View = System.Windows.Forms.View.Details;
			listView1.DoubleClick += listView1_DoubleClick;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Tag";
			columnHeader1.Width = 137;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Description";
			columnHeader2.Width = 170;
			// 
			// richTextBox1
			// 
			richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			richTextBox1.Location = new System.Drawing.Point(346, 74);
			richTextBox1.Name = "richTextBox1";
			richTextBox1.ReadOnly = true;
			richTextBox1.Size = new System.Drawing.Size(574, 185);
			richTextBox1.TabIndex = 5;
			richTextBox1.Text = "";
			// 
			// warningsLbl
			// 
			warningsLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			warningsLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			warningsLbl.ForeColor = System.Drawing.Color.Firebrick;
			warningsLbl.Location = new System.Drawing.Point(346, 262);
			warningsLbl.Name = "warningsLbl";
			warningsLbl.Size = new System.Drawing.Size(574, 77);
			warningsLbl.TabIndex = 6;
			warningsLbl.Text = "[warnings]";
			// 
			// exampleLbl
			// 
			exampleLbl.AutoSize = true;
			exampleLbl.Location = new System.Drawing.Point(346, 56);
			exampleLbl.Name = "exampleLbl";
			exampleLbl.Size = new System.Drawing.Size(54, 15);
			exampleLbl.TabIndex = 4;
			exampleLbl.Text = "Example:";
			// 
			// llblGoToWiki
			// 
			llblGoToWiki.AutoSize = true;
			llblGoToWiki.Location = new System.Drawing.Point(12, 357);
			llblGoToWiki.Name = "llblGoToWiki";
			llblGoToWiki.Size = new System.Drawing.Size(229, 15);
			llblGoToWiki.TabIndex = 100;
			llblGoToWiki.TabStop = true;
			llblGoToWiki.Text = "Read about naming templates on the Wiki";
			llblGoToWiki.LinkClicked += llblGoToWiki_LinkClicked;
			// 
			// EditTemplateDialog
			// 
			AcceptButton = saveBtn;
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			CancelButton = cancelBtn;
			ClientSize = new System.Drawing.Size(933, 388);
			Controls.Add(llblGoToWiki);
			Controls.Add(exampleLbl);
			Controls.Add(warningsLbl);
			Controls.Add(richTextBox1);
			Controls.Add(listView1);
			Controls.Add(resetToDefaultBtn);
			Controls.Add(templateLbl);
			Controls.Add(templateTb);
			Controls.Add(cancelBtn);
			Controls.Add(saveBtn);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "EditTemplateDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Edit Template";
			Load += EditTemplateDialog_Load;
			ResumeLayout(false);
			PerformLayout();

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
		private System.Windows.Forms.LinkLabel llblGoToWiki;
	}
}