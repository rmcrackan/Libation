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
			this.passwordLbl = new System.Windows.Forms.Label();
			this.passwordTb = new System.Windows.Forms.TextBox();
			this.submitBtn = new System.Windows.Forms.Button();
			this.localeLbl = new System.Windows.Forms.Label();
			this.usernameLbl = new System.Windows.Forms.Label();
			this.externalLoginLink = new System.Windows.Forms.LinkLabel();
			this.externalLoginLbl2 = new System.Windows.Forms.Label();
			this.externalLoginLbl1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// passwordLbl
			// 
			this.passwordLbl.AutoSize = true;
			this.passwordLbl.Location = new System.Drawing.Point(14, 47);
			this.passwordLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.passwordLbl.Name = "passwordLbl";
			this.passwordLbl.Size = new System.Drawing.Size(57, 15);
			this.passwordLbl.TabIndex = 2;
			this.passwordLbl.Text = "Password";
			// 
			// passwordTb
			// 
			this.passwordTb.Location = new System.Drawing.Point(83, 44);
			this.passwordTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.passwordTb.Name = "passwordTb";
			this.passwordTb.PasswordChar = '*';
			this.passwordTb.Size = new System.Drawing.Size(233, 23);
			this.passwordTb.TabIndex = 3;
			// 
			// submitBtn
			// 
			this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.submitBtn.Location = new System.Drawing.Point(293, 176);
			this.submitBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(88, 27);
			this.submitBtn.TabIndex = 7;
			this.submitBtn.Text = "Submit";
			this.submitBtn.UseVisualStyleBackColor = true;
			this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// localeLbl
			// 
			this.localeLbl.AutoSize = true;
			this.localeLbl.Location = new System.Drawing.Point(14, 10);
			this.localeLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.localeLbl.Name = "localeLbl";
			this.localeLbl.Size = new System.Drawing.Size(61, 15);
			this.localeLbl.TabIndex = 0;
			this.localeLbl.Text = "Locale: {0}";
			// 
			// usernameLbl
			// 
			this.usernameLbl.AutoSize = true;
			this.usernameLbl.Location = new System.Drawing.Point(14, 25);
			this.usernameLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.usernameLbl.Name = "usernameLbl";
			this.usernameLbl.Size = new System.Drawing.Size(80, 15);
			this.usernameLbl.TabIndex = 1;
			this.usernameLbl.Text = "Username: {0}";
			// 
			// externalLoginLink
			// 
			this.externalLoginLink.AutoSize = true;
			this.externalLoginLink.Location = new System.Drawing.Point(14, 93);
			this.externalLoginLink.Name = "externalLoginLink";
			this.externalLoginLink.Size = new System.Drawing.Size(107, 15);
			this.externalLoginLink.TabIndex = 4;
			this.externalLoginLink.TabStop = true;
			this.externalLoginLink.Text = "Or click here to log";
			this.externalLoginLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.externalLoginLink_LinkClicked);
			// 
			// externalLoginLbl2
			// 
			this.externalLoginLbl2.AutoSize = true;
			this.externalLoginLbl2.Location = new System.Drawing.Point(14, 108);
			this.externalLoginLbl2.Name = "externalLoginLbl2";
			this.externalLoginLbl2.Size = new System.Drawing.Size(352, 45);
			this.externalLoginLbl2.TabIndex = 6;
			this.externalLoginLbl2.Text = "This more advanced login is recommended if you\'re experiencing\r\nerrors logging in" +
    " the conventional way above or if you\'re not\r\ncomfortable typing your password h" +
    "ere.";
			// 
			// externalLoginLbl1
			// 
			this.externalLoginLbl1.AutoSize = true;
			this.externalLoginLbl1.Location = new System.Drawing.Point(83, 93);
			this.externalLoginLbl1.Name = "externalLoginLbl1";
			this.externalLoginLbl1.Size = new System.Drawing.Size(158, 15);
			this.externalLoginLbl1.TabIndex = 5;
			this.externalLoginLbl1.Text = "to log in using your browser.";
			// 
			// LoginChoiceEagerDialog
			// 
			this.AcceptButton = this.submitBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(394, 216);
			this.Controls.Add(this.externalLoginLbl2);
			this.Controls.Add(this.externalLoginLbl1);
			this.Controls.Add(this.externalLoginLink);
			this.Controls.Add(this.usernameLbl);
			this.Controls.Add(this.localeLbl);
			this.Controls.Add(this.submitBtn);
			this.Controls.Add(this.passwordLbl);
			this.Controls.Add(this.passwordTb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginChoiceEagerDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Audible Login";
			this.ResumeLayout(false);
			this.PerformLayout();

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