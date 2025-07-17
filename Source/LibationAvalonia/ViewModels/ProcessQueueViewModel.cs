using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using LibationUiBase.ProcessQueue;
using System;
using System.Collections.ObjectModel;

#nullable enable
namespace LibationAvalonia.ViewModels;

public record LogEntry(DateTime LogDate, string? LogMessage)
{
	public string LogDateString => LogDate.ToShortTimeString();
}

public class ProcessQueueViewModel : ProcessQueueViewModelBase
{
	public ProcessQueueViewModel() : base(CreateEmptyList())
	{
		Items = Queue.UnderlyingList as AvaloniaList<ProcessBookViewModelBase>
			?? throw new ArgumentNullException(nameof(Queue.UnderlyingList));

		SpeedLimit = Configuration.Instance.DownloadSpeedLimit / 1024m / 1024;
	}

	private decimal _speedLimit;
	public decimal SpeedLimitIncrement { get; private set; }
	public ObservableCollection<LogEntry> LogEntries { get; } = new();
	public AvaloniaList<ProcessBookViewModelBase> Items { get; }

	public decimal SpeedLimit
	{
		get
		{
			return _speedLimit;
		}
		set
		{
			var newValue = Math.Min(999 * 1024 * 1024, (long)(value * 1024 * 1024));
			var config = Configuration.Instance;
			config.DownloadSpeedLimit = newValue;

			_speedLimit
				= config.DownloadSpeedLimit <= newValue ? value
				: value == 0.01m ? config.DownloadSpeedLimit / 1024m / 1024
				: 0;

			config.DownloadSpeedLimit = (long)(_speedLimit * 1024 * 1024);

			SpeedLimitIncrement = _speedLimit > 100 ? 10
				: _speedLimit > 10 ? 1
				: _speedLimit > 1 ? 0.1m
				: 0.01m;

			RaisePropertyChanged(nameof(SpeedLimitIncrement));
			RaisePropertyChanged(nameof(SpeedLimit));
		}
	}

	public override void WriteLine(string text)
		=> Dispatcher.UIThread.Invoke(() => LogEntries.Add(new(DateTime.Now, text.Trim())));

	protected override ProcessBookViewModelBase CreateNewProcessBook(LibraryBook libraryBook)
		=> new ProcessBookViewModel(libraryBook, Logger);

	private static AvaloniaList<ProcessBookViewModelBase> CreateEmptyList()
	{
		if (Design.IsDesignMode)
			_ = Configuration.Instance.LibationFiles;
		return new AvaloniaList<ProcessBookViewModelBase>();
	}
}
