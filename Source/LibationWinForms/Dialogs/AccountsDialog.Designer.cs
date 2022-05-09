namespace LibationWinForms.Dialogs
{
	partial class AccountsDialog
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
			this.cancelBtn = new System.Windows.Forms.Button();
			this.saveBtn = new System.Windows.Forms.Button();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.DeleteAccount = new System.Windows.Forms.DataGridViewButtonColumn();
			this.LibraryScan = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.AccountId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Locale = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.AccountName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(713, 415);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 2;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.Location = new System.Drawing.Point(612, 415);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(75, 23);
			this.saveBtn.TabIndex = 1;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
			// 
			// dataGridView1
			// 
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DeleteAccount,
            this.LibraryScan,
            this.AccountId,
            this.Locale,
            this.AccountName});
			this.dataGridView1.Location = new System.Drawing.Point(12, 12);
			this.dataGridView1.MultiSelect = false;
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(776, 397);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellContentClick);
			this.dataGridView1.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView1_DefaultValuesNeeded);
			// 
			// DeleteAccount
			// 
			this.DeleteAccount.HeaderText = "Delete";
			this.DeleteAccount.Name = "DeleteAccount";
			this.DeleteAccount.ReadOnly = true;
			this.DeleteAccount.Text = "x";
			this.DeleteAccount.Width = 44;
			// 
			// LibraryScan
			// 
			this.LibraryScan.HeaderText = "Include in library scan?";
			this.LibraryScan.Name = "LibraryScan";
			this.LibraryScan.Width = 83;
			// 
			// AccountId
			// 
			this.AccountId.HeaderText = "Audible email/login";
			this.AccountId.Name = "AccountId";
			this.AccountId.Width = 111;
			// 
			// Locale
			// 
			this.Locale.HeaderText = "Locale";
			this.Locale.Name = "Locale";
			this.Locale.Width = 45;
			// 
			// AccountName
			// 
			this.AccountName.HeaderText = "Account nickname (optional)";
			this.AccountName.Name = "AccountName";
			this.AccountName.Width = 152;
			// 
			// AccountsDialog
			// 
			this.AcceptButton = this.saveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.saveBtn);
			this.Controls.Add(this.cancelBtn);
			this.Name = "AccountsDialog";
			this.Text = "Audible Accounts";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewButtonColumn DeleteAccount;
		private System.Windows.Forms.DataGridViewCheckBoxColumn LibraryScan;
		private System.Windows.Forms.DataGridViewTextBoxColumn AccountId;
		private System.Windows.Forms.DataGridViewComboBoxColumn Locale;
		private System.Windows.Forms.DataGridViewTextBoxColumn AccountName;
	}
}