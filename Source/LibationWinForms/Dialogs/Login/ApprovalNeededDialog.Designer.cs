namespace LibationWinForms.Dialogs.Login
{
	partial class ApprovalNeededDialog
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
			this.approvedBtn = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// approvedBtn
			// 
			this.approvedBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.approvedBtn.Location = new System.Drawing.Point(18, 75);
			this.approvedBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.approvedBtn.Name = "approvedBtn";
			this.approvedBtn.Size = new System.Drawing.Size(92, 27);
			this.approvedBtn.TabIndex = 1;
			this.approvedBtn.Text = "Approved";
			this.approvedBtn.UseVisualStyleBackColor = true;
			this.approvedBtn.Click += new System.EventHandler(this.approvedBtn_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 10);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(314, 45);
			this.label1.TabIndex = 0;
			this.label1.Text = "Amazon is sending you an email.\r\n\r\nPlease press this button after you approve the" +
    " notification.";
			// 
			// ApprovalNeededDialog
			// 
			this.AcceptButton = this.approvedBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(345, 115);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.approvedBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ApprovalNeededDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Approval Alert Detected";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button approvedBtn;
		private System.Windows.Forms.Label label1;
	}
}