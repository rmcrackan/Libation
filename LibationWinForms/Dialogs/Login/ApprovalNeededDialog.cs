﻿using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public partial class ApprovalNeededDialog : Form
	{
		public ApprovalNeededDialog()
		{
			InitializeComponent();
		}

		private void approvedBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
