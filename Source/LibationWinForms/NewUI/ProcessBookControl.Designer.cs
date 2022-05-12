namespace LibationWinForms
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
			this.bookInfoLbl = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.remainingTimeLbl = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.moveUpBtn = new System.Windows.Forms.Button();
			this.moveDownBtn = new System.Windows.Forms.Button();
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
			// bookInfoLbl
			// 
			this.bookInfoLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bookInfoLbl.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.bookInfoLbl.Location = new System.Drawing.Point(89, 3);
			this.bookInfoLbl.Name = "bookInfoLbl";
			this.bookInfoLbl.Size = new System.Drawing.Size(255, 56);
			this.bookInfoLbl.TabIndex = 1;
			this.bookInfoLbl.Text = "[multi-\r\nline\r\nbook\r\n info]";
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
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(304, 66);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "ETA:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.BackColor = System.Drawing.Color.Transparent;
			this.cancelBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancelBtn.BackgroundImage")));
			this.cancelBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.cancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancelBtn.ForeColor = System.Drawing.SystemColors.Control;
			this.cancelBtn.Location = new System.Drawing.Point(352, 3);
			this.cancelBtn.Margin = new System.Windows.Forms.Padding(0);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(20, 20);
			this.cancelBtn.TabIndex = 4;
			this.cancelBtn.UseVisualStyleBackColor = false;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// moveUpBtn
			// 
			this.moveUpBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveUpBtn.BackColor = System.Drawing.Color.Transparent;
			this.moveUpBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("moveUpBtn.BackgroundImage")));
			this.moveUpBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.moveUpBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.moveUpBtn.ForeColor = System.Drawing.SystemColors.Control;
			this.moveUpBtn.Location = new System.Drawing.Point(347, 39);
			this.moveUpBtn.Margin = new System.Windows.Forms.Padding(0);
			this.moveUpBtn.Name = "moveUpBtn";
			this.moveUpBtn.Size = new System.Drawing.Size(25, 10);
			this.moveUpBtn.TabIndex = 4;
			this.moveUpBtn.UseVisualStyleBackColor = false;
			this.moveUpBtn.Click += new System.EventHandler(this.moveUpBtn_Click);
			// 
			// moveDownBtn
			// 
			this.moveDownBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveDownBtn.BackColor = System.Drawing.Color.Transparent;
			this.moveDownBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("moveDownBtn.BackgroundImage")));
			this.moveDownBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.moveDownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.moveDownBtn.ForeColor = System.Drawing.SystemColors.Control;
			this.moveDownBtn.Location = new System.Drawing.Point(347, 49);
			this.moveDownBtn.Margin = new System.Windows.Forms.Padding(0);
			this.moveDownBtn.Name = "moveDownBtn";
			this.moveDownBtn.Size = new System.Drawing.Size(25, 10);
			this.moveDownBtn.TabIndex = 5;
			this.moveDownBtn.UseVisualStyleBackColor = false;
			this.moveDownBtn.Click += new System.EventHandler(this.moveDownBtn_Click);
			// 
			// ProcessBookControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.moveDownBtn);
			this.Controls.Add(this.moveUpBtn);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.remainingTimeLbl);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.bookInfoLbl);
			this.Controls.Add(this.pictureBox1);
			this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.Name = "ProcessBookControl";
			this.Size = new System.Drawing.Size(375, 86);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label bookInfoLbl;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label remainingTimeLbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button moveUpBtn;
		private System.Windows.Forms.Button moveDownBtn;
	}
}
