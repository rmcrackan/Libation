
namespace LibationWinForms.Dialogs
{
	partial class TEMP_TestNewControls
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
			this.button1 = new System.Windows.Forms.Button();
			this.directorySelectControl1 = new LibationWinForms.Dialogs.DirectorySelectControl();
			this.directoryOrCustomSelectControl1 = new LibationWinForms.Dialogs.DirectoryOrCustomSelectControl();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(438, 375);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// directorySelectControl1
			// 
			this.directorySelectControl1.Location = new System.Drawing.Point(30, 54);
			this.directorySelectControl1.Name = "directorySelectControl1";
			this.directorySelectControl1.Size = new System.Drawing.Size(758, 46);
			this.directorySelectControl1.TabIndex = 4;
			// 
			// directoryOrCustomSelectControl1
			// 
			this.directoryOrCustomSelectControl1.Location = new System.Drawing.Point(128, 199);
			this.directoryOrCustomSelectControl1.Name = "directoryOrCustomSelectControl1";
			this.directoryOrCustomSelectControl1.Size = new System.Drawing.Size(660, 81);
			this.directoryOrCustomSelectControl1.TabIndex = 5;
			// 
			// TEMP_TestNewControls
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.directoryOrCustomSelectControl1);
			this.Controls.Add(this.directorySelectControl1);
			this.Controls.Add(this.button1);
			this.Name = "TEMP_TestNewControls";
			this.Text = "TEMP_TestNewControls";
			this.Load += new System.EventHandler(this.TEMP_TestNewControls_Load);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button button1;
		private DirectorySelectControl directorySelectControl1;
		private DirectoryOrCustomSelectControl directoryOrCustomSelectControl1;
	}
}