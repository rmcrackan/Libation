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
			libationFilesDescLbl = new System.Windows.Forms.Label();
			cancelBtn = new System.Windows.Forms.Button();
			saveBtn = new System.Windows.Forms.Button();
			libationFilesSelectControl = new DirectoryOrCustomSelectControl();
			SuspendLayout();
			// 
			// libationFilesDescLbl
			// 
			libationFilesDescLbl.AutoSize = true;
			libationFilesDescLbl.Location = new System.Drawing.Point(14, 10);
			libationFilesDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			libationFilesDescLbl.Name = "libationFilesDescLbl";
			libationFilesDescLbl.Size = new System.Drawing.Size(39, 15);
			libationFilesDescLbl.TabIndex = 0;
			libationFilesDescLbl.Text = "[desc]";
			// 
			// cancelBtn
			// 
			cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelBtn.Location = new System.Drawing.Point(832, 119);
			cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			cancelBtn.Name = "cancelBtn";
			cancelBtn.Size = new System.Drawing.Size(88, 27);
			cancelBtn.TabIndex = 3;
			cancelBtn.Text = "Cancel";
			cancelBtn.UseVisualStyleBackColor = true;
			cancelBtn.Click += cancelBtn_Click;
			// 
			// saveBtn
			// 
			saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveBtn.Location = new System.Drawing.Point(736, 119);
			saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			saveBtn.Name = "saveBtn";
			saveBtn.Size = new System.Drawing.Size(88, 27);
			saveBtn.TabIndex = 2;
			saveBtn.Text = "Save";
			saveBtn.UseVisualStyleBackColor = true;
			saveBtn.Click += saveBtn_Click;
			// 
			// libationFilesSelectControl
			// 
			libationFilesSelectControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			libationFilesSelectControl.Location = new System.Drawing.Point(14, 28);
			libationFilesSelectControl.Margin = new System.Windows.Forms.Padding(6);
			libationFilesSelectControl.Name = "libationFilesSelectControl";
			libationFilesSelectControl.Size = new System.Drawing.Size(906, 81);
			libationFilesSelectControl.TabIndex = 1;
			// 
			// LibationFilesDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			AutoSize = true;
			ClientSize = new System.Drawing.Size(933, 158);
			Controls.Add(libationFilesSelectControl);
			Controls.Add(cancelBtn);
			Controls.Add(saveBtn);
			Controls.Add(libationFilesDescLbl);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "LibationFilesDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Libation Files location";
			Load += LibationFilesDialog_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label libationFilesDescLbl;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button saveBtn;
		private DirectoryOrCustomSelectControl libationFilesSelectControl;
	}
}