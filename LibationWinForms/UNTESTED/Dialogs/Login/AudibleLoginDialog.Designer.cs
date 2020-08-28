namespace LibationWinForms.Dialogs.Login
{
	partial class AudibleLoginDialog
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
			this.SuspendLayout();
			// 
			// passwordLbl
			// 
			this.passwordLbl.AutoSize = true;
			this.passwordLbl.Location = new System.Drawing.Point(12, 41);
			this.passwordLbl.Name = "passwordLbl";
			this.passwordLbl.Size = new System.Drawing.Size(53, 13);
			this.passwordLbl.TabIndex = 2;
			this.passwordLbl.Text = "Password";
			// 
			// passwordTb
			// 
			this.passwordTb.Location = new System.Drawing.Point(71, 38);
			this.passwordTb.Name = "passwordTb";
			this.passwordTb.PasswordChar = '*';
			this.passwordTb.Size = new System.Drawing.Size(200, 20);
			this.passwordTb.TabIndex = 3;
			// 
			// submitBtn
			// 
			this.submitBtn.Location = new System.Drawing.Point(196, 64);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(75, 23);
			this.submitBtn.TabIndex = 4;
			this.submitBtn.Text = "Submit";
			this.submitBtn.UseVisualStyleBackColor = true;
			this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// localeLbl
			// 
			this.localeLbl.AutoSize = true;
			this.localeLbl.Location = new System.Drawing.Point(12, 9);
			this.localeLbl.Name = "localeLbl";
			this.localeLbl.Size = new System.Drawing.Size(59, 13);
			this.localeLbl.TabIndex = 0;
			this.localeLbl.Text = "Locale: {0}";
			// 
			// usernameLbl
			// 
			this.usernameLbl.AutoSize = true;
			this.usernameLbl.Location = new System.Drawing.Point(12, 22);
			this.usernameLbl.Name = "usernameLbl";
			this.usernameLbl.Size = new System.Drawing.Size(75, 13);
			this.usernameLbl.TabIndex = 1;
			this.usernameLbl.Text = "Username: {0}";
			// 
			// AudibleLoginDialog
			// 
			this.AcceptButton = this.submitBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(283, 99);
			this.Controls.Add(this.usernameLbl);
			this.Controls.Add(this.localeLbl);
			this.Controls.Add(this.submitBtn);
			this.Controls.Add(this.passwordLbl);
			this.Controls.Add(this.passwordTb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AudibleLoginDialog";
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
	}
}