namespace WinFormsDesigner
{
    partial class WebsiteProcessorControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AuthGb = new System.Windows.Forms.GroupBox();
            this.AuthRb_Browserless = new System.Windows.Forms.RadioButton();
            this.AuthRb_UseCanonicalChrome = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.AuthRb_ManualLogin = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.PasswordTb = new System.Windows.Forms.TextBox();
            this.UsernameTb = new System.Windows.Forms.TextBox();
            this.AuthGb.SuspendLayout();
            this.SuspendLayout();
            // 
            // AuthGb
            // 
            this.AuthGb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AuthGb.Controls.Add(this.AuthRb_Browserless);
            this.AuthGb.Controls.Add(this.AuthRb_UseCanonicalChrome);
            this.AuthGb.Controls.Add(this.label3);
            this.AuthGb.Controls.Add(this.AuthRb_ManualLogin);
            this.AuthGb.Controls.Add(this.label2);
            this.AuthGb.Controls.Add(this.PasswordTb);
            this.AuthGb.Controls.Add(this.UsernameTb);
            this.AuthGb.Location = new System.Drawing.Point(0, 0);
            this.AuthGb.Name = "AuthGb";
            this.AuthGb.Size = new System.Drawing.Size(324, 137);
            this.AuthGb.TabIndex = 1;
            this.AuthGb.TabStop = false;
            this.AuthGb.Text = "Authentication";
            // 
            // AuthRb_Browserless
            // 
            this.AuthRb_Browserless.AutoSize = true;
            this.AuthRb_Browserless.Checked = true;
            this.AuthRb_Browserless.Location = new System.Drawing.Point(6, 19);
            this.AuthRb_Browserless.Name = "AuthRb_Browserless";
            this.AuthRb_Browserless.Size = new System.Drawing.Size(143, 17);
            this.AuthRb_Browserless.TabIndex = 0;
            this.AuthRb_Browserless.TabStop = true;
            this.AuthRb_Browserless.Text = "Browserless with cookies";
            this.AuthRb_Browserless.UseVisualStyleBackColor = true;
            // 
            // AuthRb_UseCanonicalChrome
            // 
            this.AuthRb_UseCanonicalChrome.AutoSize = true;
            this.AuthRb_UseCanonicalChrome.Location = new System.Drawing.Point(6, 114);
            this.AuthRb_UseCanonicalChrome.Name = "AuthRb_UseCanonicalChrome";
            this.AuthRb_UseCanonicalChrome.Size = new System.Drawing.Size(216, 17);
            this.AuthRb_UseCanonicalChrome.TabIndex = 6;
            this.AuthRb_UseCanonicalChrome.Text = "Use Canonical Chrome. SEE WARNING";
            this.AuthRb_UseCanonicalChrome.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password";
            // 
            // AuthRb_ManualLogin
            // 
            this.AuthRb_ManualLogin.AutoSize = true;
            this.AuthRb_ManualLogin.Location = new System.Drawing.Point(6, 42);
            this.AuthRb_ManualLogin.Name = "AuthRb_ManualLogin";
            this.AuthRb_ManualLogin.Size = new System.Drawing.Size(89, 17);
            this.AuthRb_ManualLogin.TabIndex = 1;
            this.AuthRb_ManualLogin.Text = "Manual Login";
            this.AuthRb_ManualLogin.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Username/Email";
            // 
            // PasswordTb
            // 
            this.PasswordTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PasswordTb.Location = new System.Drawing.Point(118, 88);
            this.PasswordTb.Name = "PasswordTb";
            this.PasswordTb.PasswordChar = '*';
            this.PasswordTb.Size = new System.Drawing.Size(200, 20);
            this.PasswordTb.TabIndex = 5;
            // 
            // UsernameTb
            // 
            this.UsernameTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UsernameTb.Location = new System.Drawing.Point(118, 62);
            this.UsernameTb.Name = "UsernameTb";
            this.UsernameTb.Size = new System.Drawing.Size(200, 20);
            this.UsernameTb.TabIndex = 3;
            // 
            // WebsiteProcessorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AuthGb);
            this.Name = "WebsiteProcessorControl";
            this.Size = new System.Drawing.Size(324, 137);
            this.AuthGb.ResumeLayout(false);
            this.AuthGb.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox AuthGb;
        private System.Windows.Forms.RadioButton AuthRb_UseCanonicalChrome;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton AuthRb_ManualLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PasswordTb;
        private System.Windows.Forms.TextBox UsernameTb;
        private System.Windows.Forms.RadioButton AuthRb_Browserless;
    }
}
