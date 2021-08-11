using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsDesigner
{
	// INSTRUCTIONS TO UPDATE DATA_GRID_VIEW
	// - delete current DataGridView
	// - view > other windows > data sources
	// - refresh
	// OR
	// - Add New Data Source
	//   Object. Next
	//   WinFormsDesigner
	//     AudibleDTO
	//       GridEntry
	// - go to Design view
	// - click on Data Sources > ProductItem. drowdown: DataGridView
	// - drag/drop ProductItem on design surface
	public partial class ProductsGrid : UserControl
	{
		public ProductsGrid()
		{
			InitializeComponent();
		}
	}
}
