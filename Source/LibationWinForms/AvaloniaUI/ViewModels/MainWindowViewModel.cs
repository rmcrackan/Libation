using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private string _removeBooksButtonText = "Remove # Books from Libation";
		private bool _removeButtonsVisible = true;
		public string RemoveBooksButtonText { get => _removeBooksButtonText; set => this.RaiseAndSetIfChanged(ref _removeBooksButtonText, value); }
		public bool RemoveButtonsVisible { get => _removeButtonsVisible; set => this.RaiseAndSetIfChanged(ref _removeButtonsVisible, value); }
	}
}
