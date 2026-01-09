using Avalonia.Controls;

namespace LibationAvalonia.Controls
{
	public partial class DataGridTemplateColumnExt : DataGridTemplateColumn
	{
		protected override Control GenerateElement(DataGridCell cell, object dataItem)
		{
			cell.Tag = this;
			return base.GenerateElement(cell, dataItem);
		}
	}
}
