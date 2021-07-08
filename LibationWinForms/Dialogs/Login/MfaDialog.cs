using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public partial class MfaDialog : Form
	{
		public MfaDialog(AudibleApi.MfaConfig mfaConfig)
		{
			InitializeComponent();

			// optional string settings
			if (!string.IsNullOrWhiteSpace(mfaConfig.Title))
				this.Text = mfaConfig.Title;
			if (!string.IsNullOrWhiteSpace(mfaConfig.Button1Text))
				this.radioButton1.Text = mfaConfig.Button1Text;
			if (!string.IsNullOrWhiteSpace(mfaConfig.Button2Text))
				this.radioButton2.Text = mfaConfig.Button2Text;
			if (!string.IsNullOrWhiteSpace(mfaConfig.Button3Text))
				this.radioButton3.Text = mfaConfig.Button3Text;

			// mandatory values
			radioButton1.Name = mfaConfig.Button1Name;
			radioButton1.Tag = mfaConfig.Button1Value;

			radioButton2.Name = mfaConfig.Button2Name;
			radioButton2.Tag = mfaConfig.Button2Value;

			radioButton3.Name = mfaConfig.Button3Name;
			radioButton3.Tag = mfaConfig.Button3Value;
		}

		public string SelectedName { get; private set; }
		public string SelectedValue { get; private set; }
		private void submitBtn_Click(object sender, EventArgs e)
		{
			var radioButtons = new[] { this.radioButton1, this.radioButton2, this.radioButton3 };
			var selected = radioButtons.Single(rb => rb.Checked);

			SelectedName = selected.Name;
			SelectedValue = (string)selected.Tag;

			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}
