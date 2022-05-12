using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{
	public partial class ProcessBookForm : Form
	{
		private Control _dockControl;
		public int WidthChange { get; set; }
		public ProcessBookForm()
		{
			InitializeComponent();
		}

		public void PassControl(Control dockControl)
		{
			_dockControl = dockControl;
			Controls.Add(_dockControl);
		}

		public Control RegainControl()
		{
			Controls.Remove(_dockControl);
			return _dockControl;
		}
	}
}
