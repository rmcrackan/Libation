using System;
using System.Windows.Forms;

namespace LibationWinForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

			if (string.IsNullOrWhiteSpace(FileManager.Configuration.Instance.Books))
			{
				var welcomeText = @"
This appears to be your first time using Libation. Welcome.
Please fill fill in a few settings on the following page. You can also change these settings later.

After you make your selections, get started by importing your library.
Go to Import > Scan Library
".Trim();
				MessageBox.Show(welcomeText, "Welcom to Libation", MessageBoxButtons.OK);
				new SettingsDialog().ShowDialog();
			}

            Application.Run(new Form1());
        }
    }
}
