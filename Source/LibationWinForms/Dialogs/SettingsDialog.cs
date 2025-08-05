using System;
using System.Linq;
using System.Windows.Forms;
using LibationFileManager;
using LibationFileManager.Templates;

namespace LibationWinForms.Dialogs
{
	public partial class SettingsDialog : Form
	{
		private Configuration config { get; } = Configuration.Instance;
		private Func<string, string> desc { get; } = Configuration.GetDescription;
		private readonly ToolTip toolTip = new ToolTip
		{
			InitialDelay = 300,
			AutoPopDelay = 10000,
			ReshowDelay = 0
		};

		public SettingsDialog()
		{
			InitializeComponent();
			this.SetLibationIcon();
		}

		private void SettingsDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			Load_Important(config);
			Load_ImportLibrary(config);
			Load_DownloadDecrypt(config);
			Load_AudioSettings(config);
		}

		private static void editTemplate(ITemplateEditor template, TextBox textBox)
		{
			var form = new EditTemplateDialog(template);
			if (form.ShowDialog() == DialogResult.OK)
				textBox.Text = template.EditingTemplate.TemplateText;
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			if (!Save_Important(config)) return;
			Save_ImportLibrary(config);
			Save_DownloadDecrypt(config);
			Save_AudioSettings(config);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
