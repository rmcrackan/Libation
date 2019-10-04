using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileManager;

namespace LibationWinForm.Dialogs
{
    public partial class EditQuickFilters : Form
    {
        const string COL_Original = "Original";
        const string COL_Delete = "Delete";
        const string COL_Filter = "Filter";
        const string COL_MoveUp = "MoveUp";
        const string COL_MoveDown = "MoveDown";

        Form1 _parent { get; }

        public EditQuickFilters(Form1 parent)
        {
            _parent = parent;

            InitializeComponent();

            dataGridView1.Columns[COL_Filter].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            populateFilters();
        }

        private void populateFilters()
        {
            var filters = QuickFilters.Filters;
            if (!filters.Any())
                return;

            foreach (var filter in filters)
                dataGridView1.Rows.Add(filter, "X", filter, "\u25B2", "\u25BC");
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            var list = dataGridView1.Rows
                .OfType<DataGridViewRow>()
                .Select(r => r.Cells[COL_Filter].Value?.ToString())
                .ToList();
            QuickFilters.ReplaceAll(list);

            _parent.UpdateFilterDropDown();
            this.Close();
        }

        private void CancelBtn_Click(object sender, EventArgs e) => this.Close();

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
