namespace LibationWinForms.Dialogs
{
	partial class LibationFilesDialog
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
			this.libationFilesDescLbl = new System.Windows.Forms.Label();
			this.libationFilesCustomBtn = new System.Windows.Forms.Button();
			this.libationFilesCustomTb = new System.Windows.Forms.TextBox();
			this.libationFilesCustomRb = new System.Windows.Forms.RadioButton();
			this.libationFilesMyDocsRb = new System.Windows.Forms.RadioButton();
			this.libationFilesRootRb = new System.Windows.Forms.RadioButton();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.saveBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// libationFilesDescLbl
			// 
			this.libationFilesDescLbl.AutoSize = true;
			this.libationFilesDescLbl.Location = new System.Drawing.Point(12, 9);
			this.libationFilesDescLbl.Name = "libationFilesDescLbl";
			this.libationFilesDescLbl.Size = new System.Drawing.Size(36, 13);
			this.libationFilesDescLbl.TabIndex = 0;
			this.libationFilesDescLbl.Text = "[desc]";
			// 
			// libationFilesCustomBtn
			// 
			this.libationFilesCustomBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.libationFilesCustomBtn.Location = new System.Drawing.Point(753, 95);
			this.libationFilesCustomBtn.Name = "libationFilesCustomBtn";
			this.libationFilesCustomBtn.Size = new System.Drawing.Size(35, 23);
			this.libationFilesCustomBtn.TabIndex = 5;
			this.libationFilesCustomBtn.Text = "...";
			this.libationFilesCustomBtn.UseVisualStyleBackColor = true;
			this.libationFilesCustomBtn.Click += new System.EventHandler(this.libationFilesCustomBtn_Click);
			// 
			// libationFilesCustomTb
			// 
			this.libationFilesCustomTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.libationFilesCustomTb.Location = new System.Drawing.Point(35, 97);
			this.libationFilesCustomTb.Name = "libationFilesCustomTb";
			this.libationFilesCustomTb.Size = new System.Drawing.Size(712, 20);
			this.libationFilesCustomTb.TabIndex = 4;
			// 
			// libationFilesCustomRb
			// 
			this.libationFilesCustomRb.AutoSize = true;
			this.libationFilesCustomRb.Location = new System.Drawing.Point(15, 100);
			this.libationFilesCustomRb.Name = "libationFilesCustomRb";
			this.libationFilesCustomRb.Size = new System.Drawing.Size(14, 13);
			this.libationFilesCustomRb.TabIndex = 3;
			this.libationFilesCustomRb.TabStop = true;
			this.libationFilesCustomRb.UseVisualStyleBackColor = true;
			// 
			// libationFilesMyDocsRb
			// 
			this.libationFilesMyDocsRb.AutoSize = true;
			this.libationFilesMyDocsRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.libationFilesMyDocsRb.Location = new System.Drawing.Point(15, 61);
			this.libationFilesMyDocsRb.Name = "libationFilesMyDocsRb";
			this.libationFilesMyDocsRb.Size = new System.Drawing.Size(111, 30);
			this.libationFilesMyDocsRb.TabIndex = 2;
			this.libationFilesMyDocsRb.TabStop = true;
			this.libationFilesMyDocsRb.Text = "[desc]\r\n[myDocs\\Libation]";
			this.libationFilesMyDocsRb.UseVisualStyleBackColor = true;
			// 
			// libationFilesRootRb
			// 
			this.libationFilesRootRb.AutoSize = true;
			this.libationFilesRootRb.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.libationFilesRootRb.Location = new System.Drawing.Point(15, 25);
			this.libationFilesRootRb.Name = "libationFilesRootRb";
			this.libationFilesRootRb.Size = new System.Drawing.Size(113, 30);
			this.libationFilesRootRb.TabIndex = 1;
			this.libationFilesRootRb.TabStop = true;
			this.libationFilesRootRb.Text = "[desc]\r\n[exeRoot\\Libation]";
			this.libationFilesRootRb.UseVisualStyleBackColor = true;
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(713, 124);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 10;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(612, 124);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(75, 23);
			this.saveBtn.TabIndex = 9;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// LibationFilesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 159);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.saveBtn);
			this.Controls.Add(this.libationFilesDescLbl);
			this.Controls.Add(this.libationFilesCustomBtn);
			this.Controls.Add(this.libationFilesCustomTb);
			this.Controls.Add(this.libationFilesRootRb);
			this.Controls.Add(this.libationFilesCustomRb);
			this.Controls.Add(this.libationFilesMyDocsRb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "LibationFilesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Libation Files location";
			this.Load += new System.EventHandler(this.LibationFilesDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label libationFilesDescLbl;
		private System.Windows.Forms.Button libationFilesCustomBtn;
		private System.Windows.Forms.TextBox libationFilesCustomTb;
		private System.Windows.Forms.RadioButton libationFilesCustomRb;
		private System.Windows.Forms.RadioButton libationFilesMyDocsRb;
		private System.Windows.Forms.RadioButton libationFilesRootRb;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button saveBtn;
	}
}