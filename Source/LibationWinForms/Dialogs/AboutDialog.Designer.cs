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
			linkLabel4 = new System.Windows.Forms.LinkLabel();
			linkLabel2 = new System.Windows.Forms.LinkLabel();
			linkLabel3 = new System.Windows.Forms.LinkLabel();
			linkLabel1 = new System.Windows.Forms.LinkLabel();
			linkLabel5 = new System.Windows.Forms.LinkLabel();
			linkLabel6 = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			groupBox1.SuspendLayout();
			flowLayoutPanel1.SuspendLayout();
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
			releaseNotesLbl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			checkForUpgradeBtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			getLibationLbl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			rmcrackanLbl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			rmcrackanLbl.Location = new System.Drawing.Point(6, 19);
			rmcrackanLbl.Name = "rmcrackanLbl";
			rmcrackanLbl.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			rmcrackanLbl.Size = new System.Drawing.Size(80, 25);
			rmcrackanLbl.TabIndex = 8;
			rmcrackanLbl.TabStop = true;
			rmcrackanLbl.Text = "rmcrackan";
			rmcrackanLbl.LinkClicked += Link_GithubUser;
			// 
			// MBucariLbl
			// 
			MBucariLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			MBucariLbl.AutoSize = true;
			MBucariLbl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			MBucariLbl.Location = new System.Drawing.Point(6, 40);
			MBucariLbl.Name = "MBucariLbl";
			MBucariLbl.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			MBucariLbl.Size = new System.Drawing.Size(64, 25);
			MBucariLbl.TabIndex = 9;
			MBucariLbl.TabStop = true;
			MBucariLbl.Text = "Mbucari";
			MBucariLbl.LinkClicked += Link_GithubUser;
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
			label3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			label4.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			flowLayoutPanel1.Controls.Add(linkLabel4);
			flowLayoutPanel1.Controls.Add(linkLabel2);
			flowLayoutPanel1.Controls.Add(linkLabel3);
			flowLayoutPanel1.Controls.Add(linkLabel1);
			flowLayoutPanel1.Controls.Add(linkLabel5);
			flowLayoutPanel1.Controls.Add(linkLabel6);
			flowLayoutPanel1.Location = new System.Drawing.Point(6, 100);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new System.Drawing.Size(398, 66);
			flowLayoutPanel1.TabIndex = 10;
			// 
			// linkLabel4
			// 
			linkLabel4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			linkLabel4.AutoSize = true;
			linkLabel4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			linkLabel4.Location = new System.Drawing.Point(3, 0);
			linkLabel4.Name = "linkLabel4";
			linkLabel4.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			linkLabel4.Size = new System.Drawing.Size(41, 21);
			linkLabel4.TabIndex = 9;
			linkLabel4.TabStop = true;
			linkLabel4.Text = "pixil98";
			linkLabel4.LinkClicked += Link_GithubUser;
			// 
			// linkLabel2
			// 
			linkLabel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			linkLabel2.AutoSize = true;
			linkLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			linkLabel2.Location = new System.Drawing.Point(50, 0);
			linkLabel2.Name = "linkLabel2";
			linkLabel2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			linkLabel2.Size = new System.Drawing.Size(104, 21);
			linkLabel2.TabIndex = 9;
			linkLabel2.TabStop = true;
			linkLabel2.Text = "hutattedonmyarm";
			linkLabel2.LinkClicked += Link_GithubUser;
			// 
			// linkLabel3
			// 
			linkLabel3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			linkLabel3.AutoSize = true;
			linkLabel3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			linkLabel3.Location = new System.Drawing.Point(160, 0);
			linkLabel3.Name = "linkLabel3";
			linkLabel3.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			linkLabel3.Size = new System.Drawing.Size(43, 21);
			linkLabel3.TabIndex = 9;
			linkLabel3.TabStop = true;
			linkLabel3.Text = "seanke";
			linkLabel3.LinkClicked += Link_GithubUser;
			// 
			// linkLabel1
			// 
			linkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			linkLabel1.AutoSize = true;
			linkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			linkLabel1.Location = new System.Drawing.Point(209, 0);
			linkLabel1.Name = "linkLabel1";
			linkLabel1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			linkLabel1.Size = new System.Drawing.Size(66, 21);
			linkLabel1.TabIndex = 9;
			linkLabel1.TabStop = true;
			linkLabel1.Text = "wtanksleyjr";
			linkLabel1.LinkClicked += Link_GithubUser;
			// 
			// linkLabel5
			// 
			linkLabel5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			linkLabel5.AutoSize = true;
			linkLabel5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			linkLabel5.Location = new System.Drawing.Point(281, 0);
			linkLabel5.Name = "linkLabel5";
			linkLabel5.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			linkLabel5.Size = new System.Drawing.Size(51, 21);
			linkLabel5.TabIndex = 9;
			linkLabel5.TabStop = true;
			linkLabel5.Text = "Dr.Blank";
			linkLabel5.LinkClicked += Link_GithubUser;
			// 
			// linkLabel6
			// 
			linkLabel6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			linkLabel6.AutoSize = true;
			linkLabel6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			linkLabel6.Location = new System.Drawing.Point(3, 21);
			linkLabel6.Name = "linkLabel6";
			linkLabel6.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			linkLabel6.Size = new System.Drawing.Size(77, 21);
			linkLabel6.TabIndex = 9;
			linkLabel6.TabStop = true;
			linkLabel6.Text = "CharlieRussel";
			linkLabel6.LinkClicked += Link_GithubUser;
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
			flowLayoutPanel1.ResumeLayout(false);
			flowLayoutPanel1.PerformLayout();
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
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.LinkLabel linkLabel4;
		private System.Windows.Forms.LinkLabel linkLabel2;
		private System.Windows.Forms.LinkLabel linkLabel3;
		private System.Windows.Forms.LinkLabel linkLabel5;
		private System.Windows.Forms.LinkLabel linkLabel6;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label2;
	}
}