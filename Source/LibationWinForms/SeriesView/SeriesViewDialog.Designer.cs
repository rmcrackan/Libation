using System.Windows.Forms;

namespace LibationWinForms.SeriesView
{
	partial class SeriesViewDialog
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
			tabControl1 = new TabControl();
			SuspendLayout();
			// 
			// tabControl1
			// 
			tabControl1.Dock = DockStyle.Fill;
			tabControl1.Location = new System.Drawing.Point(0, 0);
			tabControl1.Name = "tabControl1";
			tabControl1.SelectedIndex = 0;
			tabControl1.Size = new System.Drawing.Size(800, 450);
			tabControl1.TabIndex = 0;
			// 
			// SeriesViewDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(tabControl1);
			FormBorderStyle = FormBorderStyle.SizableToolWindow;
			Name = "SeriesViewDialog";
			StartPosition = FormStartPosition.CenterParent;
			Text = "View All Items in Series";
			ResumeLayout(false);
		}

		private TabControl tabControl1;

		#endregion
	}
}