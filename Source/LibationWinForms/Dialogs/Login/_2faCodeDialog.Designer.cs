namespace LibationWinForms.Dialogs.Login
{
	partial class _2faCodeDialog
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
			submitBtn = new System.Windows.Forms.Button();
			codeTb = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			promptLbl = new System.Windows.Forms.Label();
			SuspendLayout();
			// 
			// submitBtn
			// 
			submitBtn.Location = new System.Drawing.Point(18, 108);
			submitBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			submitBtn.Name = "submitBtn";
			submitBtn.Size = new System.Drawing.Size(191, 27);
			submitBtn.TabIndex = 1;
			submitBtn.Text = "Submit";
			submitBtn.UseVisualStyleBackColor = true;
			submitBtn.Click += submitBtn_Click;
			// 
			// codeTb
			// 
			codeTb.Location = new System.Drawing.Point(108, 79);
			codeTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			codeTb.Name = "codeTb";
			codeTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			codeTb.Size = new System.Drawing.Size(101, 23);
			codeTb.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(13, 82);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(87, 15);
			label1.TabIndex = 2;
			label1.Text = "Enter 2FA Code";
			// 
			// promptLbl
			// 
			promptLbl.Location = new System.Drawing.Point(13, 9);
			promptLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			promptLbl.Name = "promptLbl";
			promptLbl.Size = new System.Drawing.Size(196, 59);
			promptLbl.TabIndex = 2;
			promptLbl.Text = "[Prompt]";
			// 
			// _2faCodeDialog
			// 
			AcceptButton = submitBtn;
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(222, 147);
			Controls.Add(promptLbl);
			Controls.Add(label1);
			Controls.Add(codeTb);
			Controls.Add(submitBtn);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "_2faCodeDialog";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "2FA Code";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.TextBox codeTb;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label promptLbl;
	}
}