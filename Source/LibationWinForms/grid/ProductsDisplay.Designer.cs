namespace LibationWinForms
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
			this.productsGrid = new LibationWinForms.grid.ProductsGrid();
			this.SuspendLayout();
			// 
			// productsGrid
			// 
			this.productsGrid.AutoScroll = true;
			this.productsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.productsGrid.Location = new System.Drawing.Point(0, 0);
			this.productsGrid.Name = "productsGrid";
			this.productsGrid.Size = new System.Drawing.Size(1510, 380);
			this.productsGrid.TabIndex = 0;
			this.productsGrid.LiberateClicked += new LibationWinForms.grid.ProductsGrid.LibraryBookEntryClickedEventHandler(this.productsGrid_LiberateClicked);
			this.productsGrid.CoverClicked += new LibationWinForms.grid.ProductsGrid.LibraryBookEntryClickedEventHandler(this.productsGrid_CoverClicked);
			this.productsGrid.DetailsClicked += new LibationWinForms.grid.ProductsGrid.LibraryBookEntryClickedEventHandler(this.productsGrid_DetailsClicked);
			this.productsGrid.DescriptionClicked += new LibationWinForms.grid.ProductsGrid.LibraryBookEntryRectangleClickedEventHandler(this.productsGrid_DescriptionClicked);
			this.productsGrid.VisibleCountChanged += new System.EventHandler<int>(this.productsGrid_VisibleCountChanged);
			// 
			// ProductsDisplay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.productsGrid);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "ProductsDisplay";
			this.Size = new System.Drawing.Size(1510, 380);
			this.ResumeLayout(false);

		}

		#endregion

		private grid.ProductsGrid productsGrid;
	}
}
