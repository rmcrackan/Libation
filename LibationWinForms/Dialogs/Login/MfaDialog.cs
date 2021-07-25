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

		AudibleApi.MfaConfig _mfaConfig { get; }

		public MfaDialog(AudibleApi.MfaConfig mfaConfig)
		{
			InitializeComponent();

			_mfaConfig = mfaConfig;

			radioButtons = new[] { this.radioButton1, this.radioButton2, this.radioButton3 };

			// optional string settings
			if (!string.IsNullOrWhiteSpace(mfaConfig.Title))
				this.Text = mfaConfig.Title;

			setRadioButton(0, this.radioButton1);
			setRadioButton(1, this.radioButton2);
			setRadioButton(2, this.radioButton3);

			Serilog.Log.Logger.Information("{@DebugInfo}", new {
				paramButtonCount = mfaConfig.Buttons.Count,
				visibleRadioButtonCount = radioButtons.Count(rb => rb.Visible)
			});
		}

		private void setRadioButton(int pos, RadioButton rb)
		{
			if (_mfaConfig.Buttons.Count <= pos)
			{
				rb.Checked = false;
				rb.Enabled = false;
				rb.Visible = false;
				return;
			}

			var btn = _mfaConfig.Buttons[pos];

			// optional
			if (!string.IsNullOrWhiteSpace(btn.Text))
				rb.Text = btn.Text;

			// mandatory values
			rb.Name = btn.Name;
			rb.Tag = btn.Value;
		}

		public string SelectedName { get; private set; }
		public string SelectedValue { get; private set; }
		private void submitBtn_Click(object sender, EventArgs e)
		{
			Serilog.Log.Logger.Information("RadioButton states: {@DebugInfo}", new {
				rb1_visible = radioButton1.Visible,
				rb1_checked = radioButton1.Checked,

				r21_visible = radioButton2.Visible,
				r21_checked = radioButton2.Checked,

				rb3_visible = radioButton3.Visible,
				rb3_checked = radioButton3.Checked
			});

			var selected = radioButtons.FirstOrDefault(rb => rb.Checked);
			if (selected is null)
			{
				MessageBox.Show("No MFA option selected", "None selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Serilog.Log.Logger.Information("Selected: {@DebugInfo}", new { isSelected = selected is not null, name = selected?.Name, value = selected?.Tag });

			SelectedName = selected.Name;
			SelectedValue = (string)selected.Tag;

			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}
