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
			welcomeLbl = new System.Windows.Forms.Label();
			newUserBtn = new System.Windows.Forms.Button();
			returningUserBtn = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// welcomeLbl
			// 
			welcomeLbl.AutoSize = true;
			welcomeLbl.Location = new System.Drawing.Point(14, 10);
			welcomeLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			welcomeLbl.Name = "welcomeLbl";
			welcomeLbl.Size = new System.Drawing.Size(449, 135);
			welcomeLbl.TabIndex = 0;
			welcomeLbl.Text = resources.GetString("welcomeLbl.Text");
			// 
			// newUserBtn
			// 
			newUserBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			newUserBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			newUserBtn.Location = new System.Drawing.Point(18, 156);
			newUserBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			newUserBtn.Name = "newUserBtn";
			newUserBtn.Size = new System.Drawing.Size(462, 66);
			newUserBtn.TabIndex = 2;
			newUserBtn.Text = "NEW USER";
			newUserBtn.UseVisualStyleBackColor = true;
			newUserBtn.Click += newUserBtn_Click;
			// 
			// returningUserBtn
			// 
			returningUserBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			returningUserBtn.Location = new System.Drawing.Point(18, 228);
			returningUserBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			returningUserBtn.Name = "returningUserBtn";
			returningUserBtn.Size = new System.Drawing.Size(462, 66);
			returningUserBtn.TabIndex = 3;
			returningUserBtn.Text = "RETURNING USER\r\n\r\nI have previously installed Libation. This is an upgrade or re-install";
			returningUserBtn.UseVisualStyleBackColor = true;
			returningUserBtn.Click += returningUserBtn_Click;
			// 
			// SetupDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(493, 308);
			Controls.Add(returningUserBtn);
			Controls.Add(newUserBtn);
			Controls.Add(welcomeLbl);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "SetupDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Welcome to Libation";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label welcomeLbl;
		private System.Windows.Forms.Button newUserBtn;
		private System.Windows.Forms.Button returningUserBtn;
	}
}