namespace LibationWinForms.Dialogs
{
	partial class TrashBinDialog
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
			label1 = new System.Windows.Forms.Label();
			restoreBtn = new System.Windows.Forms.Button();
			permanentlyDeleteBtn = new System.Windows.Forms.Button();
			everythingCb = new System.Windows.Forms.CheckBox();
			deletedCheckedLbl = new System.Windows.Forms.Label();
			productsGrid1 = new LibationWinForms.GridView.ProductsGrid();
			label2 = new System.Windows.Forms.Label();
			textBox1 = new System.Windows.Forms.TextBox();
			button1 = new System.Windows.Forms.Button();
			audiblePlusCb = new System.Windows.Forms.CheckBox();
			plusBookcSheckedLbl = new System.Windows.Forms.Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(388, 15);
			label1.TabIndex = 0;
			label1.Text = "Check books you want to permanently delete from or restore to Libation";
			// 
			// restoreBtn
			// 
			restoreBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			restoreBtn.Location = new System.Drawing.Point(572, 450);
			restoreBtn.Name = "restoreBtn";
			restoreBtn.Size = new System.Drawing.Size(75, 40);
			restoreBtn.TabIndex = 6;
			restoreBtn.Text = "Restore";
			restoreBtn.UseVisualStyleBackColor = true;
			restoreBtn.Click += restoreBtn_Click;
			// 
			// permanentlyDeleteBtn
			// 
			permanentlyDeleteBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			permanentlyDeleteBtn.Location = new System.Drawing.Point(653, 450);
			permanentlyDeleteBtn.Name = "permanentlyDeleteBtn";
			permanentlyDeleteBtn.Size = new System.Drawing.Size(135, 40);
			permanentlyDeleteBtn.TabIndex = 7;
			permanentlyDeleteBtn.Text = "Permanently Remove\r\nfrom Libation";
			permanentlyDeleteBtn.UseVisualStyleBackColor = true;
			permanentlyDeleteBtn.Click += permanentlyDeleteBtn_Click;
			// 
			// everythingCb
			// 
			everythingCb.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			everythingCb.AutoSize = true;
			everythingCb.Location = new System.Drawing.Point(12, 462);
			everythingCb.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			everythingCb.Name = "everythingCb";
			everythingCb.Size = new System.Drawing.Size(82, 19);
			everythingCb.TabIndex = 4;
			everythingCb.Text = "Everything";
			everythingCb.ThreeState = true;
			everythingCb.UseVisualStyleBackColor = true;
			everythingCb.CheckStateChanged += everythingCb_CheckStateChanged;
			// 
			// deletedCheckedLbl
			// 
			deletedCheckedLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			deletedCheckedLbl.AutoSize = true;
			deletedCheckedLbl.Location = new System.Drawing.Point(100, 463);
			deletedCheckedLbl.Name = "deletedCheckedLbl";
			deletedCheckedLbl.Size = new System.Drawing.Size(104, 15);
			deletedCheckedLbl.TabIndex = 0;
			deletedCheckedLbl.Text = "Checked: {0} of {1}";
			// 
			// productsGrid1
			// 
			productsGrid1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			productsGrid1.AutoScroll = true;
			productsGrid1.DisableColumnCustomization = true;
			productsGrid1.DisableContextMenu = true;
			productsGrid1.Location = new System.Drawing.Point(12, 62);
			productsGrid1.Name = "productsGrid1";
			productsGrid1.SearchEngine = null;
			productsGrid1.Size = new System.Drawing.Size(776, 382);
			productsGrid1.TabIndex = 3;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 36);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(123, 15);
			label2.TabIndex = 0;
			label2.Text = "Search Deleted Books:";
			// 
			// textBox1
			// 
			textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textBox1.Location = new System.Drawing.Point(141, 33);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(574, 23);
			textBox1.TabIndex = 1;
			textBox1.KeyDown += textBox1_KeyDown;
			// 
			// button1
			// 
			button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			button1.Location = new System.Drawing.Point(721, 33);
			button1.Name = "button1";
			button1.Size = new System.Drawing.Size(67, 23);
			button1.TabIndex = 2;
			button1.Text = "Filter";
			button1.UseVisualStyleBackColor = true;
			button1.Click += searchBtn_Click;
			// 
			// audiblePlusCb
			// 
			audiblePlusCb.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			audiblePlusCb.AutoSize = true;
			audiblePlusCb.Location = new System.Drawing.Point(247, 462);
			audiblePlusCb.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			audiblePlusCb.Name = "audiblePlusCb";
			audiblePlusCb.Size = new System.Drawing.Size(127, 19);
			audiblePlusCb.TabIndex = 5;
			audiblePlusCb.Text = "Audible Plus Books";
			audiblePlusCb.ThreeState = true;
			audiblePlusCb.UseVisualStyleBackColor = true;
			audiblePlusCb.CheckStateChanged += audiblePlusCb_CheckStateChanged;
			// 
			// plusBookcSheckedLbl
			// 
			plusBookcSheckedLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			plusBookcSheckedLbl.AutoSize = true;
			plusBookcSheckedLbl.Location = new System.Drawing.Point(380, 463);
			plusBookcSheckedLbl.Name = "plusBookcSheckedLbl";
			plusBookcSheckedLbl.Size = new System.Drawing.Size(104, 15);
			plusBookcSheckedLbl.TabIndex = 0;
			plusBookcSheckedLbl.Text = "Checked: {0} of {1}";
			// 
			// TrashBinDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(800, 502);
			Controls.Add(plusBookcSheckedLbl);
			Controls.Add(button1);
			Controls.Add(textBox1);
			Controls.Add(label2);
			Controls.Add(productsGrid1);
			Controls.Add(deletedCheckedLbl);
			Controls.Add(audiblePlusCb);
			Controls.Add(everythingCb);
			Controls.Add(permanentlyDeleteBtn);
			Controls.Add(restoreBtn);
			Controls.Add(label1);
			Name = "TrashBinDialog";
			Text = "Trash Bin";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button restoreBtn;
		private System.Windows.Forms.Button permanentlyDeleteBtn;
		private System.Windows.Forms.CheckBox everythingCb;
		private System.Windows.Forms.Label deletedCheckedLbl;
		private GridView.ProductsGrid productsGrid1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox audiblePlusCb;
		private System.Windows.Forms.Label plusBookcSheckedLbl;
	}
}