namespace LibationWinForms.Dialogs
{
	partial class UpgradeNotificationDialog
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
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.GroupBox groupBox1;
			System.Windows.Forms.LinkLabel linkLabel3;
			System.Windows.Forms.LinkLabel linkLabel2;
			System.Windows.Forms.Label label3;
			this.packageDlLink = new System.Windows.Forms.LinkLabel();
			this.releaseNotesTbox = new System.Windows.Forms.TextBox();
			this.dontRemindBtn = new System.Windows.Forms.Button();
			this.yesBtn = new System.Windows.Forms.Button();
			this.noBtn = new System.Windows.Forms.Button();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			groupBox1 = new System.Windows.Forms.GroupBox();
			linkLabel3 = new System.Windows.Forms.LinkLabel();
			linkLabel2 = new System.Windows.Forms.LinkLabel();
			label3 = new System.Windows.Forms.Label();
			groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(416, 21);
			label1.TabIndex = 0;
			label1.Text = "There is a new version available. Would you like to update?";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 39);
			label2.Name = "label2";
			label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			label2.Size = new System.Drawing.Size(327, 25);
			label2.TabIndex = 1;
			label2.Text = "After you close Libation, the upgrade will start automatically.";
			// 
			// groupBox1
			// 
			groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			groupBox1.Controls.Add(linkLabel3);
			groupBox1.Controls.Add(linkLabel2);
			groupBox1.Controls.Add(this.packageDlLink);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(this.releaseNotesTbox);
			groupBox1.Location = new System.Drawing.Point(12, 67);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(531, 303);
			groupBox1.TabIndex = 3;
			groupBox1.TabStop = false;
			groupBox1.Text = "Release Information";
			// 
			// linkLabel3
			// 
			linkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			linkLabel3.AutoSize = true;
			linkLabel3.Location = new System.Drawing.Point(348, 250);
			linkLabel3.Name = "linkLabel3";
			linkLabel3.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			linkLabel3.Size = new System.Drawing.Size(177, 25);
			linkLabel3.TabIndex = 1;
			linkLabel3.TabStop = true;
			linkLabel3.Text = "View the source code on GitHub";
			linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoToGithub_LinkClicked);
			// 
			// linkLabel2
			// 
			linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			linkLabel2.AutoSize = true;
			linkLabel2.Location = new System.Drawing.Point(392, 275);
			linkLabel2.Name = "linkLabel2";
			linkLabel2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			linkLabel2.Size = new System.Drawing.Size(133, 25);
			linkLabel2.TabIndex = 2;
			linkLabel2.TabStop = true;
			linkLabel2.Text = "Go to Libation\'s website";
			linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoToWebsite_LinkClicked);
			// 
			// packageDlLink
			// 
			this.packageDlLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.packageDlLink.AutoSize = true;
			this.packageDlLink.Location = new System.Drawing.Point(6, 275);
			this.packageDlLink.Name = "packageDlLink";
			this.packageDlLink.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.packageDlLink.Size = new System.Drawing.Size(157, 25);
			this.packageDlLink.TabIndex = 3;
			this.packageDlLink.TabStop = true;
			this.packageDlLink.Text = "[Release Package File Name]";
			this.packageDlLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PackageDlLink_LinkClicked);
			// 
			// label3
			// 
			label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(6, 250);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(106, 15);
			label3.TabIndex = 3;
			label3.Text = "Download Release:";
			// 
			// releaseNotesTbox
			// 
			this.releaseNotesTbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.releaseNotesTbox.Location = new System.Drawing.Point(6, 22);
			this.releaseNotesTbox.Multiline = true;
			this.releaseNotesTbox.Name = "releaseNotesTbox";
			this.releaseNotesTbox.ReadOnly = true;
			this.releaseNotesTbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.releaseNotesTbox.Size = new System.Drawing.Size(519, 214);
			this.releaseNotesTbox.TabIndex = 0;
			// 
			// dontRemindBtn
			// 
			this.dontRemindBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.dontRemindBtn.Location = new System.Drawing.Point(12, 376);
			this.dontRemindBtn.Name = "dontRemindBtn";
			this.dontRemindBtn.Size = new System.Drawing.Size(121, 38);
			this.dontRemindBtn.TabIndex = 4;
			this.dontRemindBtn.Text = "Don\'t remind me about this release";
			this.dontRemindBtn.UseVisualStyleBackColor = true;
			this.dontRemindBtn.Click += new System.EventHandler(this.DontRemindBtn_Click);
			// 
			// yesBtn
			// 
			this.yesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.yesBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.yesBtn.Location = new System.Drawing.Point(440, 376);
			this.yesBtn.Name = "yesBtn";
			this.yesBtn.Size = new System.Drawing.Size(103, 38);
			this.yesBtn.TabIndex = 6;
			this.yesBtn.Text = "Yes";
			this.yesBtn.UseVisualStyleBackColor = true;
			this.yesBtn.Click += new System.EventHandler(this.YesBtn_Click);
			// 
			// noBtn
			// 
			this.noBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.noBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBtn.Location = new System.Drawing.Point(360, 376);
			this.noBtn.Name = "noBtn";
			this.noBtn.Size = new System.Drawing.Size(74, 38);
			this.noBtn.TabIndex = 5;
			this.noBtn.Text = "No";
			this.noBtn.UseVisualStyleBackColor = true;
			this.noBtn.Click += new System.EventHandler(this.NoBtn_Click);
			// 
			// UpgradeNotificationDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(555, 426);
			this.Controls.Add(this.noBtn);
			this.Controls.Add(this.yesBtn);
			this.Controls.Add(this.dontRemindBtn);
			this.Controls.Add(groupBox1);
			this.Controls.Add(label2);
			this.Controls.Add(label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(460, 420);
			this.Name = "UpgradeNotificationDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "UpgradeNotificationDialog";
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox releaseNotesTbox;
		private System.Windows.Forms.LinkLabel packageDlLink;
		private System.Windows.Forms.Button dontRemindBtn;
		private System.Windows.Forms.Button yesBtn;
		private System.Windows.Forms.Button noBtn;
	}
}