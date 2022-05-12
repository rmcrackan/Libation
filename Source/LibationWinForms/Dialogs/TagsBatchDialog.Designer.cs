namespace LibationWinForms.Dialogs
{
    partial class TagsBatchDialog
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
            this.tagsDescLbl = new System.Windows.Forms.Label();
            this.newTagsTb = new System.Windows.Forms.TextBox();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tagsDescLbl
            // 
            this.tagsDescLbl.AutoSize = true;
            this.tagsDescLbl.Location = new System.Drawing.Point(13, 9);
            this.tagsDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tagsDescLbl.Name = "tagsDescLbl";
            this.tagsDescLbl.Size = new System.Drawing.Size(458, 15);
            this.tagsDescLbl.TabIndex = 2;
            this.tagsDescLbl.Text = "Tags are separated by a space. Each tag can contain letters, numbers, and undersc" +
    "ores";
            // 
            // newTagsTb
            // 
            this.newTagsTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.newTagsTb.Location = new System.Drawing.Point(13, 30);
            this.newTagsTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newTagsTb.Name = "newTagsTb";
            this.newTagsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.newTagsTb.Size = new System.Drawing.Size(591, 23);
            this.newTagsTb.TabIndex = 3;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(517, 71);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(88, 27);
            this.cancelBtn.TabIndex = 6;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(399, 71);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(88, 27);
            this.saveBtn.TabIndex = 5;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // TagsBatchDialog
            // 
            this.AcceptButton = this.saveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(617, 110);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.tagsDescLbl);
            this.Controls.Add(this.newTagsTb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagsBatchDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Replace Tags";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label tagsDescLbl;
        private System.Windows.Forms.TextBox newTagsTb;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button saveBtn;
    }
}