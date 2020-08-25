using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class ScanAccountsDialog : Form
	{
		public ScanAccountsDialog()
		{
			InitializeComponent();
		}

		private void importBtn_Click(object sender, EventArgs e)
		{



			// this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e) => this.Close();
	}
}
