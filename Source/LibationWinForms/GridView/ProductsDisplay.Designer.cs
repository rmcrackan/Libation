namespace LibationWinForms.GridView
{
	partial class ProductsDisplay
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
			productsGrid = new ProductsGrid();
			SuspendLayout();
			// 
			// productsGrid
			// 
			productsGrid.AutoScroll = true;
			productsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			productsGrid.Location = new System.Drawing.Point(0, 0);
			productsGrid.Name = "productsGrid";
			productsGrid.Size = new System.Drawing.Size(1510, 380);
			productsGrid.TabIndex = 0;
			productsGrid.VisibleCountChanged += productsGrid_VisibleCountChanged;
			productsGrid.LiberateClicked += productsGrid_LiberateClicked;
			productsGrid.CoverClicked += productsGrid_CoverClicked;
			productsGrid.DetailsClicked += productsGrid_DetailsClicked;
			productsGrid.DescriptionClicked += productsGrid_DescriptionClicked;
			productsGrid.RemovableCountChanged += productsGrid_RemovableCountChanged;
			productsGrid.LiberateContextMenuStripNeeded += productsGrid_CellContextMenuStripNeeded;
			// 
			// ProductsDisplay
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			Controls.Add(productsGrid);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "ProductsDisplay";
			Size = new System.Drawing.Size(1510, 380);
			ResumeLayout(false);
		}

		#endregion

		private GridView.ProductsGrid productsGrid;
	}
}
