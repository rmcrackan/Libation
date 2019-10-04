namespace ffmpeg_decrypt
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.inpbutton = new System.Windows.Forms.Button();
			this.outpbutton = new System.Windows.Forms.Button();
			this.inputdisplay = new System.Windows.Forms.TextBox();
			this.outputdisplay = new System.Windows.Forms.TextBox();
			this.convertbutton = new System.Windows.Forms.Button();
			this.txtConsole = new System.Windows.Forms.TextBox();
			this.qualityCombo = new System.Windows.Forms.ComboBox();
			this.setQualityLbl = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.statuslbl = new System.Windows.Forms.Label();
			this.rmp3 = new System.Windows.Forms.RadioButton();
			this.raac = new System.Windows.Forms.RadioButton();
			this.rflac = new System.Windows.Forms.RadioButton();
			this.inputPnl = new System.Windows.Forms.Panel();
			this.convertGb = new System.Windows.Forms.GroupBox();
			this.decryptConvertPnl = new System.Windows.Forms.Panel();
			this.convertRb = new System.Windows.Forms.RadioButton();
			this.decryptRb = new System.Windows.Forms.RadioButton();
			this.inputPnl.SuspendLayout();
			this.convertGb.SuspendLayout();
			this.decryptConvertPnl.SuspendLayout();
			this.SuspendLayout();
			// 
			// inpbutton
			// 
			this.inpbutton.Location = new System.Drawing.Point(306, 0);
			this.inpbutton.Name = "inpbutton";
			this.inpbutton.Size = new System.Drawing.Size(99, 23);
			this.inpbutton.TabIndex = 0;
			this.inpbutton.Text = "Choose .aax ...";
			this.inpbutton.UseVisualStyleBackColor = true;
			this.inpbutton.Click += new System.EventHandler(this.inpbutton_Click);
			// 
			// outpbutton
			// 
			this.outpbutton.Location = new System.Drawing.Point(306, 29);
			this.outpbutton.Name = "outpbutton";
			this.outpbutton.Size = new System.Drawing.Size(99, 23);
			this.outpbutton.TabIndex = 1;
			this.outpbutton.Text = "Set extract dir...";
			this.outpbutton.UseVisualStyleBackColor = true;
			this.outpbutton.Click += new System.EventHandler(this.outpbutton_Click);
			// 
			// inputdisplay
			// 
			this.inputdisplay.Location = new System.Drawing.Point(0, 2);
			this.inputdisplay.Name = "inputdisplay";
			this.inputdisplay.Size = new System.Drawing.Size(300, 20);
			this.inputdisplay.TabIndex = 2;
			// 
			// outputdisplay
			// 
			this.outputdisplay.Location = new System.Drawing.Point(0, 31);
			this.outputdisplay.Name = "outputdisplay";
			this.outputdisplay.Size = new System.Drawing.Size(300, 20);
			this.outputdisplay.TabIndex = 3;
			// 
			// convertbutton
			// 
			this.convertbutton.Enabled = false;
			this.convertbutton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.convertbutton.Location = new System.Drawing.Point(306, 58);
			this.convertbutton.Name = "convertbutton";
			this.convertbutton.Size = new System.Drawing.Size(99, 107);
			this.convertbutton.TabIndex = 6;
			this.convertbutton.Text = "Convert Audible Audio File";
			this.convertbutton.UseVisualStyleBackColor = true;
			this.convertbutton.Click += new System.EventHandler(this.convertbutton_Click);
			// 
			// txtConsole
			// 
			this.txtConsole.BackColor = System.Drawing.Color.Black;
			this.txtConsole.ForeColor = System.Drawing.Color.White;
			this.txtConsole.Location = new System.Drawing.Point(12, 199);
			this.txtConsole.Multiline = true;
			this.txtConsole.Name = "txtConsole";
			this.txtConsole.ReadOnly = true;
			this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtConsole.Size = new System.Drawing.Size(405, 184);
			this.txtConsole.TabIndex = 7;
			// 
			// qualityCombo
			// 
			this.qualityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.qualityCombo.FormattingEnabled = true;
			this.qualityCombo.Items.AddRange(new object[] {
			"32",
			"80",
			"96",
			"128",
			"160",
			"192",
			"256",
			"320"});
			this.qualityCombo.Location = new System.Drawing.Point(148, 41);
			this.qualityCombo.Name = "qualityCombo";
			this.qualityCombo.Size = new System.Drawing.Size(146, 21);
			this.qualityCombo.TabIndex = 9;
			// 
			// setQualityLbl
			// 
			this.setQualityLbl.AutoSize = true;
			this.setQualityLbl.Location = new System.Drawing.Point(145, 21);
			this.setQualityLbl.Name = "setQualityLbl";
			this.setQualityLbl.Size = new System.Drawing.Size(149, 13);
			this.setQualityLbl.TabIndex = 10;
			this.setQualityLbl.Text = "Set MP3/M4B Quality (kbit/s):";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 183);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 13);
			this.label1.TabIndex = 11;
			this.label1.Text = "Status:";
			// 
			// statuslbl
			// 
			this.statuslbl.AutoSize = true;
			this.statuslbl.Location = new System.Drawing.Point(58, 183);
			this.statuslbl.Name = "statuslbl";
			this.statuslbl.Size = new System.Drawing.Size(51, 13);
			this.statuslbl.TabIndex = 12;
			this.statuslbl.Text = "[statuslbl]";
			// 
			// rmp3
			// 
			this.rmp3.AutoSize = true;
			this.rmp3.Location = new System.Drawing.Point(6, 19);
			this.rmp3.Name = "rmp3";
			this.rmp3.Size = new System.Drawing.Size(76, 17);
			this.rmp3.TabIndex = 13;
			this.rmp3.Text = "MP3 audio";
			this.rmp3.UseVisualStyleBackColor = true;
			// 
			// raac
			// 
			this.raac.AutoSize = true;
			this.raac.Checked = true;
			this.raac.Location = new System.Drawing.Point(6, 42);
			this.raac.Name = "raac";
			this.raac.Size = new System.Drawing.Size(125, 17);
			this.raac.TabIndex = 14;
			this.raac.TabStop = true;
			this.raac.Text = "AAC M4B Audiobook";
			this.raac.UseVisualStyleBackColor = true;
			// 
			// rflac
			// 
			this.rflac.AutoSize = true;
			this.rflac.Location = new System.Drawing.Point(6, 65);
			this.rflac.Name = "rflac";
			this.rflac.Size = new System.Drawing.Size(99, 17);
			this.rflac.TabIndex = 15;
			this.rflac.Text = "FLAC HQ audio";
			this.rflac.UseVisualStyleBackColor = true;
			// 
			// inputPnl
			// 
			this.inputPnl.Controls.Add(this.convertGb);
			this.inputPnl.Controls.Add(this.decryptConvertPnl);
			this.inputPnl.Controls.Add(this.inputdisplay);
			this.inputPnl.Controls.Add(this.inpbutton);
			this.inputPnl.Controls.Add(this.outpbutton);
			this.inputPnl.Controls.Add(this.outputdisplay);
			this.inputPnl.Controls.Add(this.convertbutton);
			this.inputPnl.Location = new System.Drawing.Point(12, 12);
			this.inputPnl.Name = "inputPnl";
			this.inputPnl.Size = new System.Drawing.Size(405, 168);
			this.inputPnl.TabIndex = 16;
			// 
			// convertGb
			// 
			this.convertGb.Controls.Add(this.rmp3);
			this.convertGb.Controls.Add(this.setQualityLbl);
			this.convertGb.Controls.Add(this.rflac);
			this.convertGb.Controls.Add(this.qualityCombo);
			this.convertGb.Controls.Add(this.raac);
			this.convertGb.Location = new System.Drawing.Point(0, 80);
			this.convertGb.Name = "convertGb";
			this.convertGb.Size = new System.Drawing.Size(300, 85);
			this.convertGb.TabIndex = 14;
			this.convertGb.TabStop = false;
			this.convertGb.Text = "Convert options";
			// 
			// decryptConvertPnl
			// 
			this.decryptConvertPnl.Controls.Add(this.convertRb);
			this.decryptConvertPnl.Controls.Add(this.decryptRb);
			this.decryptConvertPnl.Location = new System.Drawing.Point(0, 58);
			this.decryptConvertPnl.Name = "decryptConvertPnl";
			this.decryptConvertPnl.Size = new System.Drawing.Size(266, 16);
			this.decryptConvertPnl.TabIndex = 13;
			// 
			// convertRb
			// 
			this.convertRb.AutoSize = true;
			this.convertRb.Location = new System.Drawing.Point(88, 0);
			this.convertRb.Name = "convertRb";
			this.convertRb.Size = new System.Drawing.Size(62, 17);
			this.convertRb.TabIndex = 1;
			this.convertRb.Text = "Convert";
			this.convertRb.UseVisualStyleBackColor = true;
			this.convertRb.CheckedChanged += new System.EventHandler(this.decryptConvertRb_CheckedChanged);
			// 
			// decryptRb
			// 
			this.decryptRb.AutoSize = true;
			this.decryptRb.Location = new System.Drawing.Point(0, 0);
			this.decryptRb.Name = "decryptRb";
			this.decryptRb.Size = new System.Drawing.Size(82, 17);
			this.decryptRb.TabIndex = 0;
			this.decryptRb.Text = "Just decrypt";
			this.decryptRb.UseVisualStyleBackColor = true;
			this.decryptRb.CheckedChanged += new System.EventHandler(this.decryptConvertRb_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(429, 395);
			this.Controls.Add(this.inputPnl);
			this.Controls.Add(this.txtConsole);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.statuslbl);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Open Source Audible Converter";
			this.inputPnl.ResumeLayout(false);
			this.inputPnl.PerformLayout();
			this.convertGb.ResumeLayout(false);
			this.convertGb.PerformLayout();
			this.decryptConvertPnl.ResumeLayout(false);
			this.decryptConvertPnl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button inpbutton;
		private System.Windows.Forms.Button outpbutton;
		private System.Windows.Forms.TextBox inputdisplay;
		private System.Windows.Forms.TextBox outputdisplay;
		private System.Windows.Forms.Button convertbutton;
		private System.Windows.Forms.TextBox txtConsole;
		private System.Windows.Forms.ComboBox qualityCombo;
		private System.Windows.Forms.Label setQualityLbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label statuslbl;
		private System.Windows.Forms.RadioButton rmp3;
		private System.Windows.Forms.RadioButton raac;
		private System.Windows.Forms.RadioButton rflac;
		private System.Windows.Forms.Panel inputPnl;
		private System.Windows.Forms.GroupBox convertGb;
		private System.Windows.Forms.Panel decryptConvertPnl;
		private System.Windows.Forms.RadioButton convertRb;
		private System.Windows.Forms.RadioButton decryptRb;
	}
}
