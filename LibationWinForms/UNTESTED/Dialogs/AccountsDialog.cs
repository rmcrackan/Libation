using System;
using System.Linq;
using System.Windows.Forms;
using FileManager;

namespace LibationWinForms.Dialogs
{
	public partial class AccountsDialog : Form
	{
		const string COL_Original = "Original";
		const string COL_Delete = "Delete";
		const string COL_Filter = "Filter";
		const string COL_MoveUp = "MoveUp";
		const string COL_MoveDown = "MoveDown";

		public AccountsDialog()
		{
			InitializeComponent();
		}

		private void cancelBtn_Click(object sender, EventArgs e) => this.Close();

		#region TEMP

		private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
		{



			//e.Row.Cells["Region"].Value = "WA";
			//e.Row.Cells["CustomerID"].Value = NewCustomerId();
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{

		}

		private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{

		}

		private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{

			//if (e.ColumnIndex == dataGridView1.Columns["ItemID"].Index)  //if the ItemID-cell is edited
			//{
			//	dataGridView1.Rows[e.RowIndex].ReadOnly = true;  // set all row as read-only
			//	dataGridView1.Rows[e.RowIndex].Cells["ItemID"].ReadOnly = false;  //except ItemID-cell
			//}
		}

		private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{

			//if (dataGridView1.Rows[e.RowIndex].Cells["ItemID"].Value != null)
			//{
			//	dataGridView1.Rows[e.RowIndex].ReadOnly = true;  // set all row as read-only
			//	dataGridView1.Rows[e.RowIndex].Cells["ItemID"].ReadOnly = false;  //except ItemID-cell
			//}
		}

		private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{

		}

		private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			//var dgv = (DataGridView)sender;

			//var col = dgv.Columns[e.ColumnIndex];
			//if (col is DataGridViewButtonColumn && e.RowIndex >= 0)
			//{
			//	var row = dgv.Rows[e.RowIndex];
			//	switch (col.Name)
			//	{
			//		case COL_Delete:
			//			// if final/edit row: do nothing
			//			if (e.RowIndex < dgv.RowCount - 1)
			//				dgv.Rows.Remove(row);
			//			break;
			//		case COL_MoveUp:
			//			// if top: do nothing
			//			if (e.RowIndex < 1)
			//				break;
			//			dgv.Rows.Remove(row);
			//			dgv.Rows.Insert(e.RowIndex - 1, row);
			//			break;
			//		case COL_MoveDown:
			//			// if final/edit row or bottom filter row: do nothing
			//			if (e.RowIndex >= dgv.RowCount - 2)
			//				break;
			//			dgv.Rows.Remove(row);
			//			dgv.Rows.Insert(e.RowIndex + 1, row);
			//			break;
			//	}
			//}
		}
		#endregion
	}
}
