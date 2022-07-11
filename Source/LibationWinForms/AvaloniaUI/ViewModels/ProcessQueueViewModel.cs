using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class ProcessQueueViewModel : ViewModelBase
	{
		private TrackedQueue2<ProcessBook2> _items = new();
		public ProcessQueueViewModel() { }
		public TrackedQueue2<ProcessBook2> Items
		{
			get => _items;
			set => this.RaiseAndSetIfChanged(ref _items, value);
		}

		public ProcessBook2 SelectedItem { get; set; }
	}
}
