namespace LibationWinForms
{
	partial class ClearableTextBox
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
			textBox1 = new System.Windows.Forms.TextBox();
			button1 = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// textBox1
			// 
			textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textBox1.Location = new System.Drawing.Point(0, 0);
			textBox1.Margin = new System.Windows.Forms.Padding(0);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(625, 23);
			textBox1.TabIndex = 0;
			// 
			// button1
			// 
			button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			button1.Location = new System.Drawing.Point(623, 0);
			button1.Margin = new System.Windows.Forms.Padding(0);
			button1.Name = "button1";
			button1.Size = new System.Drawing.Size(20, 20);
			button1.TabIndex = 1;
			button1.Text = "X";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// ClearableTextBox
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			Controls.Add(button1);
			Controls.Add(textBox1);
			Name = "ClearableTextBox";
			Size = new System.Drawing.Size(642, 20);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button1;
	}
}
