namespace LibationWinForms.Dialogs
{
	partial class BadBookActionDialog
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.messageLbl = new System.Windows.Forms.Label();
			this.applyToAllCb = new System.Windows.Forms.CheckBox();
			this.rememberInSettingsCb = new System.Windows.Forms.CheckBox();
			this.abortBtn = new System.Windows.Forms.Button();
			this.retryBtn = new System.Windows.Forms.Button();
			this.ignoreBtn = new System.Windows.Forms.Button();
			this.buttonPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(12, 12);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(32, 32);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			this.pictureBox.Image = System.Drawing.SystemIcons.Question.ToBitmap();
			// 
			// messageLbl
			// 
			this.messageLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.messageLbl.AutoSize = true;
			this.messageLbl.Location = new System.Drawing.Point(50, 12);
			this.messageLbl.Name = "messageLbl";
			this.messageLbl.Size = new System.Drawing.Size(422, 15);
			this.messageLbl.TabIndex = 1;
			// 
			// applyToAllCb
			// 
			this.applyToAllCb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.applyToAllCb.AutoSize = true;
			this.applyToAllCb.Location = new System.Drawing.Point(12, 185);
			this.applyToAllCb.Name = "applyToAllCb";
			this.applyToAllCb.Size = new System.Drawing.Size(220, 19);
			this.applyToAllCb.TabIndex = 2;
			this.applyToAllCb.Text = "Apply to all remaining books in this queue";
			this.applyToAllCb.UseVisualStyleBackColor = true;
			// 
			// rememberInSettingsCb
			// 
			this.rememberInSettingsCb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.rememberInSettingsCb.AutoSize = true;
			this.rememberInSettingsCb.Location = new System.Drawing.Point(12, 210);
			this.rememberInSettingsCb.Name = "rememberInSettingsCb";
			this.rememberInSettingsCb.Size = new System.Drawing.Size(195, 19);
			this.rememberInSettingsCb.TabIndex = 3;
			this.rememberInSettingsCb.Text = "Remember this choice in Settings";
			this.rememberInSettingsCb.UseVisualStyleBackColor = true;
			// 
			// abortBtn
			// 
			this.abortBtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.abortBtn.Location = new System.Drawing.Point(197, 8);
			this.abortBtn.MinimumSize = new System.Drawing.Size(75, 28);
			this.abortBtn.Name = "abortBtn";
			this.abortBtn.Size = new System.Drawing.Size(75, 28);
			this.abortBtn.TabIndex = 4;
			this.abortBtn.Text = "Abort";
			this.abortBtn.UseVisualStyleBackColor = true;
			this.abortBtn.Click += new System.EventHandler(this.AbortBtn_Click);
			// 
			// retryBtn
			// 
			this.retryBtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.retryBtn.Location = new System.Drawing.Point(278, 8);
			this.retryBtn.MinimumSize = new System.Drawing.Size(75, 28);
			this.retryBtn.Name = "retryBtn";
			this.retryBtn.Size = new System.Drawing.Size(75, 28);
			this.retryBtn.TabIndex = 5;
			this.retryBtn.Text = "Retry";
			this.retryBtn.UseVisualStyleBackColor = true;
			this.retryBtn.Click += new System.EventHandler(this.RetryBtn_Click);
			// 
			// ignoreBtn
			// 
			this.ignoreBtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ignoreBtn.Location = new System.Drawing.Point(359, 8);
			this.ignoreBtn.MinimumSize = new System.Drawing.Size(75, 28);
			this.ignoreBtn.Name = "ignoreBtn";
			this.ignoreBtn.Size = new System.Drawing.Size(75, 28);
			this.ignoreBtn.TabIndex = 6;
			this.ignoreBtn.Text = "Ignore";
			this.ignoreBtn.UseVisualStyleBackColor = true;
			this.ignoreBtn.Click += new System.EventHandler(this.IgnoreBtn_Click);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPanel.Controls.Add(this.abortBtn);
			this.buttonPanel.Controls.Add(this.retryBtn);
			this.buttonPanel.Controls.Add(this.ignoreBtn);
			this.buttonPanel.Location = new System.Drawing.Point(0, 240);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(484, 45);
			this.buttonPanel.TabIndex = 7;
			// 
			// BadBookActionDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(484, 285);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.rememberInSettingsCb);
			this.Controls.Add(this.applyToAllCb);
			this.Controls.Add(this.messageLbl);
			this.Controls.Add(this.pictureBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BadBookActionDialog";
			this.ShowInTaskbar = true;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Skip this book?";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Label messageLbl;
		private System.Windows.Forms.CheckBox applyToAllCb;
		private System.Windows.Forms.CheckBox rememberInSettingsCb;
		private System.Windows.Forms.Button abortBtn;
		private System.Windows.Forms.Button retryBtn;
		private System.Windows.Forms.Button ignoreBtn;
		private System.Windows.Forms.Panel buttonPanel;
	}
}
