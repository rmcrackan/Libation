using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class ProcessQueueViewModel : ViewModelBase, ProcessQueue.ILogForm
	{
		public ObservableCollection<LogEntry> LogEntries { get; } = new();
		private TrackedQueue2<ProcessBook2> _items = new();
		public TrackedQueue2<ProcessBook2> Items
		{
			get => _items;
			set => this.RaiseAndSetIfChanged(ref _items, value);
		}

		public ProcessBook2 SelectedItem { get; set; }

		public void WriteLine(string text)
		{
			Dispatcher.UIThread.Post(() =>
				LogEntries.Add(new()
				{
					LogDate = DateTime.Now,
					LogMessage = text.Trim()
				}));
		}
	}

	public class LogEntry
	{
		public DateTime LogDate { get; init; }
		public string LogDateString => LogDate.ToShortTimeString();
		public string LogMessage { get; init; }
	}
}
