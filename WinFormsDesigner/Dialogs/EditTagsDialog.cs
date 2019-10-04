using System;
using System.Windows.Forms;

namespace WinFormsDesigner
{
	public partial class EditTagsDialog : Form
	{
		public string NewTags { get; private set; }

		public EditTagsDialog()
		{
			InitializeComponent();
		}
	}
}
