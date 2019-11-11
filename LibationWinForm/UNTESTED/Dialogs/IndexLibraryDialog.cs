using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;

namespace LibationWinForm
{
	public partial class IndexLibraryDialog : Form, IIndexLibraryDialog
	{
		public IndexLibraryDialog()
		{
			InitializeComponent();

			var btn = new Button();
			AcceptButton = btn;

			btn.Location = new System.Drawing.Point(this.Size.Width + 10, 0);
			// required for FindForm() to work
			this.Controls.Add(btn);

			this.Shown += (_, __) => AcceptButton.PerformClick();
		}

		List<string> successMessages { get; } = new List<string>();
		public string SuccessMessage => string.Join("\r\n", successMessages);

		public int NewBooksAdded { get; private set; }
		public int TotalBooksProcessed { get; private set; }

		public async Task DoMainWorkAsync()
		{
			var callback = new Login.WinformResponder();
			(TotalBooksProcessed, NewBooksAdded) = await LibraryCommands.IndexLibraryAsync(callback);

			successMessages.Add($"Total processed: {TotalBooksProcessed}");
			successMessages.Add($"New: {NewBooksAdded}");
		}
	}
}
