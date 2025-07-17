using DataLayer;
using LibationUiBase.ProcessQueue;
using System;
using System.Collections.Generic;

#nullable enable
namespace LibationWinForms.ProcessQueue;

internal class ProcessQueueViewModel : ProcessQueueViewModelBase
{
	public event EventHandler<string>? LogWritten;
	public List<ProcessBookViewModelBase> Items { get; }

	public ProcessQueueViewModel() : base(new List<ProcessBookViewModelBase>())
	{
		Items = Queue.UnderlyingList as List<ProcessBookViewModelBase>
			?? throw new ArgumentNullException(nameof(Queue.UnderlyingList));
	}

	public override void WriteLine(string text) => Invoke(() => LogWritten?.Invoke(this, text.Trim()));

	protected override ProcessBookViewModelBase CreateNewProcessBook(LibraryBook libraryBook)
		=> new ProcessBookViewModel(libraryBook, Logger);
}
