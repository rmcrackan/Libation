namespace LibationWinForms.ProcessQueue
{
	partial class ProcessBookControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessBookControl));
			pictureBox1 = new System.Windows.Forms.PictureBox();
			progressBar1 = new System.Windows.Forms.ProgressBar();
			remainingTimeLbl = new System.Windows.Forms.Label();
			etaLbl = new System.Windows.Forms.Label();
			cancelBtn = new System.Windows.Forms.Button();
			statusLbl = new System.Windows.Forms.Label();
			bookInfoLbl = new System.Windows.Forms.Label();
			moveUpBtn = new System.Windows.Forms.Button();
			moveDownBtn = new System.Windows.Forms.Button();
			moveFirstBtn = new System.Windows.Forms.Button();
			moveLastBtn = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Location = new System.Drawing.Point(2, 2);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(80, 80);
			pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			// 
			// progressBar1
			// 
			progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			progressBar1.Location = new System.Drawing.Point(88, 65);
			progressBar1.MarqueeAnimationSpeed = 0;
			progressBar1.Name = "progressBar1";
			progressBar1.Size = new System.Drawing.Size(212, 17);
			progressBar1.TabIndex = 2;
			// 
			// remainingTimeLbl
			// 
			remainingTimeLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			remainingTimeLbl.AutoSize = true;
			remainingTimeLbl.Location = new System.Drawing.Point(338, 65);
			remainingTimeLbl.Name = "remainingTimeLbl";
			remainingTimeLbl.Size = new System.Drawing.Size(30, 15);
			remainingTimeLbl.TabIndex = 3;
			remainingTimeLbl.Text = "--:--";
			remainingTimeLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// etaLbl
			// 
			etaLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			etaLbl.AutoSize = true;
			etaLbl.Font = new System.Drawing.Font("Segoe UI", 8F);
			etaLbl.Location = new System.Drawing.Point(304, 66);
			etaLbl.Name = "etaLbl";
			etaLbl.Size = new System.Drawing.Size(27, 13);
			etaLbl.TabIndex = 3;
			etaLbl.Text = "ETA:";
			etaLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cancelBtn
			// 
			cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			cancelBtn.BackColor = System.Drawing.Color.Transparent;
			cancelBtn.BackgroundImage = (System.Drawing.Image)resources.GetObject("cancelBtn.BackgroundImage");
			cancelBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			cancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			cancelBtn.ForeColor = System.Drawing.SystemColors.Control;
			cancelBtn.Location = new System.Drawing.Point(348, 6);
			cancelBtn.Margin = new System.Windows.Forms.Padding(0);
			cancelBtn.Name = "cancelBtn";
			cancelBtn.Size = new System.Drawing.Size(20, 20);
			cancelBtn.TabIndex = 4;
			cancelBtn.UseVisualStyleBackColor = false;
			// 
			// statusLbl
			// 
			statusLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			statusLbl.AutoSize = true;
			statusLbl.Font = new System.Drawing.Font("Segoe UI", 8F);
			statusLbl.Location = new System.Drawing.Point(89, 66);
			statusLbl.Name = "statusLbl";
			statusLbl.Size = new System.Drawing.Size(48, 13);
			statusLbl.TabIndex = 3;
			statusLbl.Text = "[STATUS]";
			statusLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// bookInfoLbl
			// 
			bookInfoLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			bookInfoLbl.Font = new System.Drawing.Font("Segoe UI", 8F);
			bookInfoLbl.Location = new System.Drawing.Point(89, 6);
			bookInfoLbl.Name = "bookInfoLbl";
			bookInfoLbl.Size = new System.Drawing.Size(219, 56);
			bookInfoLbl.TabIndex = 1;
			bookInfoLbl.Text = "[multi-\r\nline\r\nbook\r\n info]";
			// 
			// moveUpBtn
			// 
			moveUpBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			moveUpBtn.BackColor = System.Drawing.Color.Transparent;
			moveUpBtn.BackgroundImage = Properties.Resources.move_up;
			moveUpBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			moveUpBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			moveUpBtn.ForeColor = System.Drawing.SystemColors.Control;
			moveUpBtn.Location = new System.Drawing.Point(314, 24);
			moveUpBtn.Name = "moveUpBtn";
			moveUpBtn.Size = new System.Drawing.Size(30, 17);
			moveUpBtn.TabIndex = 5;
			moveUpBtn.UseVisualStyleBackColor = false;
			// 
			// moveDownBtn
			// 
			moveDownBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			moveDownBtn.BackColor = System.Drawing.Color.Transparent;
			moveDownBtn.BackgroundImage = Properties.Resources.move_down;
			moveDownBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			moveDownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			moveDownBtn.ForeColor = System.Drawing.SystemColors.Control;
			moveDownBtn.Location = new System.Drawing.Point(314, 40);
			moveDownBtn.Name = "moveDownBtn";
			moveDownBtn.Size = new System.Drawing.Size(30, 17);
			moveDownBtn.TabIndex = 5;
			moveDownBtn.UseVisualStyleBackColor = false;
			// 
			// moveFirstBtn
			// 
			moveFirstBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			moveFirstBtn.BackColor = System.Drawing.Color.Transparent;
			moveFirstBtn.BackgroundImage = Properties.Resources.move_first;
			moveFirstBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			moveFirstBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			moveFirstBtn.ForeColor = System.Drawing.SystemColors.Control;
			moveFirstBtn.Location = new System.Drawing.Point(314, 3);
			moveFirstBtn.Name = "moveFirstBtn";
			moveFirstBtn.Size = new System.Drawing.Size(30, 17);
			moveFirstBtn.TabIndex = 5;
			moveFirstBtn.UseVisualStyleBackColor = false;
			// 
			// moveLastBtn
			// 
			moveLastBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			moveLastBtn.BackColor = System.Drawing.Color.Transparent;
			moveLastBtn.BackgroundImage = Properties.Resources.move_last;
			moveLastBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			moveLastBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			moveLastBtn.ForeColor = System.Drawing.SystemColors.Control;
			moveLastBtn.Location = new System.Drawing.Point(314, 63);
			moveLastBtn.Name = "moveLastBtn";
			moveLastBtn.Size = new System.Drawing.Size(30, 17);
			moveLastBtn.TabIndex = 5;
			moveLastBtn.UseVisualStyleBackColor = false;
			// 
			// ProcessBookControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			BackColor = System.Drawing.SystemColors.ControlLight;
			BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			Controls.Add(moveLastBtn);
			Controls.Add(moveDownBtn);
			Controls.Add(moveFirstBtn);
			Controls.Add(moveUpBtn);
			Controls.Add(cancelBtn);
			Controls.Add(statusLbl);
			Controls.Add(etaLbl);
			Controls.Add(remainingTimeLbl);
			Controls.Add(progressBar1);
			Controls.Add(bookInfoLbl);
			Controls.Add(pictureBox1);
			Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
			Name = "ProcessBookControl";
			Size = new System.Drawing.Size(375, 86);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label remainingTimeLbl;
		private System.Windows.Forms.Label etaLbl;
		private System.Windows.Forms.Label statusLbl;
		private System.Windows.Forms.Label bookInfoLbl;
		public System.Windows.Forms.Button cancelBtn;
		public System.Windows.Forms.Button moveUpBtn;
		public System.Windows.Forms.Button moveDownBtn;
		public System.Windows.Forms.Button moveFirstBtn;
		public System.Windows.Forms.Button moveLastBtn;
	}
}
