using ApplicationServices;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataLayer;
using LibationFileManager;
using System.Collections;

namespace LibationWinForms.Dialogs
{
	public partial class TrashBinDialog : Form
	{
		private readonly string deletedCheckedTemplate;
		public TrashBinDialog()
		{
			InitializeComponent();

			this.SetLibationIcon();
			this.RestoreSizeAndLocation(Configuration.Instance);
			this.Closing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);

			deletedCheckedTemplate = deletedCheckedLbl.Text;

			using var context = DbContexts.GetContext();
			var deletedBooks = context.GetDeletedLibraryBooks();
			foreach (var lb in deletedBooks)
				deletedCbl.Items.Add(lb);

			setLabel();
		}

		private void deletedCbl_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			// CheckedItems.Count is not updated until after the event fires
			setLabel(e.NewValue);
		}

		private async void permanentlyDeleteBtn_Click(object sender, EventArgs e)
		{
			setControlsEnabled(false);

			var removed = deletedCbl.CheckedItems.Cast<LibraryBook>().ToList();

			removeFromCheckList(removed);
			await Task.Run(removed.PermanentlyDeleteBooks);

			setControlsEnabled(true);
		}

		private async void restoreBtn_Click(object sender, EventArgs e)
		{
			setControlsEnabled(false);

			var removed = deletedCbl.CheckedItems.Cast<LibraryBook>().ToList();

			removeFromCheckList(removed);
			await Task.Run(removed.RestoreBooks);

			setControlsEnabled(true);
		}

		private void removeFromCheckList(IEnumerable objects)
		{
			foreach (var o in objects)
				deletedCbl.Items.Remove(o);

			deletedCbl.Refresh();
			setLabel();
		}

		private void setControlsEnabled(bool enabled)
			=> restoreBtn.Enabled = permanentlyDeleteBtn.Enabled = deletedCbl.Enabled = everythingCb.Enabled = enabled;

		private void everythingCb_CheckStateChanged(object sender, EventArgs e)
		{
			if (everythingCb.CheckState is CheckState.Indeterminate)
			{
				everythingCb.CheckState = CheckState.Unchecked;
				return;
			}

			deletedCbl.ItemCheck -= deletedCbl_ItemCheck;

			for (var i = 0; i < deletedCbl.Items.Count; i++)
				deletedCbl.SetItemChecked(i, everythingCb.CheckState is CheckState.Checked);

			setLabel();

			deletedCbl.ItemCheck += deletedCbl_ItemCheck;
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

			everythingCb.CheckStateChanged -= everythingCb_CheckStateChanged;

			everythingCb.CheckState
				= count > 0 && count == deletedCbl.Items.Count ? CheckState.Checked
				: count == 0 ? CheckState.Unchecked
				: CheckState.Indeterminate;

			everythingCb.CheckStateChanged += everythingCb_CheckStateChanged;

			deletedCheckedLbl.Text = string.Format(deletedCheckedTemplate, count, deletedCbl.Items.Count);
		}
	}
}
