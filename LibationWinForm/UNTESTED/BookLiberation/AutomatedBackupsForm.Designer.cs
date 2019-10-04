namespace LibationWinForm.BookLiberation
{
    partial class AutomatedBackupsForm
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
            this.keepGoingCb = new System.Windows.Forms.CheckBox();
            this.logTb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // keepGoingCb
            // 
            this.keepGoingCb.AutoSize = true;
            this.keepGoingCb.Checked = true;
            this.keepGoingCb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.keepGoingCb.Location = new System.Drawing.Point(12, 12);
            this.keepGoingCb.Name = "keepGoingCb";
            this.keepGoingCb.Size = new System.Drawing.Size(325, 17);
            this.keepGoingCb.TabIndex = 0;
            this.keepGoingCb.Text = "Keep going. Uncheck to stop when current backup is complete";
            this.keepGoingCb.UseVisualStyleBackColor = true;
            // 
            // logTb
            // 
            this.logTb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTb.Location = new System.Drawing.Point(12, 48);
            this.logTb.Multiline = true;
            this.logTb.Name = "logTb";
            this.logTb.ReadOnly = true;
            this.logTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTb.Size = new System.Drawing.Size(960, 202);
            this.logTb.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(501, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "NOTE: if the working directories are inside of Dropbox, some book liberation acti" +
    "ons may hang indefinitely";
            // 
            // AutomatedBackupsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 262);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logTb);
            this.Controls.Add(this.keepGoingCb);
            this.Name = "AutomatedBackupsForm";
            this.Text = "Automated Backups";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AutomatedBackupsForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox keepGoingCb;
        private System.Windows.Forms.TextBox logTb;
        private System.Windows.Forms.Label label1;
    }
}