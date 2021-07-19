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
			this.newUserBtn = new System.Windows.Forms.Button();
			this.returningUserBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// welcomeLbl
			// 
			this.welcomeLbl.AutoSize = true;
			this.welcomeLbl.Location = new System.Drawing.Point(14, 10);
			this.welcomeLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.welcomeLbl.Name = "welcomeLbl";
			this.welcomeLbl.Size = new System.Drawing.Size(449, 135);
			this.welcomeLbl.TabIndex = 0;
			this.welcomeLbl.Text = resources.GetString("welcomeLbl.Text");
			// 
			// newUserBtn
			// 
			this.newUserBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.newUserBtn.Location = new System.Drawing.Point(18, 156);
			this.newUserBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.newUserBtn.Name = "newUserBtn";
			this.newUserBtn.Size = new System.Drawing.Size(462, 66);
			this.newUserBtn.TabIndex = 2;
			this.newUserBtn.Text = "NEW USER\r\n\r\nChoose settings";
			this.newUserBtn.UseVisualStyleBackColor = true;
			this.newUserBtn.Click += new System.EventHandler(this.newUserBtn_Click);
			// 
			// returningUserBtn
			// 
			this.returningUserBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.returningUserBtn.Location = new System.Drawing.Point(18, 228);
			this.returningUserBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.returningUserBtn.Name = "returningUserBtn";
			this.returningUserBtn.Size = new System.Drawing.Size(462, 66);
			this.returningUserBtn.TabIndex = 3;
			this.returningUserBtn.Text = "RETURNING USER\r\n\r\nI have previously installed Libation. This is an upgrade or re-" +
    "install";
			this.returningUserBtn.UseVisualStyleBackColor = true;
			this.returningUserBtn.Click += new System.EventHandler(this.returningUserBtn_Click);
			// 
			// SetupDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(493, 308);
			this.Controls.Add(this.returningUserBtn);
			this.Controls.Add(this.newUserBtn);
			this.Controls.Add(this.welcomeLbl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "SetupDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Welcome to Libation";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label welcomeLbl;
		private System.Windows.Forms.Button newUserBtn;
		private System.Windows.Forms.Button returningUserBtn;
	}
}