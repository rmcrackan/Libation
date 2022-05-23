using Dinah.Core.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{

    internal class MasterDataGridView : DataGridView
    {
		internal delegate void LibraryBookEntryClickedEventHandler(DataGridViewCellEventArgs e, LibraryBookEntry entry);
		public event LibraryBookEntryClickedEventHandler LibraryBookEntryClicked;
        public MasterDataGridView()
        {

        }


		public GridEntry getGridEntry(int rowIndex) => this.GetBoundItem<GridEntry>(rowIndex);

    }
}
