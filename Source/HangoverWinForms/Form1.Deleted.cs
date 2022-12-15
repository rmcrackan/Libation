using ApplicationServices;
using DataLayer;

namespace HangoverWinForms
{
    public partial class Form1
    {
        private string deletedCheckedTemplate;

        private void Load_deletedTab()
        {
            deletedCheckedTemplate = deletedCheckedLbl.Text;
        }

        private void deletedTab_VisibleChanged(object sender, EventArgs e)
        {
            if (!deletedTab.Visible)
                return;

            if (deletedCbl.Items.Count == 0)
                reload();
        }

        private void deletedCbl_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // CheckedItems.Count is not updated until after the event fires
            setLabel(e.NewValue);
        }

        private void checkAllBtn_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < deletedCbl.Items.Count; i++)
                deletedCbl.SetItemChecked(i, true);
        }

        private void uncheckAllBtn_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < deletedCbl.Items.Count; i++)
                deletedCbl.SetItemChecked(i, false);
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            var libraryBooksToRestore = deletedCbl.CheckedItems.Cast<LibraryBook>().ToList();
            var qtyChanges = libraryBooksToRestore.RestoreBooks();
            if (qtyChanges > 0)
                reload();
        }

        private void reload()
        {
            deletedCbl.Items.Clear();
            var deletedBooks = DbContexts.GetContext().GetDeletedLibraryBooks();
            foreach (var lb in deletedBooks)
                deletedCbl.Items.Add(lb);

            setLabel();
        }

        private void setLabel(CheckState? checkedState = null)
        {
            var pre = deletedCbl.CheckedItems.Count;
            int count = checkedState switch
            {
                CheckState.Checked => pre + 1,
                CheckState.Unchecked => pre - 1,
                _ => pre,
            };

            deletedCheckedLbl.Text = string.Format(deletedCheckedTemplate, count, deletedCbl.Items.Count);
        }
    }
}
