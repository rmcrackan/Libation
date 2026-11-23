using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Linq;

namespace LibationAvalonia.ViewModels;

public enum RightPanelTab { Details, Queue }

partial class MainVM
{
	public RightPanelTab SelectedRightTab
	{
		get => field;
		set
		{
			if (field == value) return;
			this.RaiseAndSetIfChanged(ref field, value);
			this.RaisePropertyChanged(nameof(QueueOpen));
			this.RaisePropertyChanged(nameof(DetailsOpen));
			this.RaisePropertyChanged(nameof(QueueTabActive));
			this.RaisePropertyChanged(nameof(DetailsTabActive));
		}
	} = RightPanelTab.Details;

	public bool QueueTabActive => SelectedRightTab == RightPanelTab.Queue;
	public bool DetailsTabActive => SelectedRightTab == RightPanelTab.Details;
	public bool DetailsOpen => InspectorOpen && DetailsTabActive;

	/// <summary> Right panel is open on the Queue tab. Setting false only leaves the tab. </summary>
	public bool QueueOpen
	{
		get => InspectorOpen && QueueTabActive;
		set
		{
			if (value)
			{
				SelectedRightTab = RightPanelTab.Queue;
				InspectorOpen = true;
			}
			else if (QueueTabActive)
				SelectedRightTab = RightPanelTab.Details;
		}
	}

	public void ShowDetailsTab() => SelectedRightTab = RightPanelTab.Details;
	public void ShowQueueTab() => SelectedRightTab = RightPanelTab.Queue;

	/// <summary> Books waiting or downloading </summary>
	public int QueueBadgeCount => ProcessQueue.Queue.Count - ProcessQueue.Queue.Completed.Count;
	public bool QueueBadgeVisible => QueueBadgeCount > 0;
	public string QueueBadgeText => QueueBadgeCount > 99 ? "99+" : QueueBadgeCount.ToString();

	private void Configure_ProcessQueue()
	{
		ProcessQueue.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName is nameof(ProcessQueue.HasUnfinished) or nameof(ProcessQueue.QueuedCount))
			{
				this.RaisePropertyChanged(nameof(QueueBadgeCount));
				this.RaisePropertyChanged(nameof(QueueBadgeVisible));
				this.RaisePropertyChanged(nameof(QueueBadgeText));
			}
		};
	}

	public async void LiberateClicked(System.Collections.Generic.IList<LibraryBook> libraryBooks, Configuration config)
	{
		try
		{
			if (!await ProcessQueue.QueueDownloadDecryptAsync(libraryBooks, config)
				&& libraryBooks.Count == 1 && libraryBooks[0].Book.AudioExists)
			{
				// liberated: open explorer to file
				var filePath = AudibleFileStorage.Audio.GetPath(libraryBooks[0].Book.AudibleProductId);
				if (!Go.To.File(filePath?.ShortPathName))
				{
					var suffix = string.IsNullOrWhiteSpace(filePath) ? "" : $":\r\n{filePath}";
					await MessageBox.Show($"File not found" + suffix);
				}
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBooks);
		}
	}

	public async void LiberateSeriesClicked(SeriesEntry series)
	{
		try
		{
			Serilog.Log.Logger.Information("Begin backing up all {series} episodes", series.LibraryBook);

			// Every episode, not just the ones that can be queued: the menu item is enabled from the grid's
			// display status, so it can be clicked for episodes the queue will reject, and only the queue
			// knows why it rejected them.
			await ProcessQueue.QueueDownloadDecryptAsync(series.Children.Select(c => c.LibraryBook).ToArray());
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error occurred while backing up {series} episodes", series.LibraryBook);
		}
	}

	public async void ConvertToMp3Clicked(LibraryBook[] libraryBooks)
	{
		try
		{
			await ProcessQueue.QueueConvertToMp3Async(libraryBooks);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBooks);
		}
	}

	public void ToggleQueueHideBtn()
	{
		if (QueueOpen)
			InspectorOpen = false;
		else
			QueueOpen = true;
	}
}
