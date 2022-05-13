using System;
using System.Windows.Forms;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
    public partial class Form1
    {
        private void Configure_Settings() { }

        private void accountsToolStripMenuItem_Click(object sender, EventArgs e) => new AccountsDialog().ShowDialog();

        private void basicSettingsToolStripMenuItem_Click(object sender, EventArgs e) => new SettingsDialog().ShowDialog();

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
            => MessageBox.Show($"Running Libation version {AppScaffolding.LibationScaffolding.BuildVersion}", $"Libation v{AppScaffolding.LibationScaffolding.BuildVersion}");
    }
}
