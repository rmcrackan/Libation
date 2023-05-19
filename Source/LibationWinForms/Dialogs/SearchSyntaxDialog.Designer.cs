namespace LibationWinForms.Dialogs
{
	partial class SearchSyntaxDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchSyntaxDialog));
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			closeBtn = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(14, 10);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(410, 60);
			label1.TabIndex = 0;
			label1.Text = "Full Lucene query syntax is supported\r\nFields with similar names are synomyns (eg: Author, Authors, AuthorNames)\r\n\r\nTAG FORMAT: [tagName]";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(14, 82);
			label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(129, 75);
			label2.TabIndex = 1;
			label2.Text = "STRING FIELDS\r\n\r\nSearch for wizard of oz:\r\n     title:oz\r\n     title:\"wizard of oz\"";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(272, 82);
			label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(224, 135);
			label3.TabIndex = 2;
			label3.Text = resources.GetString("label3.Text");
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(530, 82);
			label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(187, 60);
			label4.TabIndex = 3;
			label4.Text = "BOOLEAN (TRUE/FALSE) FIELDS\r\n\r\nFind books that you haven't rated:\r\n     -IsRated";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(785, 82);
			label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(278, 90);
			label5.TabIndex = 4;
			label5.Text = "ID FIELDS\r\n\r\nAlice's Adventures in Wonderland (ID: B015D78L0U)\r\n     id:B015D78L0U\r\n\r\nAll of these are synonyms for the ID field";
			// 
			// closeBtn
			// 
			closeBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			closeBtn.Location = new System.Drawing.Point(1038, 537);
			closeBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			closeBtn.Name = "closeBtn";
			closeBtn.Size = new System.Drawing.Size(88, 27);
			closeBtn.TabIndex = 5;
			closeBtn.Text = "Close";
			closeBtn.UseVisualStyleBackColor = true;
			closeBtn.Click += CloseBtn_Click;
			// 
			// SearchSyntaxDialog
			// 
			AcceptButton = closeBtn;
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = closeBtn;
			ClientSize = new System.Drawing.Size(1140, 577);
			Controls.Add(closeBtn);
			Controls.Add(label5);
			Controls.Add(label4);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SearchSyntaxDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Filter options";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button closeBtn;
	}
}