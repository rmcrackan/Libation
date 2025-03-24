namespace LibationWinForms.Dialogs
{
	partial class AboutDialog
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
			pictureBox1 = new System.Windows.Forms.PictureBox();
			releaseNotesLbl = new System.Windows.Forms.LinkLabel();
			checkForUpgradeBtn = new System.Windows.Forms.Button();
			getLibationLbl = new System.Windows.Forms.LinkLabel();
			rmcrackanLbl = new System.Windows.Forms.LinkLabel();
			MBucariLbl = new System.Windows.Forms.LinkLabel();
			groupBox1 = new System.Windows.Forms.GroupBox();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pictureBox1.Image = Properties.Resources.cheers;
			pictureBox1.Location = new System.Drawing.Point(12, 91);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(410, 210);
			pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			// 
			// releaseNotesLbl
			// 
			releaseNotesLbl.AutoSize = true;
			releaseNotesLbl.Font = new System.Drawing.Font("Segoe UI", 11F);
			releaseNotesLbl.Location = new System.Drawing.Point(12, 12);
			releaseNotesLbl.Name = "releaseNotesLbl";
			releaseNotesLbl.Size = new System.Drawing.Size(171, 20);
			releaseNotesLbl.TabIndex = 2;
			releaseNotesLbl.TabStop = true;
			releaseNotesLbl.Text = "Libation Classic v11.0.0.0";
			releaseNotesLbl.LinkClicked += releaseNotesLbl_LinkClicked;
			// 
			// checkForUpgradeBtn
			// 
			checkForUpgradeBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			checkForUpgradeBtn.Font = new System.Drawing.Font("Segoe UI", 10F);
			checkForUpgradeBtn.Location = new System.Drawing.Point(12, 54);
			checkForUpgradeBtn.Name = "checkForUpgradeBtn";
			checkForUpgradeBtn.Size = new System.Drawing.Size(410, 31);
			checkForUpgradeBtn.TabIndex = 3;
			checkForUpgradeBtn.Text = "Check for Upgrade";
			checkForUpgradeBtn.UseVisualStyleBackColor = true;
			checkForUpgradeBtn.Click += checkForUpgradeBtn_Click;
			// 
			// getLibationLbl
			// 
			getLibationLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			getLibationLbl.AutoSize = true;
			getLibationLbl.Font = new System.Drawing.Font("Segoe UI", 11F);
			getLibationLbl.Location = new System.Drawing.Point(245, 12);
			getLibationLbl.Name = "getLibationLbl";
			getLibationLbl.Size = new System.Drawing.Size(162, 20);
			getLibationLbl.TabIndex = 7;
			getLibationLbl.TabStop = true;
			getLibationLbl.Text = "https://getlibation.com";
			getLibationLbl.LinkClicked += getLibationLbl_LinkClicked;
			// 
			// rmcrackanLbl
			// 
			rmcrackanLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			rmcrackanLbl.AutoSize = true;
			rmcrackanLbl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			rmcrackanLbl.Location = new System.Drawing.Point(6, 19);
			rmcrackanLbl.Name = "rmcrackanLbl";
			rmcrackanLbl.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			rmcrackanLbl.Size = new System.Drawing.Size(80, 25);
			rmcrackanLbl.TabIndex = 8;
			rmcrackanLbl.TabStop = true;
			rmcrackanLbl.Text = "rmcrackan";
			rmcrackanLbl.LinkClicked += ContributorLabel_LinkClicked;
			// 
			// MBucariLbl
			// 
			MBucariLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			MBucariLbl.AutoSize = true;
			MBucariLbl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			MBucariLbl.Location = new System.Drawing.Point(6, 40);
			MBucariLbl.Name = "MBucariLbl";
			MBucariLbl.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			MBucariLbl.Size = new System.Drawing.Size(64, 25);
			MBucariLbl.TabIndex = 9;
			MBucariLbl.TabStop = true;
			MBucariLbl.Text = "Mbucari";
			MBucariLbl.LinkClicked += ContributorLabel_LinkClicked;
			// 
			// groupBox1
			// 
			groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(label4);
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(label1);
			groupBox1.Controls.Add(flowLayoutPanel1);
			groupBox1.Controls.Add(MBucariLbl);
			groupBox1.Controls.Add(rmcrackanLbl);
			groupBox1.Location = new System.Drawing.Point(12, 307);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(410, 172);
			groupBox1.TabIndex = 10;
			groupBox1.TabStop = false;
			groupBox1.Text = "Acknowledgements";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Segoe UI", 10F);
			label3.Location = new System.Drawing.Point(92, 43);
			label3.Name = "label3";
			label3.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			label3.Size = new System.Drawing.Size(71, 22);
			label3.TabIndex = 12;
			label3.Text = "Developer";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Font = new System.Drawing.Font("Segoe UI", 10F);
			label4.Location = new System.Drawing.Point(92, 22);
			label4.Name = "label4";
			label4.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			label4.Size = new System.Drawing.Size(55, 22);
			label4.TabIndex = 12;
			label4.Text = "Creator";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Segoe UI", 10F);
			label2.Location = new System.Drawing.Point(92, 22);
			label2.Name = "label2";
			label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			label2.Size = new System.Drawing.Size(45, 22);
			label2.TabIndex = 12;
			label2.Text = "label2";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(6, 82);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(157, 15);
			label1.TabIndex = 11;
			label1.Text = "Additional Contributions by:";
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			flowLayoutPanel1.Location = new System.Drawing.Point(6, 100);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new System.Drawing.Size(398, 66);
			flowLayoutPanel1.TabIndex = 10;
			// 
			// AboutDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(434, 491);
			Controls.Add(groupBox1);
			Controls.Add(getLibationLbl);
			Controls.Add(checkForUpgradeBtn);
			Controls.Add(releaseNotesLbl);
			Controls.Add(pictureBox1);
			MinimumSize = new System.Drawing.Size(445, 530);
			Name = "AboutDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "About Libation";
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.LinkLabel releaseNotesLbl;
		private System.Windows.Forms.Button checkForUpgradeBtn;
		private System.Windows.Forms.LinkLabel getLibationLbl;
		private System.Windows.Forms.LinkLabel rmcrackanLbl;
		private System.Windows.Forms.LinkLabel MBucariLbl;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label label2;
	}
}