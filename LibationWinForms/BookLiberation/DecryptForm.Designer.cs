namespace LibationWinForms.BookLiberation
{
    partial class DecryptForm
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
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // bookInfoLbl
            // 
            this.bookInfoLbl.AutoSize = true;
            this.bookInfoLbl.Location = new System.Drawing.Point(118, 12);
            this.bookInfoLbl.Name = "bookInfoLbl";
            this.bookInfoLbl.Size = new System.Drawing.Size(100, 13);
            this.bookInfoLbl.TabIndex = 0;
            this.bookInfoLbl.Text = "[multi-line book info]";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 526);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(582, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // rtbLog
            // 
            this.rtbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbLog.Location = new System.Drawing.Point(12, 118);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(582, 402);
            this.rtbLog.TabIndex = 1;
            this.rtbLog.Text = "";
            // 
            // DecryptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 561);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.bookInfoLbl);
            this.Controls.Add(this.pictureBox1);
            this.Name = "DecryptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DecryptForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DecryptForm_FormClosing);
            this.Load += new System.EventHandler(this.DecryptForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label bookInfoLbl;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}