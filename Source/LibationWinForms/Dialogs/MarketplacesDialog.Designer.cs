namespace LibationWinForms.Dialogs
{
	partial class MarketplacesDialog
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.introLbl = new System.Windows.Forms.Label();
			this.accountLbl = new System.Windows.Forms.Label();
			this.marketplacesClb = new System.Windows.Forms.CheckedListBox();
			this.statusLbl = new System.Windows.Forms.Label();
			this.checkBtn = new System.Windows.Forms.Button();
			this.saveBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// introLbl
			// 
			this.introLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.introLbl.Location = new System.Drawing.Point(14, 14);
			this.introLbl.Name = "introLbl";
			this.introLbl.Size = new System.Drawing.Size(516, 76);
			this.introLbl.TabIndex = 0;
			// 
			// accountLbl
			// 
			this.accountLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.accountLbl.AutoEllipsis = true;
			this.accountLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.accountLbl.Location = new System.Drawing.Point(14, 96);
			this.accountLbl.Name = "accountLbl";
			this.accountLbl.Size = new System.Drawing.Size(516, 19);
			this.accountLbl.TabIndex = 1;
			// 
			// marketplacesClb
			// 
			this.marketplacesClb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.marketplacesClb.CheckOnClick = true;
			this.marketplacesClb.FormattingEnabled = true;
			this.marketplacesClb.Location = new System.Drawing.Point(14, 121);
			this.marketplacesClb.Name = "marketplacesClb";
			this.marketplacesClb.Size = new System.Drawing.Size(516, 202);
			this.marketplacesClb.TabIndex = 2;
			this.marketplacesClb.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.marketplacesClb_ItemCheck);
			// 
			// statusLbl
			// 
			this.statusLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.statusLbl.Location = new System.Drawing.Point(14, 331);
			this.statusLbl.Name = "statusLbl";
			this.statusLbl.Size = new System.Drawing.Size(516, 60);
			this.statusLbl.TabIndex = 3;
			// 
			// checkBtn
			// 
			this.checkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBtn.Location = new System.Drawing.Point(14, 399);
			this.checkBtn.Name = "checkBtn";
			this.checkBtn.Size = new System.Drawing.Size(190, 27);
			this.checkBtn.TabIndex = 4;
			this.checkBtn.UseVisualStyleBackColor = true;
			this.checkBtn.Click += new System.EventHandler(this.checkBtn_Click);
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(342, 399);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(88, 27);
			this.saveBtn.TabIndex = 5;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(442, 399);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(88, 27);
			this.cancelBtn.TabIndex = 6;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// MarketplacesDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(544, 440);
			this.Controls.Add(this.introLbl);
			this.Controls.Add(this.accountLbl);
			this.Controls.Add(this.marketplacesClb);
			this.Controls.Add(this.statusLbl);
			this.Controls.Add(this.checkBtn);
			this.Controls.Add(this.saveBtn);
			this.Controls.Add(this.cancelBtn);
			this.MinimumSize = new System.Drawing.Size(460, 400);
			this.Name = "MarketplacesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Marketplaces";
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Label introLbl;
		private System.Windows.Forms.Label accountLbl;
		private System.Windows.Forms.CheckedListBox marketplacesClb;
		private System.Windows.Forms.Label statusLbl;
		private System.Windows.Forms.Button checkBtn;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.Button cancelBtn;
	}
}
