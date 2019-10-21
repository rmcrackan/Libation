using System;
using System.Windows.Forms;

namespace WinFormsDesigner
{
	public partial class SettingsDialog : Form
	{
		public SettingsDialog()
		{
			InitializeComponent();
			audibleLocaleCb.SelectedIndex = 0;
		}
	}
}
