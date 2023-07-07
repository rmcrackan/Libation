namespace LibationWinForms.Dialogs.Login
{
	partial class CaptchaDialog
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
			captchaPb = new System.Windows.Forms.PictureBox();
			answerTb = new System.Windows.Forms.TextBox();
			submitBtn = new System.Windows.Forms.Button();
			answerLbl = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			passwordTb = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)captchaPb).BeginInit();
			SuspendLayout();
			// 
			// captchaPb
			// 
			captchaPb.Location = new System.Drawing.Point(13, 14);
			captchaPb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			captchaPb.Name = "captchaPb";
			captchaPb.Size = new System.Drawing.Size(235, 81);
			captchaPb.TabIndex = 0;
			captchaPb.TabStop = false;
			// 
			// answerTb
			// 
			answerTb.Location = new System.Drawing.Point(136, 130);
			answerTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			answerTb.Name = "answerTb";
			answerTb.Size = new System.Drawing.Size(111, 23);
			answerTb.TabIndex = 2;
			// 
			// submitBtn
			// 
			submitBtn.Location = new System.Drawing.Point(159, 171);
			submitBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			submitBtn.Name = "submitBtn";
			submitBtn.Size = new System.Drawing.Size(88, 27);
			submitBtn.TabIndex = 2;
			submitBtn.Text = "Submit";
			submitBtn.UseVisualStyleBackColor = true;
			submitBtn.Click += submitBtn_Click;
			// 
			// answerLbl
			// 
			answerLbl.AutoSize = true;
			answerLbl.Location = new System.Drawing.Point(13, 133);
			answerLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			answerLbl.Name = "answerLbl";
			answerLbl.Size = new System.Drawing.Size(106, 15);
			answerLbl.TabIndex = 0;
			answerLbl.Text = "CAPTCHA answer: ";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(13, 104);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(60, 15);
			label1.TabIndex = 0;
			label1.Text = "Password:";
			// 
			// passwordTb
			// 
			passwordTb.Location = new System.Drawing.Point(81, 101);
			passwordTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			passwordTb.Name = "passwordTb";
			passwordTb.PasswordChar = '*';
			passwordTb.Size = new System.Drawing.Size(167, 23);
			passwordTb.TabIndex = 1;
			// 
			// CaptchaDialog
			// 
			AcceptButton = submitBtn;
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(261, 210);
			Controls.Add(passwordTb);
			Controls.Add(label1);
			Controls.Add(answerLbl);
			Controls.Add(submitBtn);
			Controls.Add(answerTb);
			Controls.Add(captchaPb);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "CaptchaDialog";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "CAPTCHA";
			((System.ComponentModel.ISupportInitialize)captchaPb).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.PictureBox captchaPb;
		private System.Windows.Forms.TextBox answerTb;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Label answerLbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox passwordTb;
	}
}