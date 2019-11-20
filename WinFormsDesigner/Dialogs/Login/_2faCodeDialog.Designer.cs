namespace WinFormsDesigner.Dialogs.Login
{
	partial class _2faCodeDialog
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
			this.submitBtn = new System.Windows.Forms.Button();
			this.codeTb = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// submitBtn
			// 
			this.submitBtn.Location = new System.Drawing.Point(15, 51);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(79, 23);
			this.submitBtn.TabIndex = 1;
			this.submitBtn.Text = "Submit";
			this.submitBtn.UseVisualStyleBackColor = true;
			// 
			// codeTb
			// 
			this.codeTb.Location = new System.Drawing.Point(15, 25);
			this.codeTb.Name = "codeTb";
			this.codeTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.codeTb.Size = new System.Drawing.Size(79, 20);
			this.codeTb.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Enter 2FA Code";
			// 
			// _2faCodeDialog
			// 
			this.AcceptButton = this.submitBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(106, 86);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.codeTb);
			this.Controls.Add(this.submitBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "_2faCodeDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "2FA Code";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.TextBox codeTb;
		private System.Windows.Forms.Label label1;
	}
}