
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
			directoryComboBox = new System.Windows.Forms.ComboBox();
			textBox1 = new System.Windows.Forms.TextBox();
			SuspendLayout();
			// 
			// directoryComboBox
			// 
			directoryComboBox.Dock = System.Windows.Forms.DockStyle.Top;
			directoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			directoryComboBox.FormattingEnabled = true;
			directoryComboBox.Location = new System.Drawing.Point(0, 0);
			directoryComboBox.Name = "directoryComboBox";
			directoryComboBox.Size = new System.Drawing.Size(814, 23);
			directoryComboBox.TabIndex = 0;
			directoryComboBox.SelectedIndexChanged += directoryComboBox_SelectedIndexChanged;
			// 
			// textBox1
			// 
			textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
			textBox1.Location = new System.Drawing.Point(0, 26);
			textBox1.Name = "textBox1";
			textBox1.ReadOnly = true;
			textBox1.Size = new System.Drawing.Size(814, 23);
			textBox1.TabIndex = 1;
			// 
			// DirectorySelectControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			Controls.Add(textBox1);
			Controls.Add(directoryComboBox);
			Name = "DirectorySelectControl";
			Size = new System.Drawing.Size(814, 49);
			Load += DirectorySelectControl_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ComboBox directoryComboBox;
		private System.Windows.Forms.TextBox textBox1;
	}
}
