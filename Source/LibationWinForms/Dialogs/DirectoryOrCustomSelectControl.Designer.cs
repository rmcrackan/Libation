
namespace LibationWinForms.Dialogs
{
	partial class DirectoryOrCustomSelectControl
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
			knownDirectoryRb = new System.Windows.Forms.RadioButton();
			customDirectoryRb = new System.Windows.Forms.RadioButton();
			customTb = new System.Windows.Forms.TextBox();
			customBtn = new System.Windows.Forms.Button();
			directorySelectControl = new DirectorySelectControl();
			SuspendLayout();
			// 
			// knownDirectoryRb
			// 
			knownDirectoryRb.AutoSize = true;
			knownDirectoryRb.Location = new System.Drawing.Point(6, 6);
			knownDirectoryRb.Margin = new System.Windows.Forms.Padding(6);
			knownDirectoryRb.Name = "knownDirectoryRb";
			knownDirectoryRb.Size = new System.Drawing.Size(27, 26);
			knownDirectoryRb.TabIndex = 0;
			knownDirectoryRb.UseVisualStyleBackColor = true;
			knownDirectoryRb.CheckedChanged += radioButton_CheckedChanged;
			// 
			// customDirectoryRb
			// 
			customDirectoryRb.AutoSize = true;
			customDirectoryRb.Location = new System.Drawing.Point(4, 124);
			customDirectoryRb.Margin = new System.Windows.Forms.Padding(6);
			customDirectoryRb.Name = "customDirectoryRb";
			customDirectoryRb.Size = new System.Drawing.Size(27, 26);
			customDirectoryRb.TabIndex = 2;
			customDirectoryRb.UseVisualStyleBackColor = true;
			customDirectoryRb.CheckedChanged += radioButton_CheckedChanged;
			// 
			// customTb
			// 
			customTb.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			customTb.Location = new System.Drawing.Point(44, 116);
			customTb.Margin = new System.Windows.Forms.Padding(6);
			customTb.Name = "customTb";
			customTb.Size = new System.Drawing.Size(1172, 39);
			customTb.TabIndex = 3;
			// 
			// customBtn
			// 
			customBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			customBtn.Location = new System.Drawing.Point(1232, 116);
			customBtn.Margin = new System.Windows.Forms.Padding(6);
			customBtn.Name = "customBtn";
			customBtn.Size = new System.Drawing.Size(82, 54);
			customBtn.TabIndex = 4;
			customBtn.Text = "...";
			customBtn.UseVisualStyleBackColor = true;
			customBtn.Click += customBtn_Click;
			// 
			// directorySelectControl
			// 
			directorySelectControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			directorySelectControl.AutoSize = true;
			directorySelectControl.Location = new System.Drawing.Point(46, 0);
			directorySelectControl.Margin = new System.Windows.Forms.Padding(12);
			directorySelectControl.Name = "directorySelectControl";
			directorySelectControl.Size = new System.Drawing.Size(1270, 104);
			directorySelectControl.TabIndex = 5;
			// 
			// DirectoryOrCustomSelectControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			Controls.Add(directorySelectControl);
			Controls.Add(customBtn);
			Controls.Add(customTb);
			Controls.Add(customDirectoryRb);
			Controls.Add(knownDirectoryRb);
			Margin = new System.Windows.Forms.Padding(6);
			Name = "DirectoryOrCustomSelectControl";
			Size = new System.Drawing.Size(1320, 176);
			Load += DirectoryOrCustomSelectControl_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.RadioButton knownDirectoryRb;
		private System.Windows.Forms.RadioButton customDirectoryRb;
		private System.Windows.Forms.TextBox customTb;
		private System.Windows.Forms.Button customBtn;
		private DirectorySelectControl directorySelectControl;
	}
}
