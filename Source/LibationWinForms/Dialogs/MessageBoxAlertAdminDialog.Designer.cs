
namespace LibationWinForms.Dialogs
{
	partial class MessageBoxAlertAdminDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxAlertAdminDialog));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.descriptionLbl = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.githubLink = new System.Windows.Forms.LinkLabel();
			this.okBtn = new System.Windows.Forms.Button();
			this.logsLink = new System.Windows.Forms.LinkLabel();
			this.exceptionTb = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(64, 64);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// descriptionLbl
			// 
			this.descriptionLbl.AutoSize = true;
			this.descriptionLbl.Location = new System.Drawing.Point(82, 12);
			this.descriptionLbl.Name = "descriptionLbl";
			this.descriptionLbl.Size = new System.Drawing.Size(89, 45);
			this.descriptionLbl.TabIndex = 0;
			this.descriptionLbl.Text = "[Error message]\r\n[Error message]\r\n[Error message]";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 244);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(269, 90);
			this.label1.TabIndex = 2;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// githubLink
			// 
			this.githubLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.githubLink.AutoSize = true;
			this.githubLink.Location = new System.Drawing.Point(271, 274);
			this.githubLink.Name = "githubLink";
			this.githubLink.Size = new System.Drawing.Size(116, 15);
			this.githubLink.TabIndex = 3;
			this.githubLink.TabStop = true;
			this.githubLink.Text = "Click to go to github";
			this.githubLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.githubLink_LinkClicked);
			// 
			// okBtn
			// 
			this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.okBtn.Location = new System.Drawing.Point(256, 347);
			this.okBtn.Name = "okBtn";
			this.okBtn.Size = new System.Drawing.Size(75, 23);
			this.okBtn.TabIndex = 5;
			this.okBtn.Text = "OK";
			this.okBtn.UseVisualStyleBackColor = true;
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// logsLink
			// 
			this.logsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.logsLink.AutoSize = true;
			this.logsLink.Location = new System.Drawing.Point(271, 289);
			this.logsLink.Name = "logsLink";
			this.logsLink.Size = new System.Drawing.Size(155, 15);
			this.logsLink.TabIndex = 4;
			this.logsLink.TabStop = true;
			this.logsLink.Text = "Click to open log files folder";
			this.logsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.logsLink_LinkClicked);
			// 
			// exceptionTb
			// 
			this.exceptionTb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.exceptionTb.Location = new System.Drawing.Point(12, 82);
			this.exceptionTb.Multiline = true;
			this.exceptionTb.Name = "exceptionTb";
			this.exceptionTb.ReadOnly = true;
			this.exceptionTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.exceptionTb.Size = new System.Drawing.Size(560, 159);
			this.exceptionTb.TabIndex = 1;
			// 
			// MessageBoxAlertAdminDialog
			// 
			this.AcceptButton = this.okBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(584, 382);
			this.Controls.Add(this.exceptionTb);
			this.Controls.Add(this.logsLink);
			this.Controls.Add(this.okBtn);
			this.Controls.Add(this.githubLink);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.descriptionLbl);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageBoxAlertAdminDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "MessageBoxAlertAdmin";
			this.Load += new System.EventHandler(this.MessageBoxAlertAdminDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label descriptionLbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel githubLink;
		private System.Windows.Forms.Button okBtn;
		private System.Windows.Forms.LinkLabel logsLink;
		private System.Windows.Forms.TextBox exceptionTb;
	}
}