using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class ProcessQueueViewModel : ViewModelBase
	{

		public string QueueHeader => "this is a header!";
		private TrackedQueue2<ProcessBook2> _items = new();
		public ProcessQueueViewModel() { }
		public TrackedQueue2<ProcessBook2> Items
		{
			get => _items;
			set => this.RaiseAndSetIfChanged(ref _items, value);
		}


		public ObservableCollection<LogEntry> LogEntries { get; } = new();

		public ProcessBook2 SelectedItem { get; set; }
	}

	public class LogEntry
	{
		public DateTime LogDate { get; init; }
		public string LogDateString => LogDate.ToShortTimeString();
		public string LogMessage { get; init; }
	}

}
