
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
			directoryComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			directoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			directoryComboBox.FormattingEnabled = true;
			directoryComboBox.Location = new System.Drawing.Point(0, 0);
			directoryComboBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			directoryComboBox.Name = "directoryComboBox";
			directoryComboBox.Size = new System.Drawing.Size(810, 40);
			directoryComboBox.TabIndex = 0;
			directoryComboBox.SelectedIndexChanged += directoryComboBox_SelectedIndexChanged;
			// 
			// textBox1
			// 
			textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textBox1.Location = new System.Drawing.Point(0, 58);
			textBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			textBox1.Name = "textBox1";
			textBox1.ReadOnly = true;
			textBox1.Size = new System.Drawing.Size(810, 39);
			textBox1.TabIndex = 1;
			// 
			// DirectorySelectControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			AutoSize = true;
			Controls.Add(textBox1);
			Controls.Add(directoryComboBox);
			Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			Name = "DirectorySelectControl";
			Size = new System.Drawing.Size(814, 104);
			Load += DirectorySelectControl_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ComboBox directoryComboBox;
		private System.Windows.Forms.TextBox textBox1;
	}
}
