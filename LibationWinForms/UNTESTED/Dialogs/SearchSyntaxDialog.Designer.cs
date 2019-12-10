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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(358, 52);
            this.label1.TabIndex = 0;
            this.label1.Text = "Full Lucene query syntax is supported\r\nFields with similar names are synomyns (eg" +
    ": Author, Authors, AuthorNames)\r\n\r\nTAG FORMAT: [tagName]";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 65);
            this.label2.TabIndex = 1;
            this.label2.Text = "STRING FIELDS\r\n\r\nSearch for wizard of oz:\r\n     title:oz\r\n     title:\"wizard of o" +
    "z\"";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(233, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(195, 78);
            this.label3.TabIndex = 2;
            this.label3.Text = "NUMBER FIELDS\r\n\r\nFind books between 1-100 minutes long\r\n     length:[1 TO 100]\r\nF" +
    "ind books exactly 1 hr long\r\n     length:60";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(454, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(168, 52);
            this.label4.TabIndex = 3;
            this.label4.Text = "BOOL FIELDS\r\n\r\nFind books that you haven\'t rated:\r\n     -IsRated";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(673, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(257, 78);
            this.label5.TabIndex = 4;
            this.label5.Text = "ID FIELDS\r\n\r\nAlice\'s Adventures in Wonderland (ID: B015D78L0U)\r\n     id:B015D78L0" +
    "U\r\n\r\nAll of these are synonyms for the ID field";
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.Location = new System.Drawing.Point(890, 415);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(75, 23);
            this.closeBtn.TabIndex = 5;
            this.closeBtn.Text = "Close";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // SearchSyntaxDialog
            // 
            this.AcceptButton = this.closeBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeBtn;
            this.ClientSize = new System.Drawing.Size(977, 450);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchSyntaxDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter options";
            this.ResumeLayout(false);
            this.PerformLayout();

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