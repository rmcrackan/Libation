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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.remainingTimeLbl = new System.Windows.Forms.Label();
			this.etaLbl = new System.Windows.Forms.Label();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.statusLbl = new System.Windows.Forms.Label();
			this.bookInfoLbl = new System.Windows.Forms.Label();
			this.moveUpBtn = new System.Windows.Forms.Button();
			this.moveDownBtn = new System.Windows.Forms.Button();
			this.moveFirstBtn = new System.Windows.Forms.Button();
			this.moveLastBtn = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(2, 2);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(80, 80);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(88, 65);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(212, 17);
			this.progressBar1.TabIndex = 2;
			// 
			// remainingTimeLbl
			// 
			this.remainingTimeLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.remainingTimeLbl.AutoSize = true;
			this.remainingTimeLbl.Location = new System.Drawing.Point(338, 65);
			this.remainingTimeLbl.Name = "remainingTimeLbl";
			this.remainingTimeLbl.Size = new System.Drawing.Size(30, 15);
			this.remainingTimeLbl.TabIndex = 3;
			this.remainingTimeLbl.Text = "--:--";
			this.remainingTimeLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// etaLbl
			// 
			this.etaLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.etaLbl.AutoSize = true;
			this.etaLbl.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.etaLbl.Location = new System.Drawing.Point(304, 66);
			this.etaLbl.Name = "etaLbl";
			this.etaLbl.Size = new System.Drawing.Size(28, 13);
			this.etaLbl.TabIndex = 3;
			this.etaLbl.Text = "ETA:";
			this.etaLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.BackColor = System.Drawing.Color.Transparent;
			this.cancelBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancelBtn.BackgroundImage")));
			this.cancelBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.cancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancelBtn.ForeColor = System.Drawing.SystemColors.Control;
			this.cancelBtn.Location = new System.Drawing.Point(348, 6);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(0);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(20, 20);
			this.cancelBtn.TabIndex = 4;
			this.cancelBtn.UseVisualStyleBackColor = false;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// statusLbl
			// 
			this.statusLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.statusLbl.AutoSize = true;
			this.statusLbl.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.statusLbl.Location = new System.Drawing.Point(89, 66);
			this.statusLbl.Name = "statusLbl";
			this.statusLbl.Size = new System.Drawing.Size(50, 13);
			this.statusLbl.TabIndex = 3;
			this.statusLbl.Text = "[STATUS]";
			this.statusLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// bookInfoLbl
			// 
			this.bookInfoLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bookInfoLbl.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.bookInfoLbl.Location = new System.Drawing.Point(89, 6);
			this.bookInfoLbl.Name = "bookInfoLbl";
			this.bookInfoLbl.Size = new System.Drawing.Size(219, 56);
			this.bookInfoLbl.TabIndex = 1;
			this.bookInfoLbl.Text = "[multi-\r\nline\r\nbook\r\n info]";
			// 
			// moveUpBtn
			// 
			this.moveUpBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.moveUpBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("moveUpBtn.BackgroundImage")));
			this.moveUpBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.moveUpBtn.Location = new System.Drawing.Point(314, 24);
			this.moveUpBtn.Name = "moveUpBtn";
			this.moveUpBtn.Size = new System.Drawing.Size(30, 17);
			this.moveUpBtn.TabIndex = 5;
			this.moveUpBtn.UseVisualStyleBackColor = true;
			this.moveUpBtn.Click += new System.EventHandler(this.moveUpBtn_Click_1);
			// 
			// moveDownBtn
			// 
			this.moveDownBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.moveDownBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("moveDownBtn.BackgroundImage")));
			this.moveDownBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.moveDownBtn.Location = new System.Drawing.Point(314, 40);
			this.moveDownBtn.Name = "moveDownBtn";
			this.moveDownBtn.Size = new System.Drawing.Size(30, 17);
			this.moveDownBtn.TabIndex = 5;
			this.moveDownBtn.UseVisualStyleBackColor = true;
			this.moveDownBtn.Click += new System.EventHandler(this.moveDownBtn_Click);
			// 
			// moveFirstBtn
			// 
			this.moveFirstBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.moveFirstBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("moveFirstBtn.BackgroundImage")));
			this.moveFirstBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.moveFirstBtn.Location = new System.Drawing.Point(314, 3);
			this.moveFirstBtn.Name = "moveFirstBtn";
			this.moveFirstBtn.Size = new System.Drawing.Size(30, 17);
			this.moveFirstBtn.TabIndex = 5;
			this.moveFirstBtn.UseVisualStyleBackColor = true;
			this.moveFirstBtn.Click += new System.EventHandler(this.moveFirstBtn_Click);
			// 
			// moveLastBtn
			// 
			this.moveLastBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.moveLastBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("moveLastBtn.BackgroundImage")));
			this.moveLastBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.moveLastBtn.Location = new System.Drawing.Point(314, 63);
			this.moveLastBtn.Name = "moveLastBtn";
			this.moveLastBtn.Size = new System.Drawing.Size(30, 17);
			this.moveLastBtn.TabIndex = 5;
			this.moveLastBtn.UseVisualStyleBackColor = true;
			this.moveLastBtn.Click += new System.EventHandler(this.moveLastBtn_Click);
			// 
			// ProcessBookControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.moveLastBtn);
			this.Controls.Add(this.moveDownBtn);
			this.Controls.Add(this.moveFirstBtn);
			this.Controls.Add(this.moveUpBtn);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.statusLbl);
			this.Controls.Add(this.etaLbl);
			this.Controls.Add(this.remainingTimeLbl);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.bookInfoLbl);
			this.Controls.Add(this.pictureBox1);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "ProcessBookControl";
			this.Size = new System.Drawing.Size(375, 86);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label remainingTimeLbl;
		private System.Windows.Forms.Label etaLbl;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Label statusLbl;
		private System.Windows.Forms.Label bookInfoLbl;
		private System.Windows.Forms.Button moveUpBtn;
		private System.Windows.Forms.Button moveDownBtn;
		private System.Windows.Forms.Button moveFirstBtn;
		private System.Windows.Forms.Button moveLastBtn;
	}
}
