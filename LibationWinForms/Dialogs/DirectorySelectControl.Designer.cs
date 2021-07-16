
namespace LibationWinForms.Dialogs
{
	partial class DirectorySelectControl
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
			this.directoryComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// directoryComboBox
			// 
			this.directoryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.directoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.directoryComboBox.FormattingEnabled = true;
			this.directoryComboBox.Location = new System.Drawing.Point(0, 0);
			this.directoryComboBox.Name = "directoryComboBox";
			this.directoryComboBox.Size = new System.Drawing.Size(647, 23);
			this.directoryComboBox.TabIndex = 0;
			this.directoryComboBox.SelectedIndexChanged += new System.EventHandler(this.directoryComboBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(97, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select a directory";
			// 
			// DirectorySelectControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.directoryComboBox);
			this.Name = "DirectorySelectControl";
			this.Size = new System.Drawing.Size(647, 46);
			this.Load += new System.EventHandler(this.DirectorySelectControl_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox directoryComboBox;
		private System.Windows.Forms.Label label1;
	}
}
