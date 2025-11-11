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
			saveBtn = new System.Windows.Forms.Button();
			newTagsTb = new System.Windows.Forms.TextBox();
			tagsDescLbl = new System.Windows.Forms.Label();
			coverPb = new System.Windows.Forms.PictureBox();
			detailsTb = new System.Windows.Forms.TextBox();
			tagsGb = new System.Windows.Forms.GroupBox();
			cancelBtn = new System.Windows.Forms.Button();
			liberatedGb = new System.Windows.Forms.GroupBox();
			pdfLiberatedCb = new System.Windows.Forms.ComboBox();
			pdfLiberatedLbl = new System.Windows.Forms.Label();
			bookLiberatedCb = new System.Windows.Forms.ComboBox();
			bookLiberatedLbl = new System.Windows.Forms.Label();
			liberatedDescLbl = new System.Windows.Forms.Label();
			audibleLink = new System.Windows.Forms.LinkLabel();
			dolbyAtmosPb = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)coverPb).BeginInit();
			tagsGb.SuspendLayout();
			liberatedGb.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dolbyAtmosPb).BeginInit();
			SuspendLayout();
			// 
			// saveBtn
			// 
			saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveBtn.Location = new System.Drawing.Point(376, 427);
			saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			saveBtn.Name = "saveBtn";
			saveBtn.Size = new System.Drawing.Size(88, 27);
			saveBtn.TabIndex = 4;
			saveBtn.Text = "Save";
			saveBtn.UseVisualStyleBackColor = true;
			saveBtn.Click += saveBtn_Click;
			// 
			// newTagsTb
			// 
			newTagsTb.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			newTagsTb.Location = new System.Drawing.Point(7, 40);
			newTagsTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			newTagsTb.Name = "newTagsTb";
			newTagsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			newTagsTb.Size = new System.Drawing.Size(556, 23);
			newTagsTb.TabIndex = 1;
			// 
			// tagsDescLbl
			// 
			tagsDescLbl.AutoSize = true;
			tagsDescLbl.Location = new System.Drawing.Point(7, 19);
			tagsDescLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			tagsDescLbl.Name = "tagsDescLbl";
			tagsDescLbl.Size = new System.Drawing.Size(459, 15);
			tagsDescLbl.TabIndex = 0;
			tagsDescLbl.Text = "Tags are separated by a space. Each tag can contain letters, numbers, and underscores";
			// 
			// coverPb
			// 
			coverPb.Location = new System.Drawing.Point(12, 12);
			coverPb.Name = "coverPb";
			coverPb.Size = new System.Drawing.Size(80, 80);
			coverPb.TabIndex = 3;
			coverPb.TabStop = false;
			// 
			// detailsTb
			// 
			detailsTb.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			detailsTb.Location = new System.Drawing.Point(98, 12);
			detailsTb.Multiline = true;
			detailsTb.Name = "detailsTb";
			detailsTb.ReadOnly = true;
			detailsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			detailsTb.Size = new System.Drawing.Size(484, 202);
			detailsTb.TabIndex = 3;
			// 
			// tagsGb
			// 
			tagsGb.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tagsGb.Controls.Add(tagsDescLbl);
			tagsGb.Controls.Add(newTagsTb);
			tagsGb.Location = new System.Drawing.Point(12, 220);
			tagsGb.Name = "tagsGb";
			tagsGb.Size = new System.Drawing.Size(570, 73);
			tagsGb.TabIndex = 0;
			tagsGb.TabStop = false;
			tagsGb.Text = "Edit Tags";
			// 
			// cancelBtn
			// 
			cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cancelBtn.Location = new System.Drawing.Point(494, 427);
			cancelBtn.Name = "cancelBtn";
			cancelBtn.Size = new System.Drawing.Size(88, 27);
			cancelBtn.TabIndex = 5;
			cancelBtn.Text = "Cancel";
			cancelBtn.UseVisualStyleBackColor = true;
			cancelBtn.Click += cancelBtn_Click;
			// 
			// liberatedGb
			// 
			liberatedGb.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			liberatedGb.Controls.Add(pdfLiberatedCb);
			liberatedGb.Controls.Add(pdfLiberatedLbl);
			liberatedGb.Controls.Add(bookLiberatedCb);
			liberatedGb.Controls.Add(bookLiberatedLbl);
			liberatedGb.Controls.Add(liberatedDescLbl);
			liberatedGb.Location = new System.Drawing.Point(12, 299);
			liberatedGb.Name = "liberatedGb";
			liberatedGb.Size = new System.Drawing.Size(570, 122);
			liberatedGb.TabIndex = 1;
			liberatedGb.TabStop = false;
			liberatedGb.Text = "Liberated status: Whether the book/pdf has been downloaded";
			// 
			// pdfLiberatedCb
			// 
			pdfLiberatedCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			pdfLiberatedCb.FormattingEnabled = true;
			pdfLiberatedCb.Location = new System.Drawing.Point(244, 86);
			pdfLiberatedCb.Name = "pdfLiberatedCb";
			pdfLiberatedCb.Size = new System.Drawing.Size(121, 23);
			pdfLiberatedCb.TabIndex = 4;
			// 
			// pdfLiberatedLbl
			// 
			pdfLiberatedLbl.AutoSize = true;
			pdfLiberatedLbl.Location = new System.Drawing.Point(210, 89);
			pdfLiberatedLbl.Name = "pdfLiberatedLbl";
			pdfLiberatedLbl.Size = new System.Drawing.Size(28, 15);
			pdfLiberatedLbl.TabIndex = 3;
			pdfLiberatedLbl.Text = "PDF";
			// 
			// bookLiberatedCb
			// 
			bookLiberatedCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			bookLiberatedCb.FormattingEnabled = true;
			bookLiberatedCb.Location = new System.Drawing.Point(47, 86);
			bookLiberatedCb.Name = "bookLiberatedCb";
			bookLiberatedCb.Size = new System.Drawing.Size(121, 23);
			bookLiberatedCb.TabIndex = 2;
			// 
			// bookLiberatedLbl
			// 
			bookLiberatedLbl.AutoSize = true;
			bookLiberatedLbl.Location = new System.Drawing.Point(7, 89);
			bookLiberatedLbl.Name = "bookLiberatedLbl";
			bookLiberatedLbl.Size = new System.Drawing.Size(34, 15);
			bookLiberatedLbl.TabIndex = 1;
			bookLiberatedLbl.Text = "Book";
			// 
			// liberatedDescLbl
			// 
			liberatedDescLbl.AutoSize = true;
			liberatedDescLbl.Location = new System.Drawing.Point(20, 31);
			liberatedDescLbl.Name = "liberatedDescLbl";
			liberatedDescLbl.Size = new System.Drawing.Size(312, 30);
			liberatedDescLbl.TabIndex = 0;
			liberatedDescLbl.Text = "To download again next time: change to Not Downloaded\r\nTo not download: change to Downloaded";
			// 
			// audibleLink
			// 
			audibleLink.Location = new System.Drawing.Point(12, 169);
			audibleLink.Name = "audibleLink";
			audibleLink.Size = new System.Drawing.Size(80, 45);
			audibleLink.TabIndex = 2;
			audibleLink.TabStop = true;
			audibleLink.Text = "Open in\r\nAudible\r\n(browser)";
			audibleLink.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			audibleLink.LinkClicked += audibleLink_LinkClicked;
			// 
			// dolbyAtmosPb
			// 
			dolbyAtmosPb.Image = Properties.Resources.Dolby_Atmos_Vertical_80;
			dolbyAtmosPb.Location = new System.Drawing.Point(12, 112);
			dolbyAtmosPb.Name = "dolbyAtmosPb";
			dolbyAtmosPb.Size = new System.Drawing.Size(80, 36);
			dolbyAtmosPb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			dolbyAtmosPb.TabIndex = 6;
			dolbyAtmosPb.TabStop = false;
			// 
			// BookDetailsDialog
			// 
			AcceptButton = saveBtn;
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			CancelButton = cancelBtn;
			ClientSize = new System.Drawing.Size(594, 466);
			Controls.Add(dolbyAtmosPb);
			Controls.Add(audibleLink);
			Controls.Add(liberatedGb);
			Controls.Add(cancelBtn);
			Controls.Add(tagsGb);
			Controls.Add(detailsTb);
			Controls.Add(coverPb);
			Controls.Add(saveBtn);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "BookDetailsDialog";
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Book Details";
			((System.ComponentModel.ISupportInitialize)coverPb).EndInit();
			tagsGb.ResumeLayout(false);
			tagsGb.PerformLayout();
			liberatedGb.ResumeLayout(false);
			liberatedGb.PerformLayout();
			((System.ComponentModel.ISupportInitialize)dolbyAtmosPb).EndInit();
			ResumeLayout(false);
			PerformLayout();

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
		private System.Windows.Forms.PictureBox dolbyAtmosPb;
	}
}