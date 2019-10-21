namespace LibationWinForm.Dialogs.Login
{
	partial class CaptchaDialog
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
			this.captchaPb = new System.Windows.Forms.PictureBox();
			this.answerTb = new System.Windows.Forms.TextBox();
			this.submitBtn = new System.Windows.Forms.Button();
			this.answerLbl = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.captchaPb)).BeginInit();
			this.SuspendLayout();
			// 
			// captchaPb
			// 
			this.captchaPb.Location = new System.Drawing.Point(12, 12);
			this.captchaPb.Name = "captchaPb";
			this.captchaPb.Size = new System.Drawing.Size(429, 186);
			this.captchaPb.TabIndex = 0;
			this.captchaPb.TabStop = false;
			// 
			// answerTb
			// 
			this.answerTb.Location = new System.Drawing.Point(118, 206);
			this.answerTb.Name = "answerTb";
			this.answerTb.Size = new System.Drawing.Size(100, 20);
			this.answerTb.TabIndex = 1;
			// 
			// submitBtn
			// 
			this.submitBtn.Location = new System.Drawing.Point(366, 204);
			this.submitBtn.Name = "submitBtn";
			this.submitBtn.Size = new System.Drawing.Size(75, 23);
			this.submitBtn.TabIndex = 2;
			this.submitBtn.Text = "Submit";
			this.submitBtn.UseVisualStyleBackColor = true;
			this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
			// 
			// answerLbl
			// 
			this.answerLbl.AutoSize = true;
			this.answerLbl.Location = new System.Drawing.Point(12, 209);
			this.answerLbl.Name = "answerLbl";
			this.answerLbl.Size = new System.Drawing.Size(100, 13);
			this.answerLbl.TabIndex = 0;
			this.answerLbl.Text = "CAPTCHA answer: ";
			// 
			// CaptchaDialog
			// 
			this.AcceptButton = this.submitBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(453, 239);
			this.Controls.Add(this.answerLbl);
			this.Controls.Add(this.submitBtn);
			this.Controls.Add(this.answerTb);
			this.Controls.Add(this.captchaPb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CaptchaDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "CAPTCHA";
			((System.ComponentModel.ISupportInitialize)(this.captchaPb)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox captchaPb;
		private System.Windows.Forms.TextBox answerTb;
		private System.Windows.Forms.Button submitBtn;
		private System.Windows.Forms.Label answerLbl;
	}
}