
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
			this.knownDirectoryRb = new System.Windows.Forms.RadioButton();
			this.customDirectoryRb = new System.Windows.Forms.RadioButton();
			this.customTb = new System.Windows.Forms.TextBox();
			this.customBtn = new System.Windows.Forms.Button();
			this.directorySelectControl = new LibationWinForms.Dialogs.DirectorySelectControl();
			this.SuspendLayout();
			// 
			// knownDirectoryRb
			// 
			this.knownDirectoryRb.AutoSize = true;
			this.knownDirectoryRb.Location = new System.Drawing.Point(3, 3);
			this.knownDirectoryRb.Name = "knownDirectoryRb";
			this.knownDirectoryRb.Size = new System.Drawing.Size(14, 13);
			this.knownDirectoryRb.TabIndex = 0;
			this.knownDirectoryRb.UseVisualStyleBackColor = true;
			this.knownDirectoryRb.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// customDirectoryRb
			// 
			this.customDirectoryRb.AutoSize = true;
			this.customDirectoryRb.Location = new System.Drawing.Point(2, 62);
			this.customDirectoryRb.Name = "customDirectoryRb";
			this.customDirectoryRb.Size = new System.Drawing.Size(14, 13);
			this.customDirectoryRb.TabIndex = 2;
			this.customDirectoryRb.UseVisualStyleBackColor = true;
			this.customDirectoryRb.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// customTb
			// 
			this.customTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.customTb.Location = new System.Drawing.Point(22, 58);
			this.customTb.Name = "customTb";
			this.customTb.Size = new System.Drawing.Size(588, 23);
			this.customTb.TabIndex = 3;
			// 
			// customBtn
			// 
			this.customBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.customBtn.Location = new System.Drawing.Point(616, 58);
			this.customBtn.Name = "customBtn";
			this.customBtn.Size = new System.Drawing.Size(41, 27);
			this.customBtn.TabIndex = 4;
			this.customBtn.Text = "...";
			this.customBtn.UseVisualStyleBackColor = true;
			this.customBtn.Click += new System.EventHandler(this.customBtn_Click);
			// 
			// directorySelectControl
			// 
			this.directorySelectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.directorySelectControl.Location = new System.Drawing.Point(23, 0);
			this.directorySelectControl.Name = "directorySelectControl";
			this.directorySelectControl.Size = new System.Drawing.Size(635, 52);
			this.directorySelectControl.TabIndex = 5;
			// 
			// DirectoryOrCustomSelectControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.directorySelectControl);
			this.Controls.Add(this.customBtn);
			this.Controls.Add(this.customTb);
			this.Controls.Add(this.customDirectoryRb);
			this.Controls.Add(this.knownDirectoryRb);
			this.Name = "DirectoryOrCustomSelectControl";
			this.Size = new System.Drawing.Size(660, 87);
			this.Load += new System.EventHandler(this.DirectoryOrCustomSelectControl_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton knownDirectoryRb;
		private System.Windows.Forms.RadioButton customDirectoryRb;
		private System.Windows.Forms.TextBox customTb;
		private System.Windows.Forms.Button customBtn;
		private DirectorySelectControl directorySelectControl;
	}
}
