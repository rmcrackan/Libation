namespace WinFormsDesigner.BookLiberation
{
    partial class NoLongerAvailableForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.missingBtn = new System.Windows.Forms.Button();
            this.abortBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(174, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "Book details download failed.\r\n{0} may be no longer available.\r\nVerify the book i" +
    "s still available here";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 51);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(384, 20);
            this.textBox1.TabIndex = 1;
            // 
            // missingBtn
            // 
            this.missingBtn.Location = new System.Drawing.Point(324, 77);
            this.missingBtn.Name = "missingBtn";
            this.missingBtn.Size = new System.Drawing.Size(75, 23);
            this.missingBtn.TabIndex = 3;
            this.missingBtn.Text = "Missing";
            this.missingBtn.UseVisualStyleBackColor = true;
            // 
            // abortBtn
            // 
            this.abortBtn.Location = new System.Drawing.Point(324, 126);
            this.abortBtn.Name = "abortBtn";
            this.abortBtn.Size = new System.Drawing.Size(75, 23);
            this.abortBtn.TabIndex = 5;
            this.abortBtn.Text = "Abort";
            this.abortBtn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(306, 26);
            this.label2.TabIndex = 2;
            this.label2.Text = "If the book is not available, click here to mark it as missing\r\nNo further book d" +
    "etails download will be attempted for this book";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 26);
            this.label3.TabIndex = 4;
            this.label3.Text = "If the book is actually available, click here\r\nto abort and try again later";
            // 
            // NoLongerAvailableForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 161);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.abortBtn);
            this.Controls.Add(this.missingBtn);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NoLongerAvailableForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "No Longer Available";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button missingBtn;
        private System.Windows.Forms.Button abortBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}