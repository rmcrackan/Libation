namespace inAudibleLite
{
	partial class Form1
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
			this.btnConvert = new System.Windows.Forms.Button();
			this.txtInputFile = new System.Windows.Forms.TextBox();
			this.btnSelectFile = new System.Windows.Forms.Button();
			this.txtOutputFile = new System.Windows.Forms.TextBox();
			this.rtbLog = new System.Windows.Forms.RichTextBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.decryptKeyLbl = new System.Windows.Forms.Label();
			this.decryptKeyTb = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// btnConvert
			// 
			this.btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.btnConvert.Location = new System.Drawing.Point(12, 306);
			this.btnConvert.Name = "btnConvert";
			this.btnConvert.Size = new System.Drawing.Size(600, 23);
			this.btnConvert.TabIndex = 7;
			this.btnConvert.Text = "Begin Conversion";
			this.btnConvert.UseVisualStyleBackColor = true;
			this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
			// 
			// txtInputFile
			// 
			this.txtInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtInputFile.Location = new System.Drawing.Point(47, 14);
			this.txtInputFile.Name = "txtInputFile";
			this.txtInputFile.Size = new System.Drawing.Size(565, 20);
			this.txtInputFile.TabIndex = 1;
			// 
			// btnSelectFile
			// 
			this.btnSelectFile.Location = new System.Drawing.Point(12, 12);
			this.btnSelectFile.Name = "btnSelectFile";
			this.btnSelectFile.Size = new System.Drawing.Size(27, 23);
			this.btnSelectFile.TabIndex = 0;
			this.btnSelectFile.Text = "...";
			this.btnSelectFile.UseVisualStyleBackColor = true;
			this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
			// 
			// txtOutputFile
			// 
			this.txtOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutputFile.Location = new System.Drawing.Point(47, 40);
			this.txtOutputFile.Name = "txtOutputFile";
			this.txtOutputFile.Size = new System.Drawing.Size(565, 20);
			this.txtOutputFile.TabIndex = 2;
			// 
			// rtbLog
			// 
			this.rtbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbLog.Location = new System.Drawing.Point(12, 66);
			this.rtbLog.Name = "rtbLog";
			this.rtbLog.Size = new System.Drawing.Size(491, 205);
			this.rtbLog.TabIndex = 5;
			this.rtbLog.Text = "";
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(12, 277);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(600, 23);
			this.progressBar1.TabIndex = 6;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(512, 111);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(100, 100);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			// 
			// decryptKeyLbl
			// 
			this.decryptKeyLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.decryptKeyLbl.AutoSize = true;
			this.decryptKeyLbl.Location = new System.Drawing.Point(509, 69);
			this.decryptKeyLbl.Name = "decryptKeyLbl";
			this.decryptKeyLbl.Size = new System.Drawing.Size(64, 13);
			this.decryptKeyLbl.TabIndex = 3;
			this.decryptKeyLbl.Text = "Decrypt key";
			// 
			// decryptKeyTb
			// 
			this.decryptKeyTb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.decryptKeyTb.Location = new System.Drawing.Point(512, 85);
			this.decryptKeyTb.Name = "decryptKeyTb";
			this.decryptKeyTb.Size = new System.Drawing.Size(100, 20);
			this.decryptKeyTb.TabIndex = 4;
			this.decryptKeyTb.Text = "bd895809";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 341);
			this.Controls.Add(this.decryptKeyTb);
			this.Controls.Add(this.decryptKeyLbl);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.rtbLog);
			this.Controls.Add(this.txtOutputFile);
			this.Controls.Add(this.btnSelectFile);
			this.Controls.Add(this.txtInputFile);
			this.Controls.Add(this.btnConvert);
			this.Name = "Form1";
			this.Text = "inAudibleLite 1.0";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.TextBox txtInputFile;
		private System.Windows.Forms.Button btnSelectFile;
		private System.Windows.Forms.TextBox txtOutputFile;
		private System.Windows.Forms.RichTextBox rtbLog;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label decryptKeyLbl;
		private System.Windows.Forms.TextBox decryptKeyTb;
	}
}
