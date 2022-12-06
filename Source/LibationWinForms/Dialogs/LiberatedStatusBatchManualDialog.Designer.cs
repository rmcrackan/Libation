namespace LibationWinForms.Dialogs
{
    partial class LiberatedStatusBatchManualDialog
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
            this.bookLiberatedCb = new System.Windows.Forms.ComboBox();
            this.bookLiberatedLbl = new System.Windows.Forms.Label();
            this.liberatedDescLbl = new System.Windows.Forms.Label();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bookLiberatedCb
            // 
            this.bookLiberatedCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bookLiberatedCb.FormattingEnabled = true;
            this.bookLiberatedCb.Location = new System.Drawing.Point(52, 54);
            this.bookLiberatedCb.Name = "bookLiberatedCb";
            this.bookLiberatedCb.Size = new System.Drawing.Size(121, 23);
            this.bookLiberatedCb.TabIndex = 7;
            // 
            // bookLiberatedLbl
            // 
            this.bookLiberatedLbl.AutoSize = true;
            this.bookLiberatedLbl.Location = new System.Drawing.Point(12, 57);
            this.bookLiberatedLbl.Name = "bookLiberatedLbl";
            this.bookLiberatedLbl.Size = new System.Drawing.Size(34, 15);
            this.bookLiberatedLbl.TabIndex = 6;
            this.bookLiberatedLbl.Text = "Book";
            // 
            // liberatedDescLbl
            // 
            this.liberatedDescLbl.AutoSize = true;
            this.liberatedDescLbl.Location = new System.Drawing.Point(12, 9);
            this.liberatedDescLbl.Name = "liberatedDescLbl";
            this.liberatedDescLbl.Size = new System.Drawing.Size(312, 30);
            this.liberatedDescLbl.TabIndex = 5;
            this.liberatedDescLbl.Text = "To download again next time: change to Not Downloaded\r\nTo not download: change to" +
    " Downloaded";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(464, 79);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(88, 27);
            this.cancelBtn.TabIndex = 9;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(346, 79);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(88, 27);
            this.saveBtn.TabIndex = 8;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // LiberatedStatusBatchManualDialog
            // 
            this.AcceptButton = this.saveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(564, 118);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.bookLiberatedCb);
            this.Controls.Add(this.bookLiberatedLbl);
            this.Controls.Add(this.liberatedDescLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LiberatedStatusBatchManualDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Liberated status: Whether the book has been downloaded";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox bookLiberatedCb;
        private System.Windows.Forms.Label bookLiberatedLbl;
        private System.Windows.Forms.Label liberatedDescLbl;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button saveBtn;
    }
}