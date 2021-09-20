namespace LibationWinForms.Dialogs.Login
{
	partial class LoginExternalDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginExternalDialog));
			this.submitBtn = new System.Windows.Forms.Button();
			this.localeLbl = new System.Windows.Forms.Label();
			this.usernameLbl = new System.Windows.Forms.Label();
			this.loginUrlLbl = new System.Windows.Forms.Label();
			this.loginUrlTb = new System.Windows.Forms.TextBox();
			this.copyBtn = new System.Windows.Forms.Button();
			this.launchBrowserBtn = new System.Windows.Forms.Button();
			this.instructionsLbl = new System.Windows.Forms.Label();
			this.responseUrlTb = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// submitBtn
			// 
			this.submitBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.submitBtn.Location = new System.Drawing.Point(665, 400);
			this.submitBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(88, 27);
			this.submitBtn.TabIndex = 8;
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
			// loginUrlLbl
			// 
			this.loginUrlLbl.AutoSize = true;
			this.loginUrlLbl.Location = new System.Drawing.Point(14, 61);
			this.loginUrlLbl.Name = "loginUrlLbl";
			this.loginUrlLbl.Size = new System.Drawing.Size(180, 15);
			this.loginUrlLbl.TabIndex = 2;
			this.loginUrlLbl.Text = "Paste this URL into your browser:";
			// 
			// loginUrlTb
			// 
			this.loginUrlTb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.loginUrlTb.Location = new System.Drawing.Point(14, 79);
			this.loginUrlTb.Multiline = true;
			this.loginUrlTb.Name = "loginUrlTb";
			this.loginUrlTb.ReadOnly = true;
			this.loginUrlTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.loginUrlTb.Size = new System.Drawing.Size(739, 92);
			this.loginUrlTb.TabIndex = 3;
			// 
			// copyBtn
			// 
			this.copyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.copyBtn.Location = new System.Drawing.Point(14, 177);
			this.copyBtn.Name = "copyBtn";
			this.copyBtn.Size = new System.Drawing.Size(165, 23);
			this.copyBtn.TabIndex = 4;
			this.copyBtn.Text = "Copy URL to clipboard";
			this.copyBtn.UseVisualStyleBackColor = true;
			this.copyBtn.Click += new System.EventHandler(this.copyBtn_Click);
			// 
			// launchBrowserBtn
			// 
			this.launchBrowserBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.launchBrowserBtn.Location = new System.Drawing.Point(588, 177);
			this.launchBrowserBtn.Name = "launchBrowserBtn";
			this.launchBrowserBtn.Size = new System.Drawing.Size(165, 23);
			this.launchBrowserBtn.TabIndex = 5;
			this.launchBrowserBtn.Text = "Launch in browser";
			this.launchBrowserBtn.UseVisualStyleBackColor = true;
			this.launchBrowserBtn.Click += new System.EventHandler(this.launchBrowserBtn_Click);
			// 
			// instructionsLbl
			// 
			this.instructionsLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.instructionsLbl.AutoSize = true;
			this.instructionsLbl.Location = new System.Drawing.Point(14, 203);
			this.instructionsLbl.Name = "instructionsLbl";
			this.instructionsLbl.Size = new System.Drawing.Size(436, 90);
			this.instructionsLbl.TabIndex = 6;
			this.instructionsLbl.Text = resources.GetString("instructionsLbl.Text");
			// 
			// responseUrlTb
			// 
			this.responseUrlTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.responseUrlTb.Location = new System.Drawing.Point(14, 296);
			this.responseUrlTb.Multiline = true;
			this.responseUrlTb.Name = "responseUrlTb";
			this.responseUrlTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.responseUrlTb.Size = new System.Drawing.Size(739, 98);
			this.responseUrlTb.TabIndex = 7;
			// 
			// LoginExternalDialog
			// 
			this.AcceptButton = this.submitBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(766, 440);
			this.Controls.Add(this.responseUrlTb);
			this.Controls.Add(this.instructionsLbl);
			this.Controls.Add(this.launchBrowserBtn);
			this.Controls.Add(this.copyBtn);
			this.Controls.Add(this.loginUrlTb);
			this.Controls.Add(this.loginUrlLbl);
			this.Controls.Add(this.usernameLbl);
			this.Controls.Add(this.localeLbl);
			this.Controls.Add(this.submitBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginExternalDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Audible External Login";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Label localeLbl;
		private System.Windows.Forms.Label usernameLbl;
		private System.Windows.Forms.Label loginUrlLbl;
		private System.Windows.Forms.TextBox loginUrlTb;
		private System.Windows.Forms.Button copyBtn;
		private System.Windows.Forms.Button launchBrowserBtn;
		private System.Windows.Forms.Label instructionsLbl;
		private System.Windows.Forms.TextBox responseUrlTb;
	}
}