namespace LibationWinForms.Dialogs
{
    partial class BookDetailsDialog
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
			this.SaveBtn = new System.Windows.Forms.Button();
			this.newTagsTb = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// SaveBtn
			// 
			this.SaveBtn.Location = new System.Drawing.Point(462, 29);
			this.SaveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SaveBtn.Name = "SaveBtn";
			this.SaveBtn.Size = new System.Drawing.Size(88, 27);
			this.SaveBtn.TabIndex = 1;
			this.SaveBtn.Text = "Save";
			this.SaveBtn.UseVisualStyleBackColor = true;
			this.SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
			// 
			// newTagsTb
			// 
			this.newTagsTb.Location = new System.Drawing.Point(14, 31);
			this.newTagsTb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.newTagsTb.Name = "newTagsTb";
			this.newTagsTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.newTagsTb.Size = new System.Drawing.Size(437, 23);
			this.newTagsTb.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 10);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(458, 15);
			this.label1.TabIndex = 2;
			this.label1.Text = "Tags are separated by a space. Each tag can contain letters, numbers, and undersc" +
    "ores";
			// 
			// BookDetailsDialog
			// 
			this.AcceptButton = this.SaveBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(564, 69);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.newTagsTb);
			this.Controls.Add(this.SaveBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BookDetailsDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Book Details";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.TextBox newTagsTb;
        private System.Windows.Forms.Label label1;
    }
}