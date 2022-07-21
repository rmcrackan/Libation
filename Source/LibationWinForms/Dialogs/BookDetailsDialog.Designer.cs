namespace LibationWinForms.Dialogs
{
    partial class BookDetailsDialog
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
            this.saveBtn = new System.Windows.Forms.Button();
            this.newTagsTb = new System.Windows.Forms.TextBox();
            this.tagsDescLbl = new System.Windows.Forms.Label();
            this.coverPb = new System.Windows.Forms.PictureBox();
            this.detailsTb = new System.Windows.Forms.TextBox();
            this.tagsGb = new System.Windows.Forms.GroupBox();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.liberatedGb = new System.Windows.Forms.GroupBox();
            this.pdfLiberatedCb = new System.Windows.Forms.ComboBox();
            this.pdfLiberatedLbl = new System.Windows.Forms.Label();
            this.bookLiberatedCb = new System.Windows.Forms.ComboBox();
            this.bookLiberatedLbl = new System.Windows.Forms.Label();
            this.liberatedDescLbl = new System.Windows.Forms.Label();
            this.audibleLink = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.coverPb)).BeginInit();
            this.tagsGb.SuspendLayout();
            this.liberatedGb.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(376, 427);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(88, 27);
            this.saveBtn.TabIndex = 4;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // newTagsTb
            // 
            this.newTagsTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.newTagsTb.Location = new System.Drawing.Point(7, 40);
            this.newTagsTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.newTagsTb.Name = "newTagsTb";
            this.newTagsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.newTagsTb.Size = new System.Drawing.Size(556, 23);
            this.newTagsTb.TabIndex = 1;
            // 
            // tagsDescLbl
            // 
            this.tagsDescLbl.AutoSize = true;
            this.tagsDescLbl.Location = new System.Drawing.Point(7, 19);
            this.tagsDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tagsDescLbl.Name = "tagsDescLbl";
            this.tagsDescLbl.Size = new System.Drawing.Size(458, 15);
            this.tagsDescLbl.TabIndex = 0;
            this.tagsDescLbl.Text = "Tags are separated by a space. Each tag can contain letters, numbers, and undersc" +
    "ores";
            // 
            // coverPb
            // 
            this.coverPb.Location = new System.Drawing.Point(12, 12);
            this.coverPb.Name = "coverPb";
            this.coverPb.Size = new System.Drawing.Size(80, 80);
            this.coverPb.TabIndex = 3;
            this.coverPb.TabStop = false;
            // 
            // detailsTb
            // 
            this.detailsTb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.detailsTb.Location = new System.Drawing.Point(98, 12);
            this.detailsTb.Multiline = true;
            this.detailsTb.Name = "detailsTb";
            this.detailsTb.ReadOnly = true;
            this.detailsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.detailsTb.Size = new System.Drawing.Size(484, 202);
            this.detailsTb.TabIndex = 1;
            // 
            // tagsGb
            // 
            this.tagsGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagsGb.Controls.Add(this.tagsDescLbl);
            this.tagsGb.Controls.Add(this.newTagsTb);
            this.tagsGb.Location = new System.Drawing.Point(12, 220);
            this.tagsGb.Name = "tagsGb";
            this.tagsGb.Size = new System.Drawing.Size(570, 73);
            this.tagsGb.TabIndex = 2;
            this.tagsGb.TabStop = false;
            this.tagsGb.Text = "Edit Tags";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(494, 427);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(88, 27);
            this.cancelBtn.TabIndex = 5;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // liberatedGb
            // 
            this.liberatedGb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.liberatedGb.Controls.Add(this.pdfLiberatedCb);
            this.liberatedGb.Controls.Add(this.pdfLiberatedLbl);
            this.liberatedGb.Controls.Add(this.bookLiberatedCb);
            this.liberatedGb.Controls.Add(this.bookLiberatedLbl);
            this.liberatedGb.Controls.Add(this.liberatedDescLbl);
            this.liberatedGb.Location = new System.Drawing.Point(12, 299);
            this.liberatedGb.Name = "liberatedGb";
            this.liberatedGb.Size = new System.Drawing.Size(570, 122);
            this.liberatedGb.TabIndex = 3;
            this.liberatedGb.TabStop = false;
            this.liberatedGb.Text = "Liberated status: Whether the book/pdf has been downloaded";
            // 
            // pdfLiberatedCb
            // 
            this.pdfLiberatedCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pdfLiberatedCb.FormattingEnabled = true;
            this.pdfLiberatedCb.Location = new System.Drawing.Point(244, 86);
            this.pdfLiberatedCb.Name = "pdfLiberatedCb";
            this.pdfLiberatedCb.Size = new System.Drawing.Size(121, 23);
            this.pdfLiberatedCb.TabIndex = 4;
            // 
            // pdfLiberatedLbl
            // 
            this.pdfLiberatedLbl.AutoSize = true;
            this.pdfLiberatedLbl.Location = new System.Drawing.Point(210, 89);
            this.pdfLiberatedLbl.Name = "pdfLiberatedLbl";
            this.pdfLiberatedLbl.Size = new System.Drawing.Size(28, 15);
            this.pdfLiberatedLbl.TabIndex = 3;
            this.pdfLiberatedLbl.Text = "PDF";
            // 
            // bookLiberatedCb
            // 
            this.bookLiberatedCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bookLiberatedCb.FormattingEnabled = true;
            this.bookLiberatedCb.Location = new System.Drawing.Point(47, 86);
            this.bookLiberatedCb.Name = "bookLiberatedCb";
            this.bookLiberatedCb.Size = new System.Drawing.Size(121, 23);
            this.bookLiberatedCb.TabIndex = 2;
            // 
            // bookLiberatedLbl
            // 
            this.bookLiberatedLbl.AutoSize = true;
            this.bookLiberatedLbl.Location = new System.Drawing.Point(7, 89);
            this.bookLiberatedLbl.Name = "bookLiberatedLbl";
            this.bookLiberatedLbl.Size = new System.Drawing.Size(34, 15);
            this.bookLiberatedLbl.TabIndex = 1;
            this.bookLiberatedLbl.Text = "Book";
            // 
            // liberatedDescLbl
            // 
            this.liberatedDescLbl.AutoSize = true;
            this.liberatedDescLbl.Location = new System.Drawing.Point(20, 31);
            this.liberatedDescLbl.Name = "liberatedDescLbl";
            this.liberatedDescLbl.Size = new System.Drawing.Size(312, 30);
            this.liberatedDescLbl.TabIndex = 0;
            this.liberatedDescLbl.Text = "To download again next time: change to Not Downloaded\r\nTo not download: change to" +
    " Downloaded";
            // 
            // audibleLink
            // 
            this.audibleLink.AutoSize = true;
            this.audibleLink.Location = new System.Drawing.Point(12, 169);
            this.audibleLink.Name = "audibleLink";
            this.audibleLink.Size = new System.Drawing.Size(57, 45);
            this.audibleLink.TabIndex = 0;
            this.audibleLink.TabStop = true;
            this.audibleLink.Text = "Open in\r\nAudible\r\n(browser)";
            this.audibleLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.audibleLink_LinkClicked);
            // 
            // BookDetailsDialog
            // 
            this.AcceptButton = this.saveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(594, 466);
            this.Controls.Add(this.audibleLink);
            this.Controls.Add(this.liberatedGb);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.tagsGb);
            this.Controls.Add(this.detailsTb);
            this.Controls.Add(this.coverPb);
            this.Controls.Add(this.saveBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BookDetailsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Book Details";
            ((System.ComponentModel.ISupportInitialize)(this.coverPb)).EndInit();
            this.tagsGb.ResumeLayout(false);
            this.tagsGb.PerformLayout();
            this.liberatedGb.ResumeLayout(false);
            this.liberatedGb.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.TextBox newTagsTb;
        private System.Windows.Forms.Label tagsDescLbl;
		private System.Windows.Forms.PictureBox coverPb;
		private System.Windows.Forms.TextBox detailsTb;
		private System.Windows.Forms.GroupBox tagsGb;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.GroupBox liberatedGb;
		private System.Windows.Forms.ComboBox pdfLiberatedCb;
		private System.Windows.Forms.Label pdfLiberatedLbl;
		private System.Windows.Forms.ComboBox bookLiberatedCb;
		private System.Windows.Forms.Label bookLiberatedLbl;
		private System.Windows.Forms.Label liberatedDescLbl;
        private System.Windows.Forms.LinkLabel audibleLink;
    }
}