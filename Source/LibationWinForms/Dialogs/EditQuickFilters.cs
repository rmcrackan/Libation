using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibationFileManager;

namespace LibationWinForms.Dialogs
{
	public class DisableButtonColumn : DataGridViewButtonColumn
	{
		public DisableButtonColumn()
		{
			CellTemplate = new EditQuickFilters.DisableButtonCell();
		}
	}

	public partial class EditQuickFilters : Form
	{
		private const string BLACK_UP_POINTING_TRIANGLE = "\u25B2";
		private const string BLACK_DOWN_POINTING_TRIANGLE = "\u25BC";
		private const string COL_Original = nameof(Original);
		private const string COL_Delete = nameof(Delete);
		private const string COL_Filter = nameof(Filter);
		private const string COL_MoveUp = nameof(MoveUp);
		private const string COL_MoveDown = nameof(MoveDown);

		internal class DisableButtonCell : DataGridViewButtonCell
		{
			protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
			{
				if ((OwningColumn.Name == COL_MoveUp && rowIndex == 0)
					|| (OwningColumn.Name == COL_MoveDown && rowIndex == LastRowIndex)
					|| OwningRow.IsNewRow)
				{
					base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts ^ (DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.SelectionBackground));

					ButtonRenderer.DrawButton(graphics, cellBounds, value as string, cellStyle.Font, false, System.Windows.Forms.VisualStyles.PushButtonState.Disabled);
				}
				else
					base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
			}

			int LastRowIndex => DataGridView.Rows[^1].IsNewRow ? DataGridView.Rows[^1].Index - 1 : DataGridView.Rows[^1].Index;
		}

		public EditQuickFilters()
		{
			InitializeComponent();

			dataGridView1.Columns[COL_Filter].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			populateGridValues();
			this.SetLibationIcon();
		}

		private void populateGridValues()
		{
			var filters = QuickFilters.Filters;
			if (!filters.Any())
				return;

			foreach (var filter in filters)
				dataGridView1.Rows.Add(filter, "X", filter, BLACK_UP_POINTING_TRIANGLE, BLACK_DOWN_POINTING_TRIANGLE);
		}

		private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
		{
			e.Row.Cells[COL_Delete].Value = "X";
			e.Row.Cells[COL_MoveUp].Value = BLACK_UP_POINTING_TRIANGLE;
			e.Row.Cells[COL_MoveDown].Value = BLACK_DOWN_POINTING_TRIANGLE;
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var list = dataGridView1.Rows
				.OfType<DataGridViewRow>()
				.Select(r => r.Cells[COL_Filter].Value?.ToString())
				.ToList();
			QuickFilters.ReplaceAll(list);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			var dgv = (DataGridView)sender;

			var col = dgv.Columns[e.ColumnIndex];
			if (col is DataGridViewButtonColumn && e.RowIndex >= 0)
			{
				var row = dgv.Rows[e.RowIndex];
				switch (col.Name)
				{
					case COL_Delete:
						// if final/edit row: do nothing
						if (e.RowIndex < dgv.RowCount - 1)
							dgv.Rows.Remove(row);
						break;
					case COL_MoveUp:
						// if top: do nothing
						if (e.RowIndex < 1)
							break;
						dgv.Rows.Remove(row);
						dgv.Rows.Insert(e.RowIndex - 1, row);
						break;
					case COL_MoveDown:
						// if final/edit row or bottom filter row: do nothing
						if (e.RowIndex >= dgv.RowCount - 2)
							break;
						dgv.Rows.Remove(row);
						dgv.Rows.Insert(e.RowIndex + 1, row);
						break;
				}
			}
		}
	}
}
