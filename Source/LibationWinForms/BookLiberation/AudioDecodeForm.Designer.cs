namespace LibationWinForms.BookLiberation
{
    partial class AudioDecodeForm
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.bookInfoLbl = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.remainingTimeLbl = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(14, 14);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(117, 115);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// bookInfoLbl
			// 
			this.bookInfoLbl.AutoSize = true;
			this.bookInfoLbl.Location = new System.Drawing.Point(138, 14);
			this.bookInfoLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.bookInfoLbl.Name = "bookInfoLbl";
			this.bookInfoLbl.Size = new System.Drawing.Size(121, 15);
			this.bookInfoLbl.TabIndex = 0;
			this.bookInfoLbl.Text = "[multi-line book info]";
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(14, 143);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(611, 27);
			this.progressBar1.TabIndex = 2;
			// 
			// remainingTimeLbl
			// 
			this.remainingTimeLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.remainingTimeLbl.Location = new System.Drawing.Point(632, 143);
			this.remainingTimeLbl.Name = "remainingTimeLbl";
			this.remainingTimeLbl.Size = new System.Drawing.Size(60, 31);
			this.remainingTimeLbl.TabIndex = 3;
			this.remainingTimeLbl.Text = "ETA:\r\n";
			this.remainingTimeLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// DecryptForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(707, 183);
			this.Controls.Add(this.remainingTimeLbl);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.bookInfoLbl);
			this.Controls.Add(this.pictureBox1);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "DecryptForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "DecryptForm";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label bookInfoLbl;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label remainingTimeLbl;
    }
}