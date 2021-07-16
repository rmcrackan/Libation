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
	public partial class TEMP_TestNewControls : Form
	{
		public TEMP_TestNewControls()
        {
            InitializeComponent();
        }

		private void TEMP_TestNewControls_Load(object sender, EventArgs e)
		{

			if (this.DesignMode)
				return;

			{
				var dirCtrl = this.directorySelectControl1;
				dirCtrl.SetDirectoryItems(new()
				{
					FileManager.Configuration.KnownDirectories.AppDir,
					FileManager.Configuration.KnownDirectories.MyDocs,
					FileManager.Configuration.KnownDirectories.LibationFiles,
					FileManager.Configuration.KnownDirectories.MyDocs,
					FileManager.Configuration.KnownDirectories.None,
					FileManager.Configuration.KnownDirectories.WinTemp,
					FileManager.Configuration.KnownDirectories.UserProfile
				}
				,
				FileManager.Configuration.KnownDirectories.MyDocs
				);
			}

			//{
			//	var dirOrCustCtrl = this.directoryOrCustomSelectControl1;
			//	dirOrCustCtrl.SetSearchTitle("Libation Files");
			//	dirOrCustCtrl.SetDirectoryItems(new()
			//	{
			//		FileManager.Configuration.KnownDirectories.AppDir,
			//		FileManager.Configuration.KnownDirectories.MyDocs,
			//		FileManager.Configuration.KnownDirectories.LibationFiles,
			//		FileManager.Configuration.KnownDirectories.MyDocs,
			//		FileManager.Configuration.KnownDirectories.None,
			//		FileManager.Configuration.KnownDirectories.WinTemp,
			//		FileManager.Configuration.KnownDirectories.UserProfile
			//	}
			//	,
			//	FileManager.Configuration.KnownDirectories.MyDocs
			//	);
			//}


		}

		private void button1_Click(object sender, EventArgs e)
		{
			var dirCtrl = this.directorySelectControl1;
			var x = dirCtrl.SelectedDirectory;
			dirCtrl.SelectDirectory(FileManager.Configuration.KnownDirectories.UserProfile);

			//var dirOrCustCtrl = this.directoryOrCustomSelectControl1;
			//var y = dirOrCustCtrl.SelectedDirectory;
			//dirOrCustCtrl.SelectDirectory(FileManager.Configuration.KnownDirectories.UserProfile);
		}
    }
}