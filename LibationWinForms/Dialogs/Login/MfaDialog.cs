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
		private RadioButton[] radioButtons { get; }

		public MfaDialog(AudibleApi.MfaConfig mfaConfig)
		{
			InitializeComponent();

			radioButtons = new[] { this.radioButton1, this.radioButton2, this.radioButton3 };

			// optional string settings
			if (!string.IsNullOrWhiteSpace(mfaConfig.Title))
				this.Text = mfaConfig.Title;
			setOptional(this.radioButton1, mfaConfig.Button1Text);
			setOptional(this.radioButton2, mfaConfig.Button2Text);
			setOptional(this.radioButton3, mfaConfig.Button3Text);

			// mandatory values
			radioButton1.Name = mfaConfig.Button1Name;
			radioButton1.Tag = mfaConfig.Button1Value;

			radioButton2.Name = mfaConfig.Button2Name;
			radioButton2.Tag = mfaConfig.Button2Value;

			radioButton3.Name = mfaConfig.Button3Name;
			radioButton3.Tag = mfaConfig.Button3Value;
		}

		private static void setOptional(RadioButton radioButton, string text)
		{
			if (!string.IsNullOrWhiteSpace(text))
				radioButton.Text = text;
		}

		public string SelectedName { get; private set; }
		public string SelectedValue { get; private set; }
		private void submitBtn_Click(object sender, EventArgs e)
		{
			Serilog.Log.Logger.Debug("RadioButton states: {@DebugInfo}", new {
				rb1_checked = radioButton1.Checked,
				r21_checked = radioButton2.Checked,
				rb3_checked = radioButton3.Checked
			});

			var selected = radioButtons.Single(rb => rb.Checked);

			Serilog.Log.Logger.Debug("Selected: {@DebugInfo}", new { isSelected = selected is not null, name = selected?.Name, value = selected?.Tag });

			SelectedName = selected.Name;
			SelectedValue = (string)selected.Tag;

			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}
