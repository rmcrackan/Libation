using HangoverAvalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangoverAvalonia.Views
{
	public partial class MainWindow
	{
		private void databaseTab_VisibleChanged(bool isVisible)
		{
			if (!isVisible)
				return;
		}

		public void Execute_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.ExecuteQuery();
		}
	}
}
