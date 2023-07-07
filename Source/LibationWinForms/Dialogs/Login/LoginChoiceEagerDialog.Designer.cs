namespace LibationWinForms.Dialogs.Login
{
	partial class LoginChoiceEagerDialog
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
			passwordLbl = new System.Windows.Forms.Label();
			passwordTb = new System.Windows.Forms.TextBox();
			submitBtn = new System.Windows.Forms.Button();
			localeLbl = new System.Windows.Forms.Label();
			usernameLbl = new System.Windows.Forms.Label();
			externalLoginLink = new System.Windows.Forms.LinkLabel();
			externalLoginLbl2 = new System.Windows.Forms.Label();
			externalLoginLbl1 = new System.Windows.Forms.Label();
			SuspendLayout();
			// 
			// passwordLbl
			// 
			passwordLbl.AutoSize = true;
			passwordLbl.Location = new System.Drawing.Point(14, 47);
			passwordLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			passwordLbl.Name = "passwordLbl";
			passwordLbl.Size = new System.Drawing.Size(57, 15);
			passwordLbl.TabIndex = 2;
			passwordLbl.Text = "Password";
			// 
			// passwordTb
			// 
			passwordTb.Location = new System.Drawing.Point(83, 44);
			passwordTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			passwordTb.Name = "passwordTb";
			passwordTb.PasswordChar = '*';
			passwordTb.Size = new System.Drawing.Size(233, 23);
			passwordTb.TabIndex = 3;
			// 
			// submitBtn
			// 
			submitBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			submitBtn.Location = new System.Drawing.Point(293, 176);
			submitBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			submitBtn.Name = "submitBtn";
			submitBtn.Size = new System.Drawing.Size(88, 27);
			submitBtn.TabIndex = 7;
			submitBtn.Text = "Submit";
			submitBtn.UseVisualStyleBackColor = true;
			submitBtn.Click += submitBtn_Click;
			// 
			// localeLbl
			// 
			localeLbl.AutoSize = true;
			localeLbl.Location = new System.Drawing.Point(14, 10);
			localeLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			localeLbl.Name = "localeLbl";
			localeLbl.Size = new System.Drawing.Size(61, 15);
			localeLbl.TabIndex = 0;
			localeLbl.Text = "Locale: {0}";
			// 
			// usernameLbl
			// 
			usernameLbl.AutoSize = true;
			usernameLbl.Location = new System.Drawing.Point(14, 25);
			usernameLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			usernameLbl.Name = "usernameLbl";
			usernameLbl.Size = new System.Drawing.Size(80, 15);
			usernameLbl.TabIndex = 1;
			usernameLbl.Text = "Username: {0}";
			// 
			// externalLoginLink
			// 
			externalLoginLink.AutoSize = true;
			externalLoginLink.Location = new System.Drawing.Point(14, 93);
			externalLoginLink.Name = "externalLoginLink";
			externalLoginLink.Size = new System.Drawing.Size(166, 15);
			externalLoginLink.TabIndex = 4;
			externalLoginLink.TabStop = true;
			externalLoginLink.Text = "Trouble Logging in? Click here";
			externalLoginLink.LinkClicked += externalLoginLink_LinkClicked;
			// 
			// externalLoginLbl2
			// 
			externalLoginLbl2.AutoSize = true;
			externalLoginLbl2.Location = new System.Drawing.Point(14, 108);
			externalLoginLbl2.Name = "externalLoginLbl2";
			externalLoginLbl2.Size = new System.Drawing.Size(352, 45);
			externalLoginLbl2.TabIndex = 6;
			externalLoginLbl2.Text = "This more advanced login is recommended if you're experiencing\r\nerrors logging in the conventional way above or if you're not\r\ncomfortable typing your password here.";
			// 
			// externalLoginLbl1
			// 
			externalLoginLbl1.AutoSize = true;
			externalLoginLbl1.Location = new System.Drawing.Point(177, 93);
			externalLoginLbl1.Name = "externalLoginLbl1";
			externalLoginLbl1.Size = new System.Drawing.Size(158, 15);
			externalLoginLbl1.TabIndex = 5;
			externalLoginLbl1.Text = "to log in using your browser.";
			// 
			// LoginChoiceEagerDialog
			// 
			AcceptButton = submitBtn;
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(394, 216);
			Controls.Add(externalLoginLbl2);
			Controls.Add(externalLoginLbl1);
			Controls.Add(externalLoginLink);
			Controls.Add(usernameLbl);
			Controls.Add(localeLbl);
			Controls.Add(submitBtn);
			Controls.Add(passwordLbl);
			Controls.Add(passwordTb);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "LoginChoiceEagerDialog";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Audible Login";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label passwordLbl;
		private System.Windows.Forms.TextBox passwordTb;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Label localeLbl;
		private System.Windows.Forms.Label usernameLbl;
		private System.Windows.Forms.LinkLabel externalLoginLink;
		private System.Windows.Forms.Label externalLoginLbl2;
		private System.Windows.Forms.Label externalLoginLbl1;
	}
}