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
			tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			lboxIdFields = new System.Windows.Forms.ListBox();
			label9 = new System.Windows.Forms.Label();
			tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			lboxBoolFields = new System.Windows.Forms.ListBox();
			label8 = new System.Windows.Forms.Label();
			tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			lboxNumberFields = new System.Windows.Forms.ListBox();
			label7 = new System.Windows.Forms.Label();
			tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			lboxStringFields = new System.Windows.Forms.ListBox();
			label6 = new System.Windows.Forms.Label();
			label10 = new System.Windows.Forms.Label();
			label11 = new System.Windows.Forms.Label();
			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel5.SuspendLayout();
			tableLayoutPanel4.SuspendLayout();
			tableLayoutPanel3.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(14, 10);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(410, 30);
			label1.TabIndex = 0;
			label1.Text = "Full Lucene query syntax is supported\r\nFields with similar names are synomyns (eg: Author, Authors, AuthorNames)";
			// 
			// label2
			// 
			label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(48, 18);
			label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(129, 45);
			label2.TabIndex = 1;
			label2.Text = "Search for wizard of oz:\r\n     title:oz\r\n     title:\"wizard of oz\"";
			// 
			// label3
			// 
			label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(4, 18);
			label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(218, 120);
			label3.TabIndex = 2;
			label3.Text = resources.GetString("label3.Text");
			// 
			// label4
			// 
			label4.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(19, 18);
			label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(187, 30);
			label4.TabIndex = 3;
			label4.Text = "Find books that you haven't rated:\r\n     -IsRated";
			// 
			// label5
			// 
			label5.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(8, 18);
			label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(209, 90);
			label5.TabIndex = 4;
			label5.Text = "Alice's Adventures in Wonderland (ID: B015D78L0U)\r\n     id:B015D78L0U\r\n\r\nAll of these are synonyms for the ID field";
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tableLayoutPanel1.ColumnCount = 4;
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			tableLayoutPanel1.Controls.Add(tableLayoutPanel5, 3, 0);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 2, 0);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 0);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
			tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
			tableLayoutPanel1.Location = new System.Drawing.Point(12, 51);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 1;
			tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel1.Size = new System.Drawing.Size(928, 425);
			tableLayoutPanel1.TabIndex = 6;
			// 
			// tableLayoutPanel5
			// 
			tableLayoutPanel5.ColumnCount = 1;
			tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel5.Controls.Add(lboxIdFields, 0, 2);
			tableLayoutPanel5.Controls.Add(label5, 0, 1);
			tableLayoutPanel5.Controls.Add(label9, 0, 0);
			tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel5.Location = new System.Drawing.Point(699, 3);
			tableLayoutPanel5.Name = "tableLayoutPanel5";
			tableLayoutPanel5.RowCount = 3;
			tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel5.Size = new System.Drawing.Size(226, 419);
			tableLayoutPanel5.TabIndex = 10;
			// 
			// lboxIdFields
			// 
			lboxIdFields.Dock = System.Windows.Forms.DockStyle.Fill;
			lboxIdFields.FormattingEnabled = true;
			lboxIdFields.Location = new System.Drawing.Point(3, 111);
			lboxIdFields.Name = "lboxIdFields";
			lboxIdFields.Size = new System.Drawing.Size(220, 305);
			lboxIdFields.TabIndex = 0;
			// 
			// label9
			// 
			label9.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label9.AutoSize = true;
			label9.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
			label9.Location = new System.Drawing.Point(86, 0);
			label9.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(54, 15);
			label9.TabIndex = 7;
			label9.Text = "ID Fields";
			label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel4
			// 
			tableLayoutPanel4.ColumnCount = 1;
			tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel4.Controls.Add(lboxBoolFields, 0, 2);
			tableLayoutPanel4.Controls.Add(label4, 0, 1);
			tableLayoutPanel4.Controls.Add(label8, 0, 0);
			tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel4.Location = new System.Drawing.Point(467, 3);
			tableLayoutPanel4.Name = "tableLayoutPanel4";
			tableLayoutPanel4.RowCount = 3;
			tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel4.Size = new System.Drawing.Size(226, 419);
			tableLayoutPanel4.TabIndex = 9;
			// 
			// lboxBoolFields
			// 
			lboxBoolFields.Dock = System.Windows.Forms.DockStyle.Fill;
			lboxBoolFields.FormattingEnabled = true;
			lboxBoolFields.Location = new System.Drawing.Point(3, 51);
			lboxBoolFields.Name = "lboxBoolFields";
			lboxBoolFields.Size = new System.Drawing.Size(220, 365);
			lboxBoolFields.TabIndex = 0;
			// 
			// label8
			// 
			label8.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label8.AutoSize = true;
			label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
			label8.Location = new System.Drawing.Point(36, 0);
			label8.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(154, 15);
			label8.TabIndex = 7;
			label8.Text = "Boolean (True/False) Fields";
			label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel3
			// 
			tableLayoutPanel3.ColumnCount = 1;
			tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel3.Controls.Add(lboxNumberFields, 0, 2);
			tableLayoutPanel3.Controls.Add(label3, 0, 1);
			tableLayoutPanel3.Controls.Add(label7, 0, 0);
			tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel3.Location = new System.Drawing.Point(235, 3);
			tableLayoutPanel3.Name = "tableLayoutPanel3";
			tableLayoutPanel3.RowCount = 3;
			tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel3.Size = new System.Drawing.Size(226, 419);
			tableLayoutPanel3.TabIndex = 8;
			// 
			// lboxNumberFields
			// 
			lboxNumberFields.Dock = System.Windows.Forms.DockStyle.Fill;
			lboxNumberFields.FormattingEnabled = true;
			lboxNumberFields.Location = new System.Drawing.Point(3, 141);
			lboxNumberFields.Name = "lboxNumberFields";
			lboxNumberFields.Size = new System.Drawing.Size(220, 275);
			lboxNumberFields.TabIndex = 0;
			// 
			// label7
			// 
			label7.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label7.AutoSize = true;
			label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
			label7.Location = new System.Drawing.Point(69, 0);
			label7.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(87, 15);
			label7.TabIndex = 7;
			label7.Text = "Number Fields";
			label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel2
			// 
			tableLayoutPanel2.ColumnCount = 1;
			tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel2.Controls.Add(lboxStringFields, 0, 2);
			tableLayoutPanel2.Controls.Add(label2, 0, 1);
			tableLayoutPanel2.Controls.Add(label6, 0, 0);
			tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 3;
			tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel2.Size = new System.Drawing.Size(226, 419);
			tableLayoutPanel2.TabIndex = 7;
			// 
			// lboxStringFields
			// 
			lboxStringFields.Dock = System.Windows.Forms.DockStyle.Fill;
			lboxStringFields.FormattingEnabled = true;
			lboxStringFields.Location = new System.Drawing.Point(3, 66);
			lboxStringFields.Name = "lboxStringFields";
			lboxStringFields.Size = new System.Drawing.Size(220, 350);
			lboxStringFields.TabIndex = 0;
			// 
			// label6
			// 
			label6.Anchor = System.Windows.Forms.AnchorStyles.Top;
			label6.AutoSize = true;
			label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
			label6.Location = new System.Drawing.Point(75, 0);
			label6.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(75, 15);
			label6.TabIndex = 0;
			label6.Text = "String Fields";
			label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label10
			// 
			label10.AutoSize = true;
			label10.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
			label10.Location = new System.Drawing.Point(515, 25);
			label10.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
			label10.Name = "label10";
			label10.Size = new System.Drawing.Size(72, 15);
			label10.TabIndex = 7;
			label10.Text = "Tag Format:";
			label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label11
			// 
			label11.AutoSize = true;
			label11.Font = new System.Drawing.Font("Segoe UI", 9F);
			label11.Location = new System.Drawing.Point(596, 25);
			label11.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
			label11.Name = "label11";
			label11.Size = new System.Drawing.Size(64, 15);
			label11.TabIndex = 8;
			label11.Text = "[tagName]";
			label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SearchSyntaxDialog
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(952, 488);
			Controls.Add(label11);
			Controls.Add(label10);
			Controls.Add(tableLayoutPanel1);
			Controls.Add(label1);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SearchSyntaxDialog";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Filter options";
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel5.ResumeLayout(false);
			tableLayoutPanel5.PerformLayout();
			tableLayoutPanel4.ResumeLayout(false);
			tableLayoutPanel4.PerformLayout();
			tableLayoutPanel3.ResumeLayout(false);
			tableLayoutPanel3.PerformLayout();
			tableLayoutPanel2.ResumeLayout(false);
			tableLayoutPanel2.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
		private System.Windows.Forms.ListBox lboxIdFields;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		private System.Windows.Forms.ListBox lboxBoolFields;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.ListBox lboxNumberFields;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.ListBox lboxStringFields;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
	}
}