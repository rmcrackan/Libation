namespace LibationWinForms.Dialogs
{
	partial class TrashBinDialog
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
			deletedCbl = new System.Windows.Forms.CheckedListBox();
			label1 = new System.Windows.Forms.Label();
			restoreBtn = new System.Windows.Forms.Button();
			permanentlyDeleteBtn = new System.Windows.Forms.Button();
			everythingCb = new System.Windows.Forms.CheckBox();
			deletedCheckedLbl = new System.Windows.Forms.Label();
			SuspendLayout();
			// 
			// deletedCbl
			// 
			deletedCbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			deletedCbl.FormattingEnabled = true;
			deletedCbl.Location = new System.Drawing.Point(12, 27);
			deletedCbl.Name = "deletedCbl";
			deletedCbl.Size = new System.Drawing.Size(776, 364);
			deletedCbl.TabIndex = 3;
			deletedCbl.ItemCheck += deletedCbl_ItemCheck;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(388, 15);
			label1.TabIndex = 4;
			label1.Text = "Check books you want to permanently delete from or restore to Libation";
			// 
			// restoreBtn
			// 
			restoreBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			restoreBtn.Location = new System.Drawing.Point(572, 398);
			restoreBtn.Name = "restoreBtn";
			restoreBtn.Size = new System.Drawing.Size(75, 40);
			restoreBtn.TabIndex = 5;
			restoreBtn.Text = "Restore";
			restoreBtn.UseVisualStyleBackColor = true;
			restoreBtn.Click += restoreBtn_Click;
			// 
			// permanentlyDeleteBtn
			// 
			permanentlyDeleteBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			permanentlyDeleteBtn.Location = new System.Drawing.Point(653, 398);
			permanentlyDeleteBtn.Name = "permanentlyDeleteBtn";
			permanentlyDeleteBtn.Size = new System.Drawing.Size(135, 40);
			permanentlyDeleteBtn.TabIndex = 5;
			permanentlyDeleteBtn.Text = "Permanently Remove\r\nfrom Libation";
			permanentlyDeleteBtn.UseVisualStyleBackColor = true;
			permanentlyDeleteBtn.Click += permanentlyDeleteBtn_Click;
			// 
			// everythingCb
			// 
			everythingCb.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			everythingCb.AutoSize = true;
			everythingCb.Location = new System.Drawing.Point(12, 410);
			everythingCb.Name = "everythingCb";
			everythingCb.Size = new System.Drawing.Size(82, 19);
			everythingCb.TabIndex = 6;
			everythingCb.Text = "Everything";
			everythingCb.ThreeState = true;
			everythingCb.UseVisualStyleBackColor = true;
			everythingCb.CheckStateChanged += everythingCb_CheckStateChanged;
			// 
			// deletedCheckedLbl
			// 
			deletedCheckedLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			deletedCheckedLbl.AutoSize = true;
			deletedCheckedLbl.Location = new System.Drawing.Point(126, 411);
			deletedCheckedLbl.Name = "deletedCheckedLbl";
			deletedCheckedLbl.Size = new System.Drawing.Size(104, 15);
			deletedCheckedLbl.TabIndex = 7;
			deletedCheckedLbl.Text = "Checked: {0} of {1}";
			// 
			// TrashBinDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(deletedCheckedLbl);
			Controls.Add(everythingCb);
			Controls.Add(permanentlyDeleteBtn);
			Controls.Add(restoreBtn);
			Controls.Add(label1);
			Controls.Add(deletedCbl);
			Name = "TrashBinDialog";
			Text = "Trash Bin";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.CheckedListBox deletedCbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button restoreBtn;
		private System.Windows.Forms.Button permanentlyDeleteBtn;
		private System.Windows.Forms.CheckBox everythingCb;
		private System.Windows.Forms.Label deletedCheckedLbl;
	}
}