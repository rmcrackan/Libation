namespace WinFormsDesigner.Dialogs.Login
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
			this.approvedBtn.Location = new System.Drawing.Point(15, 25);
			this.approvedBtn.Name = "approvedBtn";
			this.approvedBtn.Size = new System.Drawing.Size(79, 23);
			this.approvedBtn.TabIndex = 1;
			this.approvedBtn.Text = "Approved";
			this.approvedBtn.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Click after approving";
			// 
			// ApprovalNeededDialog
			// 
			this.AcceptButton = this.approvedBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(149, 60);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.approvedBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ApprovalNeededDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Approval Needed";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button approvedBtn;
		private System.Windows.Forms.Label label1;
	}
}