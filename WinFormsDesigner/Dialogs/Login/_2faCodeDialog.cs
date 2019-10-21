using System;
using System.Windows.Forms;

namespace WinFormsDesigner.Dialogs.Login
{
	public partial class _2faCodeDialog : Form
	{
		public string NewTags { get; private set; }

		public _2faCodeDialog()
		{
			InitializeComponent();
		}
	}
}
