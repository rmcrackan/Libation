namespace LibationWinForms.Dialogs
{
	partial class ScanAccountsDialog
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
			this.accountsLbl = new System.Windows.Forms.Label();
			this.accountsClb = new System.Windows.Forms.CheckedListBox();
			this.importBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// accountsLbl
			// 
			this.accountsLbl.AutoSize = true;
			this.accountsLbl.Location = new System.Drawing.Point(12, 9);
			this.accountsLbl.Name = "accountsLbl";
			this.accountsLbl.Size = new System.Drawing.Size(193, 13);
			this.accountsLbl.TabIndex = 0;
			this.accountsLbl.Text = "Check the accounts to scan and import";
			// 
			// accountsClb
			// 
			this.accountsClb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.accountsClb.FormattingEnabled = true;
			this.accountsClb.Location = new System.Drawing.Point(12, 25);
			this.accountsClb.Name = "accountsClb";
			this.accountsClb.Size = new System.Drawing.Size(560, 94);
			this.accountsClb.TabIndex = 1;
			// 
			// importBtn
			// 
			this.importBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.importBtn.Location = new System.Drawing.Point(396, 125);
			this.importBtn.Name = "importBtn";
			this.importBtn.Size = new System.Drawing.Size(75, 23);
			this.importBtn.TabIndex = 2;
			this.importBtn.Text = "Import";
			this.importBtn.UseVisualStyleBackColor = true;
			this.importBtn.Click += new System.EventHandler(this.importBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(497, 125);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 3;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// ScanAccountsDialog
			// 
			this.AcceptButton = this.importBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(584, 160);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.importBtn);
			this.Controls.Add(this.accountsClb);
			this.Controls.Add(this.accountsLbl);
			this.Name = "ScanAccountsDialog";
			this.Text = "Which accounts?";
			this.Load += new System.EventHandler(this.ScanAccountsDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label accountsLbl;
		private System.Windows.Forms.CheckedListBox accountsClb;
		private System.Windows.Forms.Button importBtn;
		private System.Windows.Forms.Button cancelBtn;
	}
}