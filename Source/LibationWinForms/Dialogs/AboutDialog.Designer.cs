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
			versionLbl = new System.Windows.Forms.Label();
			releaseNotesLbl = new System.Windows.Forms.LinkLabel();
			checkForUpgradeBtn = new System.Windows.Forms.Button();
			listView1 = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			copyBtn = new System.Windows.Forms.Button();
			label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pictureBox1.Image = Properties.Resources.cheers;
			pictureBox1.Location = new System.Drawing.Point(12, 105);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(410, 283);
			pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			// 
			// versionLbl
			// 
			versionLbl.AutoSize = true;
			versionLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			versionLbl.Location = new System.Drawing.Point(12, 9);
			versionLbl.Name = "versionLbl";
			versionLbl.Size = new System.Drawing.Size(198, 21);
			versionLbl.TabIndex = 1;
			versionLbl.Text = "Libation Classic v11.0.0.0";
			// 
			// releaseNotesLbl
			// 
			releaseNotesLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			releaseNotesLbl.AutoSize = true;
			releaseNotesLbl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			releaseNotesLbl.Location = new System.Drawing.Point(319, 10);
			releaseNotesLbl.Name = "releaseNotesLbl";
			releaseNotesLbl.Size = new System.Drawing.Size(103, 20);
			releaseNotesLbl.TabIndex = 2;
			releaseNotesLbl.TabStop = true;
			releaseNotesLbl.Text = "Release Notes";
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
			// listView1
			// 
			listView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader2 });
			listView1.Location = new System.Drawing.Point(12, 444);
			listView1.Name = "listView1";
			listView1.Size = new System.Drawing.Size(410, 105);
			listView1.TabIndex = 4;
			listView1.UseCompatibleStateImageBehavior = false;
			listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Assembly";
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Version";
			// 
			// copyBtn
			// 
			copyBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			copyBtn.Location = new System.Drawing.Point(304, 415);
			copyBtn.Name = "copyBtn";
			copyBtn.Size = new System.Drawing.Size(118, 23);
			copyBtn.TabIndex = 5;
			copyBtn.Text = "Copy to Clipboard";
			copyBtn.UseVisualStyleBackColor = true;
			copyBtn.Click += copyBtn_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 419);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(108, 15);
			label2.TabIndex = 6;
			label2.Text = "Loaded Assemblies";
			// 
			// AboutDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(434, 561);
			Controls.Add(label2);
			Controls.Add(copyBtn);
			Controls.Add(listView1);
			Controls.Add(checkForUpgradeBtn);
			Controls.Add(releaseNotesLbl);
			Controls.Add(versionLbl);
			Controls.Add(pictureBox1);
			MinimumSize = new System.Drawing.Size(450, 600);
			Name = "AboutDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "About Libation";
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label versionLbl;
		private System.Windows.Forms.LinkLabel releaseNotesLbl;
		private System.Windows.Forms.Button checkForUpgradeBtn;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button copyBtn;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
	}
}