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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.releaseNotesLbl = new System.Windows.Forms.LinkLabel();
			this.checkForUpgradeBtn = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.copyBtn = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.getLibationLbl = new System.Windows.Forms.LinkLabel();
			this.rmcrackanLbl = new System.Windows.Forms.LinkLabel();
			this.MBucariLbl = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Image = global::LibationWinForms.Properties.Resources.cheers;
			this.pictureBox1.Location = new System.Drawing.Point(22, 224);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(6);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(761, 604);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// releaseNotesLbl
			// 
			this.releaseNotesLbl.AutoSize = true;
			this.releaseNotesLbl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.releaseNotesLbl.Location = new System.Drawing.Point(22, 26);
			this.releaseNotesLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.releaseNotesLbl.Name = "releaseNotesLbl";
			this.releaseNotesLbl.Size = new System.Drawing.Size(343, 41);
			this.releaseNotesLbl.TabIndex = 2;
			this.releaseNotesLbl.TabStop = true;
			this.releaseNotesLbl.Text = "Libation Classic v11.0.0.0";
			// 
			// checkForUpgradeBtn
			// 
			this.checkForUpgradeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkForUpgradeBtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.checkForUpgradeBtn.Location = new System.Drawing.Point(22, 115);
			this.checkForUpgradeBtn.Margin = new System.Windows.Forms.Padding(6);
			this.checkForUpgradeBtn.Name = "checkForUpgradeBtn";
			this.checkForUpgradeBtn.Size = new System.Drawing.Size(761, 66);
			this.checkForUpgradeBtn.TabIndex = 3;
			this.checkForUpgradeBtn.Text = "Check for Upgrade";
			this.checkForUpgradeBtn.UseVisualStyleBackColor = true;
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.listView1.Location = new System.Drawing.Point(22, 947);
			this.listView1.Margin = new System.Windows.Forms.Padding(6);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(758, 219);
			this.listView1.TabIndex = 4;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Assembly";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Version";
			// 
			// copyBtn
			// 
			this.copyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.copyBtn.Location = new System.Drawing.Point(565, 885);
			this.copyBtn.Margin = new System.Windows.Forms.Padding(6);
			this.copyBtn.Name = "copyBtn";
			this.copyBtn.Size = new System.Drawing.Size(219, 49);
			this.copyBtn.TabIndex = 5;
			this.copyBtn.Text = "Copy to Clipboard";
			this.copyBtn.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(22, 894);
			this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(217, 32);
			this.label2.TabIndex = 6;
			this.label2.Text = "Loaded Assemblies";
			// 
			// getLibationLbl
			// 
			this.getLibationLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.getLibationLbl.AutoSize = true;
			this.getLibationLbl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.getLibationLbl.Location = new System.Drawing.Point(455, 26);
			this.getLibationLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.getLibationLbl.Name = "getLibationLbl";
			this.getLibationLbl.Size = new System.Drawing.Size(325, 41);
			this.getLibationLbl.TabIndex = 7;
			this.getLibationLbl.TabStop = true;
			this.getLibationLbl.Text = "https://getlibation.com";
			this.getLibationLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.getLibationLbl_LinkClicked);
			// 
			// rmcrackanLbl
			// 
			this.rmcrackanLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rmcrackanLbl.AutoSize = true;
			this.rmcrackanLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.rmcrackanLbl.Location = new System.Drawing.Point(45, 567);
			this.rmcrackanLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.rmcrackanLbl.Name = "rmcrackanLbl";
			this.rmcrackanLbl.Size = new System.Drawing.Size(123, 32);
			this.rmcrackanLbl.TabIndex = 8;
			this.rmcrackanLbl.TabStop = true;
			this.rmcrackanLbl.Text = "rmcrackan";
			this.rmcrackanLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.rmcrackanLbl_LinkClicked);
			// 
			// MBucariLbl
			// 
			this.MBucariLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MBucariLbl.AutoSize = true;
			this.MBucariLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.MBucariLbl.Location = new System.Drawing.Point(665, 567);
			this.MBucariLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.MBucariLbl.Name = "MBucariLbl";
			this.MBucariLbl.Size = new System.Drawing.Size(101, 32);
			this.MBucariLbl.TabIndex = 9;
			this.MBucariLbl.TabStop = true;
			this.MBucariLbl.Text = "MBucari";
			this.MBucariLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MBucariLbl_LinkClicked);
			// 
			// AboutDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(806, 1197);
			this.Controls.Add(this.MBucariLbl);
			this.Controls.Add(this.rmcrackanLbl);
			this.Controls.Add(this.getLibationLbl);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.copyBtn);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.checkForUpgradeBtn);
			this.Controls.Add(this.releaseNotesLbl);
			this.Controls.Add(this.pictureBox1);
			this.Margin = new System.Windows.Forms.Padding(6);
			this.MinimumSize = new System.Drawing.Size(813, 1200);
			this.Name = "AboutDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About Libation";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.LinkLabel releaseNotesLbl;
		private System.Windows.Forms.Button checkForUpgradeBtn;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button copyBtn;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.LinkLabel getLibationLbl;
		private System.Windows.Forms.LinkLabel rmcrackanLbl;
		private System.Windows.Forms.LinkLabel MBucariLbl;
	}
}