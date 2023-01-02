using Avalonia.Controls;
using System;
using System.Linq;

namespace LibationAvalonia.Controls
{
	public partial class DataGridTemplateColumnExt : DataGridTemplateColumn
	{
		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			cell?.AttachContextMenu();
			return base.GenerateElement(cell, dataItem);
		}
	}
}
