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
            this.DeleteAccount = new DeleteColumn();
            this.ExportAccount = new ExportColumn();
            this.LibraryScan = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.AccountId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Locale = new LocaleColumn();
            this.AccountName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.importBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(832, 479);
            this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(88, 27);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(714, 479);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(88, 27);
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
            this.ExportAccount,
            this.LibraryScan,
            this.AccountId,
            this.Locale,
            this.AccountName});
            this.dataGridView1.Location = new System.Drawing.Point(14, 14);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(905, 458);
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
            this.DeleteAccount.Width = 46;
            // 
            // ExportAccount
            // 
            this.ExportAccount.HeaderText = "Export";
            this.ExportAccount.Name = "ExportAccount";
            this.ExportAccount.Text = "Export to audible-cli";
            this.ExportAccount.Width = 47;
            // 
            // LibraryScan
            // 
            this.LibraryScan.HeaderText = "Include in library scan?";
            this.LibraryScan.Name = "LibraryScan";
            this.LibraryScan.Width = 94;
            // 
            // AccountId
            // 
            this.AccountId.HeaderText = "Audible email/login";
            this.AccountId.Name = "AccountId";
            this.AccountId.Width = 125;
            // 
            // Locale
            // 
            this.Locale.HeaderText = "Locale";
            this.Locale.Name = "Locale";
            this.Locale.Width = 47;
            // 
            // AccountName
            // 
            this.AccountName.HeaderText = "Account nickname (optional)";
            this.AccountName.Name = "AccountName";
            this.AccountName.Width = 170;
            // 
            // importBtn
            // 
            this.importBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.importBtn.Location = new System.Drawing.Point(14, 480);
            this.importBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importBtn.Name = "importBtn";
            this.importBtn.Size = new System.Drawing.Size(156, 27);
            this.importBtn.TabIndex = 1;
            this.importBtn.Text = "Import from audible-cli";
            this.importBtn.UseVisualStyleBackColor = true;
            this.importBtn.Click += new System.EventHandler(this.importBtn_Click);
            // 
            // AccountsDialog
            // 
            this.AcceptButton = this.saveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(933, 519);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.importBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.cancelBtn);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "AccountsDialog";
            this.Text = "Audible Accounts";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button saveBtn;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.Button importBtn;
		private DeleteColumn DeleteAccount;
		private ExportColumn ExportAccount;
		private System.Windows.Forms.DataGridViewCheckBoxColumn LibraryScan;
		private System.Windows.Forms.DataGridViewTextBoxColumn AccountId;
		private LocaleColumn Locale;
		private System.Windows.Forms.DataGridViewTextBoxColumn AccountName;
	}
}