namespace LibationWinForms.Dialogs
{
	partial class SetupDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialog));
			this.welcomeLbl = new System.Windows.Forms.Label();
			this.noQuestionsBtn = new System.Windows.Forms.Button();
			this.basicBtn = new System.Windows.Forms.Button();
			this.advancedBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// welcomeLbl
			// 
			this.welcomeLbl.AutoSize = true;
			this.welcomeLbl.Location = new System.Drawing.Point(12, 9);
			this.welcomeLbl.Name = "welcomeLbl";
			this.welcomeLbl.Size = new System.Drawing.Size(399, 78);
			this.welcomeLbl.TabIndex = 0;
			this.welcomeLbl.Text = resources.GetString("welcomeLbl.Text");
			// 
			// noQuestionsBtn
			// 
			this.noQuestionsBtn.Location = new System.Drawing.Point(15, 90);
			this.noQuestionsBtn.Name = "noQuestionsBtn";
			this.noQuestionsBtn.Size = new System.Drawing.Size(396, 57);
			this.noQuestionsBtn.TabIndex = 1;
			this.noQuestionsBtn.Text = "NO-QUESTIONS SETUP\r\n\r\nAccept all defaults";
			this.noQuestionsBtn.UseVisualStyleBackColor = true;
			// 
			// basicBtn
			// 
			this.basicBtn.Location = new System.Drawing.Point(15, 153);
			this.basicBtn.Name = "basicBtn";
			this.basicBtn.Size = new System.Drawing.Size(396, 57);
			this.basicBtn.TabIndex = 2;
			this.basicBtn.Text = "BASIC SETUP\r\n\r\nChoose settings";
			this.basicBtn.UseVisualStyleBackColor = true;
			// 
			// advancedBtn
			// 
			this.advancedBtn.Location = new System.Drawing.Point(15, 216);
			this.advancedBtn.Name = "advancedBtn";
			this.advancedBtn.Size = new System.Drawing.Size(396, 57);
			this.advancedBtn.TabIndex = 3;
			this.advancedBtn.Text = "ADVANCED SETUP\r\n\r\nChoose settings and where to store them";
			this.advancedBtn.UseVisualStyleBackColor = true;
			// 
			// SetupDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(423, 285);
			this.Controls.Add(this.advancedBtn);
			this.Controls.Add(this.basicBtn);
			this.Controls.Add(this.noQuestionsBtn);
			this.Controls.Add(this.welcomeLbl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SetupDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Welcome to Libation";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label welcomeLbl;
		private System.Windows.Forms.Button noQuestionsBtn;
		private System.Windows.Forms.Button basicBtn;
		private System.Windows.Forms.Button advancedBtn;
	}
}