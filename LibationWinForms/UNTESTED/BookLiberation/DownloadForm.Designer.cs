namespace LibationWinForms.BookLiberation
{
    partial class DownloadForm
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
            this.filenameLbl = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressLbl = new System.Windows.Forms.Label();
            this.lastUpdateLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // filenameLbl
            // 
            this.filenameLbl.AutoSize = true;
            this.filenameLbl.Location = new System.Drawing.Point(12, 9);
            this.filenameLbl.Name = "filenameLbl";
            this.filenameLbl.Size = new System.Drawing.Size(52, 13);
            this.filenameLbl.TabIndex = 0;
            this.filenameLbl.Text = "[filename]";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(15, 67);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(877, 23);
            this.progressBar1.TabIndex = 4;
            // 
            // progressLbl
            // 
            this.progressLbl.Location = new System.Drawing.Point(12, 36);
            this.progressLbl.Name = "progressLbl";
            this.progressLbl.Size = new System.Drawing.Size(173, 13);
            this.progressLbl.TabIndex = 5;
            this.progressLbl.Text = "[2,999,999,999] of [2,999,999,999]";
            this.progressLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lastUpdateLbl
            // 
            this.lastUpdateLbl.AutoSize = true;
            this.lastUpdateLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastUpdateLbl.ForeColor = System.Drawing.Color.DarkRed;
            this.lastUpdateLbl.Location = new System.Drawing.Point(361, 36);
            this.lastUpdateLbl.Name = "lastUpdateLbl";
            this.lastUpdateLbl.Size = new System.Drawing.Size(81, 13);
            this.lastUpdateLbl.TabIndex = 6;
            this.lastUpdateLbl.Text = "Last updated";
            this.lastUpdateLbl.Visible = false;
            // 
            // DownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 102);
            this.Controls.Add(this.lastUpdateLbl);
            this.Controls.Add(this.progressLbl);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.filenameLbl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DownloadForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Downloading";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DownloadForm_FormClosing);
            this.Load += new System.EventHandler(this.DownloadForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label filenameLbl;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label progressLbl;
        private System.Windows.Forms.Label lastUpdateLbl;
    }
}