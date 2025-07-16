using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using LibationUiBase.ProcessQueue;
using System;

#nullable enable
namespace LibationAvalonia.ViewModels;

public class ProcessQueueViewModel : ProcessQueueViewModelBase
{
	public override void WriteLine(string text)
	{
		Dispatcher.UIThread.Invoke(() =>
			LogEntries.Add(new()
			{
				LogDate = DateTime.Now,
				LogMessage = text.Trim()
			}));
	}

	public ProcessQueueViewModel() : base(CreateEmptyList())
	{
		Items = Queue.UnderlyingList as AvaloniaList<ProcessBookViewModelBase>
			?? throw new ArgumentNullException(nameof(Queue.UnderlyingList));
	}

	public AvaloniaList<ProcessBookViewModelBase> Items { get; }
	protected override ProcessBookViewModelBase CreateNewBook(LibraryBook libraryBook)
		=> new ProcessBookViewModel(libraryBook, Logger);

	private static AvaloniaList<ProcessBookViewModelBase> CreateEmptyList()
	{
		if (Design.IsDesignMode)
			_ = Configuration.Instance.LibationFiles;
		return new AvaloniaList<ProcessBookViewModelBase>();
	}
}
