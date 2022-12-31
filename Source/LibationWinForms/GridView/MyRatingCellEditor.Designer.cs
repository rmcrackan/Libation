namespace LibationWinForms.GridView
{
	partial class MyRatingCellEditor
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
			this.lblOverall = new System.Windows.Forms.Label();
			this.lblPerform = new System.Windows.Forms.Label();
			this.lblStory = new System.Windows.Forms.Label();
			this.panelOverall = new System.Windows.Forms.Panel();
			this.noBorderLabel1 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel2 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel3 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel4 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel5 = new LibationWinForms.GridView.NoBorderLabel();
			this.panelPerform = new System.Windows.Forms.Panel();
			this.noBorderLabel6 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel7 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel8 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel9 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel10 = new LibationWinForms.GridView.NoBorderLabel();
			this.panelStory = new System.Windows.Forms.Panel();
			this.noBorderLabel11 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel12 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel13 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel14 = new LibationWinForms.GridView.NoBorderLabel();
			this.noBorderLabel15 = new LibationWinForms.GridView.NoBorderLabel();
			this.panelOverall.SuspendLayout();
			this.panelPerform.SuspendLayout();
			this.panelStory.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblOverall
			// 
			this.lblOverall.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblOverall.AutoSize = true;
			this.lblOverall.Location = new System.Drawing.Point(0, 1);
			this.lblOverall.Margin = new System.Windows.Forms.Padding(0);
			this.lblOverall.Name = "lblOverall";
			this.lblOverall.Size = new System.Drawing.Size(47, 15);
			this.lblOverall.TabIndex = 6;
			this.lblOverall.Text = "Overall:";
			// 
			// lblPerform
			// 
			this.lblPerform.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblPerform.AutoSize = true;
			this.lblPerform.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.lblPerform.Location = new System.Drawing.Point(0, 16);
			this.lblPerform.Margin = new System.Windows.Forms.Padding(0);
			this.lblPerform.Name = "lblPerform";
			this.lblPerform.Size = new System.Drawing.Size(53, 15);
			this.lblPerform.TabIndex = 8;
			this.lblPerform.Text = "Perform:";
			// 
			// lblStory
			// 
			this.lblStory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblStory.AutoSize = true;
			this.lblStory.Location = new System.Drawing.Point(0, 31);
			this.lblStory.Margin = new System.Windows.Forms.Padding(0);
			this.lblStory.Name = "lblStory";
			this.lblStory.Size = new System.Drawing.Size(37, 15);
			this.lblStory.TabIndex = 10;
			this.lblStory.Text = "Story:";
			// 
			// panelOverall
			// 
			this.panelOverall.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.panelOverall.Controls.Add(this.noBorderLabel1);
			this.panelOverall.Controls.Add(this.noBorderLabel2);
			this.panelOverall.Controls.Add(this.noBorderLabel3);
			this.panelOverall.Controls.Add(this.noBorderLabel4);
			this.panelOverall.Controls.Add(this.noBorderLabel5);
			this.panelOverall.Location = new System.Drawing.Point(52, 4);
			this.panelOverall.Name = "panelOverall";
			this.panelOverall.Size = new System.Drawing.Size(50, 11);
			this.panelOverall.TabIndex = 5;
			// 
			// noBorderLabel1
			// 
			this.noBorderLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel1.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel1.Location = new System.Drawing.Point(0, 0);
			this.noBorderLabel1.Name = "noBorderLabel1";
			this.noBorderLabel1.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel1.TabIndex = 0;
			this.noBorderLabel1.Text = "☆";
			this.noBorderLabel1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel1.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel1.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel2
			// 
			this.noBorderLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel2.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel2.Location = new System.Drawing.Point(10, 0);
			this.noBorderLabel2.Name = "noBorderLabel2";
			this.noBorderLabel2.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel2.TabIndex = 0;
			this.noBorderLabel2.Text = "☆";
			this.noBorderLabel2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel2.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel2.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel3
			// 
			this.noBorderLabel3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel3.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel3.Location = new System.Drawing.Point(20, 0);
			this.noBorderLabel3.Name = "noBorderLabel3";
			this.noBorderLabel3.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel3.TabIndex = 0;
			this.noBorderLabel3.Text = "☆";
			this.noBorderLabel3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel3.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel3.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel4
			// 
			this.noBorderLabel4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel4.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel4.Location = new System.Drawing.Point(30, 0);
			this.noBorderLabel4.Name = "noBorderLabel4";
			this.noBorderLabel4.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel4.TabIndex = 0;
			this.noBorderLabel4.Text = "☆";
			this.noBorderLabel4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel4.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel4.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel5
			// 
			this.noBorderLabel5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel5.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel5.Location = new System.Drawing.Point(40, 0);
			this.noBorderLabel5.Name = "noBorderLabel5";
			this.noBorderLabel5.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel5.TabIndex = 0;
			this.noBorderLabel5.Text = "☆";
			this.noBorderLabel5.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel5.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel5.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// panelPerform
			// 
			this.panelPerform.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.panelPerform.Controls.Add(this.noBorderLabel6);
			this.panelPerform.Controls.Add(this.noBorderLabel7);
			this.panelPerform.Controls.Add(this.noBorderLabel8);
			this.panelPerform.Controls.Add(this.noBorderLabel9);
			this.panelPerform.Controls.Add(this.noBorderLabel10);
			this.panelPerform.Location = new System.Drawing.Point(52, 19);
			this.panelPerform.Name = "panelPerform";
			this.panelPerform.Size = new System.Drawing.Size(50, 11);
			this.panelPerform.TabIndex = 6;
			// 
			// noBorderLabel6
			// 
			this.noBorderLabel6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel6.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel6.Location = new System.Drawing.Point(0, 0);
			this.noBorderLabel6.Name = "noBorderLabel6";
			this.noBorderLabel6.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel6.TabIndex = 0;
			this.noBorderLabel6.Text = "☆";
			this.noBorderLabel6.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel6.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel6.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel7
			// 
			this.noBorderLabel7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel7.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel7.Location = new System.Drawing.Point(10, 0);
			this.noBorderLabel7.Name = "noBorderLabel7";
			this.noBorderLabel7.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel7.TabIndex = 0;
			this.noBorderLabel7.Text = "☆";
			this.noBorderLabel7.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel7.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel7.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel8
			// 
			this.noBorderLabel8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel8.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel8.Location = new System.Drawing.Point(20, 0);
			this.noBorderLabel8.Name = "noBorderLabel8";
			this.noBorderLabel8.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel8.TabIndex = 0;
			this.noBorderLabel8.Text = "☆";
			this.noBorderLabel8.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel8.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel8.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel9
			// 
			this.noBorderLabel9.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel9.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel9.Location = new System.Drawing.Point(30, 0);
			this.noBorderLabel9.Name = "noBorderLabel9";
			this.noBorderLabel9.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel9.TabIndex = 0;
			this.noBorderLabel9.Text = "☆";
			this.noBorderLabel9.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel9.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel9.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel10
			// 
			this.noBorderLabel10.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel10.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel10.Location = new System.Drawing.Point(40, 0);
			this.noBorderLabel10.Name = "noBorderLabel10";
			this.noBorderLabel10.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel10.TabIndex = 0;
			this.noBorderLabel10.Text = "☆";
			this.noBorderLabel10.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel10.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel10.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// panelStory
			// 
			this.panelStory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.panelStory.Controls.Add(this.noBorderLabel11);
			this.panelStory.Controls.Add(this.noBorderLabel12);
			this.panelStory.Controls.Add(this.noBorderLabel13);
			this.panelStory.Controls.Add(this.noBorderLabel14);
			this.panelStory.Controls.Add(this.noBorderLabel15);
			this.panelStory.Location = new System.Drawing.Point(52, 34);
			this.panelStory.Name = "panelStory";
			this.panelStory.Size = new System.Drawing.Size(50, 11);
			this.panelStory.TabIndex = 6;
			// 
			// noBorderLabel11
			// 
			this.noBorderLabel11.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel11.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel11.Location = new System.Drawing.Point(0, 0);
			this.noBorderLabel11.Name = "noBorderLabel11";
			this.noBorderLabel11.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel11.TabIndex = 0;
			this.noBorderLabel11.Text = "☆";
			this.noBorderLabel11.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel11.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel11.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel12
			// 
			this.noBorderLabel12.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel12.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel12.Location = new System.Drawing.Point(10, 0);
			this.noBorderLabel12.Name = "noBorderLabel12";
			this.noBorderLabel12.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel12.TabIndex = 0;
			this.noBorderLabel12.Text = "☆";
			this.noBorderLabel12.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel12.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel12.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel13
			// 
			this.noBorderLabel13.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel13.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel13.Location = new System.Drawing.Point(20, 0);
			this.noBorderLabel13.Name = "noBorderLabel13";
			this.noBorderLabel13.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel13.TabIndex = 0;
			this.noBorderLabel13.Text = "☆";
			this.noBorderLabel13.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel13.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel13.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel14
			// 
			this.noBorderLabel14.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel14.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel14.Location = new System.Drawing.Point(30, 0);
			this.noBorderLabel14.Name = "noBorderLabel14";
			this.noBorderLabel14.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel14.TabIndex = 0;
			this.noBorderLabel14.Text = "☆";
			this.noBorderLabel14.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel14.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel14.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// noBorderLabel15
			// 
			this.noBorderLabel15.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.noBorderLabel15.LabelOffset = new System.Drawing.Point(-3, -3);
			this.noBorderLabel15.Location = new System.Drawing.Point(40, 0);
			this.noBorderLabel15.Name = "noBorderLabel15";
			this.noBorderLabel15.Size = new System.Drawing.Size(10, 11);
			this.noBorderLabel15.TabIndex = 0;
			this.noBorderLabel15.Text = "☆";
			this.noBorderLabel15.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Star_MouseClick);
			this.noBorderLabel15.MouseEnter += new System.EventHandler(this.Star_MouseEnter);
			this.noBorderLabel15.MouseLeave += new System.EventHandler(this.Star_MouseLeave);
			// 
			// MyRatingCellEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelStory);
			this.Controls.Add(this.panelPerform);
			this.Controls.Add(this.lblStory);
			this.Controls.Add(this.lblPerform);
			this.Controls.Add(this.lblOverall);
			this.Controls.Add(this.panelOverall);
			this.Name = "MyRatingCellEditor";
			this.Size = new System.Drawing.Size(110, 46);
			this.panelOverall.ResumeLayout(false);
			this.panelPerform.ResumeLayout(false);
			this.panelStory.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Panel panelOverall;
		private System.Windows.Forms.Label lblOverall;
		private System.Windows.Forms.Label lblPerform;
		private System.Windows.Forms.Label lblStory;
		private NoBorderLabel noBorderLabel1;
		private NoBorderLabel noBorderLabel5;
		private NoBorderLabel noBorderLabel4;
		private NoBorderLabel noBorderLabel3;
		private NoBorderLabel noBorderLabel2;
		private System.Windows.Forms.Panel panelPerform;
		private NoBorderLabel noBorderLabel6;
		private NoBorderLabel noBorderLabel7;
		private NoBorderLabel noBorderLabel8;
		private NoBorderLabel noBorderLabel9;
		private NoBorderLabel noBorderLabel10;
		private System.Windows.Forms.Panel panelStory;
		private NoBorderLabel noBorderLabel11;
		private NoBorderLabel noBorderLabel12;
		private NoBorderLabel noBorderLabel13;
		private NoBorderLabel noBorderLabel14;
		private NoBorderLabel noBorderLabel15;
	}
}
