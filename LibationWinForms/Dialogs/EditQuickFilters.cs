using FileManager;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class EditQuickFilters : Form
	{
		private const string BLACK_UP_POINTING_TRIANGLE = "\u25B2";
		private const string BLACK_DOWN_POINTING_TRIANGLE = "\u25BC";
		private const string COL_Original = nameof(Original);
		private const string COL_Delete = nameof(Delete);
		private const string COL_Filter = nameof(Filter);
		private const string COL_MoveUp = nameof(MoveUp);
		private const string COL_MoveDown = nameof(MoveDown);

		private Form1 _parent { get; }

		public EditQuickFilters(Form1 parent)
		{
			_parent = parent;

			InitializeComponent();

			dataGridView1.Columns[COL_Filter].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			populateGridValues();
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

			_parent.UpdateFilterDropDown();
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
