namespace LibationWinForm
{
    partial class ScanLibraryDialog
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
            this.websiteProcessorControl1 = new LibationWinForm.WebsiteProcessorControl();
            this.BeginScanBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // websiteProcessorControl1
            // 
            this.websiteProcessorControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.websiteProcessorControl1.Location = new System.Drawing.Point(12, 12);
            this.websiteProcessorControl1.Name = "websiteProcessorControl1";
            this.websiteProcessorControl1.Size = new System.Drawing.Size(324, 137);
            this.websiteProcessorControl1.TabIndex = 0;
            // 
            // BeginScanBtn
            // 
            this.BeginScanBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BeginScanBtn.Location = new System.Drawing.Point(12, 155);
            this.BeginScanBtn.Name = "BeginScanBtn";
            this.BeginScanBtn.Size = new System.Drawing.Size(324, 23);
            this.BeginScanBtn.TabIndex = 1;
            this.BeginScanBtn.Text = "BEGIN SCAN";
            this.BeginScanBtn.UseVisualStyleBackColor = true;
            // 
            // ScanLibraryDialog
            // 
            this.AcceptButton = this.BeginScanBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 190);
            this.Controls.Add(this.BeginScanBtn);
            this.Controls.Add(this.websiteProcessorControl1);
            this.Name = "ScanLibraryDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Scan Library";
            this.ResumeLayout(false);

        }

        #endregion

        private WebsiteProcessorControl websiteProcessorControl1;
        private System.Windows.Forms.Button BeginScanBtn;
    }
}