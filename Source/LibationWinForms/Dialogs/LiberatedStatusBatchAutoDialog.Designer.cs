namespace LibationWinForms.Dialogs
{
    partial class LiberatedStatusBatchAutoDialog
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
            this.setDownloadedCb = new System.Windows.Forms.CheckBox();
            this.setNotDownloadedCb = new System.Windows.Forms.CheckBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // setDownloadedCb
            // 
            this.setDownloadedCb.AutoSize = true;
            this.setDownloadedCb.Location = new System.Drawing.Point(12, 12);
            this.setDownloadedCb.Name = "setDownloadedCb";
            this.setDownloadedCb.Size = new System.Drawing.Size(379, 19);
            this.setDownloadedCb.TabIndex = 0;
            this.setDownloadedCb.Text = "If the audio file can be found, set download status to \'Downloaded\'";
            this.setDownloadedCb.UseVisualStyleBackColor = true;
            // 
            // setNotDownloadedCb
            // 
            this.setNotDownloadedCb.AutoSize = true;
            this.setNotDownloadedCb.Location = new System.Drawing.Point(12, 37);
            this.setNotDownloadedCb.Name = "setNotDownloadedCb";
            this.setNotDownloadedCb.Size = new System.Drawing.Size(412, 19);
            this.setNotDownloadedCb.TabIndex = 1;
            this.setNotDownloadedCb.Text = "If the audio file cannot be found, set download status to \'Not Downloaded\'";
            this.setNotDownloadedCb.UseVisualStyleBackColor = true;
            // 
            // okBtn
            // 
            this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okBtn.Location = new System.Drawing.Point(346, 79);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(88, 27);
            this.okBtn.TabIndex = 2;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(464, 79);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(88, 27);
            this.cancelBtn.TabIndex = 3;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // LiberatedStatusBatchAutoDialog
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(564, 118);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.setNotDownloadedCb);
            this.Controls.Add(this.setDownloadedCb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LiberatedStatusBatchAutoDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Liberated status: Scan for files";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox setDownloadedCb;
        private System.Windows.Forms.CheckBox setNotDownloadedCb;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
    }
}