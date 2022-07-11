using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class ProcessQueueControl2 : UserControl, ProcessQueue.ILogForm
    {
        private readonly ProcessQueueViewModel _viewModel;
        private ItemsRepeater _repeater;
        private ScrollViewer _scroller;
        private int _selectedIndex;
        private Random _random = new Random(0);


		private TrackedQueue2<ProcessBook2> Queue => _viewModel.Items;

		private readonly ProcessQueue.LogMe Logger;
        private int QueuedCount
        {
            set
            {
                queueNumberLbl_Text.Text = value.ToString();
				queueNumberLbl_Text.IsVisible = value > 0;
				queueNumberLbl_Icon.IsVisible = value > 0;
            }
        }
        private int ErrorCount
        {
            set
            {
                errorNumberLbl_Text.Text = value.ToString();
				errorNumberLbl_Text.IsVisible = value > 0;
				errorNumberLbl_Icon.IsVisible = value > 0;
            }
        }

        private int CompletedCount
        {
            set
            {
                completedNumberLbl_Text.Text = value.ToString();
				completedNumberLbl_Text.IsVisible = value > 0;
                completedNumberLbl_Icon.IsVisible = value > 0;
            }
        }

        public Task QueueRunner { get; private set; }
        public bool Running => !QueueRunner?.IsCompleted ?? false;

		public ProcessQueueControl2()
		{
			InitializeComponent();
            _repeater = this.Get<ItemsRepeater>("repeater");
            _scroller = this.Get<ScrollViewer>("scroller");
            _repeater.PointerPressed += RepeaterClick;
            _repeater.KeyDown += RepeaterOnKeyDown;
            DataContext = _viewModel = new ProcessQueueViewModel();

			ProcessBookControl2.ButtonClicked += ProcessBookControl2_ButtonClicked;

            queueNumberLbl_Icon = this.FindControl<Image>(nameof(queueNumberLbl_Icon));
			errorNumberLbl_Icon = this.FindControl<Image>(nameof(errorNumberLbl_Icon));
			completedNumberLbl_Icon = this.FindControl<Image>(nameof(completedNumberLbl_Icon));

            queueNumberLbl_Text = this.FindControl<TextBlock>(nameof(queueNumberLbl_Text));
            errorNumberLbl_Text = this.FindControl<TextBlock>(nameof(errorNumberLbl_Text));
            completedNumberLbl_Text = this.FindControl<TextBlock>(nameof(completedNumberLbl_Text));

            runningTimeLbl = this.FindControl<TextBlock>(nameof(runningTimeLbl));

			toolStripProgressBar1 = this.FindControl<ProgressBar>(nameof(toolStripProgressBar1));

			Logger = ProcessQueue.LogMe.RegisterForm(this);

			Queue.QueuededCountChanged += Queue_QueuededCountChanged;
			Queue.CompletedCountChanged += Queue_CompletedCountChanged;

			if (Design.IsDesignMode)
                return;

            runningTimeLbl.Text = string.Empty;
            QueuedCount = 0;
            ErrorCount = 0;
            CompletedCount = 0;

		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

        private async void ProcessBookControl2_ButtonClicked(ProcessBook2 item, QueueButton queueButton)
        {
            switch (queueButton)
            {
                case QueueButton.MoveFirst:
					Queue.MoveQueuePosition(item, QueuePosition.Fisrt);
					break;
                case QueueButton.MoveUp:
					Queue.MoveQueuePosition(item, QueuePosition.OneUp);
					break;
                case QueueButton.MoveDown:
					Queue.MoveQueuePosition(item, QueuePosition.OneDown);
					break;
                case QueueButton.MoveLast:
					Queue.MoveQueuePosition(item, QueuePosition.Last);
					break;
                case QueueButton.Cancel:
					if (item is not null)
						await item.CancelAsync();
					Queue.RemoveQueued(item);
					break;
            }
        }

        private void RepeaterClick(object sender, PointerPressedEventArgs e)
        {
            if ((e.Source as TextBlock)?.DataContext is ProcessBook2 item)
            {
                _viewModel.SelectedItem = item;
                _selectedIndex = _viewModel.Items.IndexOf(item);
            }
        }

        private void RepeaterOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                //_viewModel.ResetItems();
            }
        }
		public async void CancelAllBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Queue.ClearQueue();
			if (Queue.Current is not null)
				await Queue.Current.CancelAsync();
		}

		public void ClearFinishedBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Queue.ClearCompleted();

			if (!Running)
				runningTimeLbl.Text = string.Empty;
		}

		private bool isBookInQueue(DataLayer.LibraryBook libraryBook)
			=> Queue.Any(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId);

		public void AddDownloadPdf(DataLayer.LibraryBook libraryBook)
			=> AddDownloadPdf(new List<DataLayer.LibraryBook>() { libraryBook });

		public void AddDownloadDecrypt(DataLayer.LibraryBook libraryBook)
			=> AddDownloadDecrypt(new List<DataLayer.LibraryBook>() { libraryBook });

		public void AddConvertMp3(DataLayer.LibraryBook libraryBook)
			=> AddConvertMp3(new List<DataLayer.LibraryBook>() { libraryBook });

		public void AddDownloadPdf(IEnumerable<DataLayer.LibraryBook> entries)
		{
			List<ProcessBook2> procs = new();
			foreach (var entry in entries)
			{
				if (isBookInQueue(entry))
					continue;

				ProcessBook2 pbook = new(entry, Logger);
				pbook.AddDownloadPdf();
				procs.Add(pbook);
			}

			Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
			AddToQueue(procs);
		}

		public void AddDownloadDecrypt(IEnumerable<DataLayer.LibraryBook> entries)
		{
			List<ProcessBook2> procs = new();
			foreach (var entry in entries)
			{
				if (isBookInQueue(entry))
					continue;

				ProcessBook2 pbook = new(entry, Logger);
				pbook.AddDownloadDecryptBook();
				pbook.AddDownloadPdf();
				procs.Add(pbook);
			}

			Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
			AddToQueue(procs);
		}

		public void AddConvertMp3(IEnumerable<DataLayer.LibraryBook> entries)
		{
			List<ProcessBook2> procs = new();
			foreach (var entry in entries)
			{
				if (isBookInQueue(entry))
					continue;

				ProcessBook2 pbook = new(entry, Logger);
				pbook.AddConvertToMp3();
				procs.Add(pbook);
			}

			Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
			AddToQueue(procs);
		}
		private void AddToQueue(IEnumerable<ProcessBook2> pbook)
		{
			Dispatcher.UIThread.Post(() =>
			{
				Queue.Enqueue(pbook);
				if (!Running)
					QueueRunner = QueueLoop();
			});
		}

		DateTime StartingTime;
		private async Task QueueLoop()
		{
			try
			{
				Serilog.Log.Logger.Information("Begin processing queue");

				StartingTime = DateTime.Now;

				using var counterTimer = new System.Threading.Timer(CounterTimer_Tick, null, 0, 500);

				while (Queue.MoveNext())
				{
					var nextBook = Queue.Current;

					Serilog.Log.Logger.Information("Begin processing queued item. {item_LibraryBook}", nextBook?.LibraryBook);

					var result = await nextBook.ProcessOneAsync();

					Serilog.Log.Logger.Information("Completed processing queued item: {item_LibraryBook}\r\nResult: {result}", nextBook?.LibraryBook, result);

					if (result == ProcessBookResult.ValidationFail)
						Queue.ClearCurrent();
					else if (result == ProcessBookResult.FailedAbort)
						Queue.ClearQueue();
					else if (result == ProcessBookResult.FailedSkip)
						nextBook.LibraryBook.Book.UpdateBookStatus(DataLayer.LiberatedStatus.Error);
				}
				Serilog.Log.Logger.Information("Completed processing queue");

				Queue_CompletedCountChanged(this, 0);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error was encountered while processing queued items");
			}
		}

		public void WriteLine(string text)
		{
			
		}

		#region Control event handlers

		private void Queue_CompletedCountChanged(object sender, int e)
		{
			int errCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.FailedAbort or ProcessBookResult.FailedSkip or ProcessBookResult.FailedRetry or ProcessBookResult.ValidationFail);
			int completeCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.Success);

			ErrorCount = errCount;
			CompletedCount = completeCount;
			UpdateProgressBar();
		}
		private void Queue_QueuededCountChanged(object sender, int cueCount)
		{
			QueuedCount = cueCount;
			UpdateProgressBar();
		}
		private void UpdateProgressBar()
		{
			double percent = 100d * Queue.Completed.Count / Queue.Count;
			toolStripProgressBar1.Value = percent;
		}

		private async void cancelAllBtn_Click(object sender, EventArgs e)
		{
			Queue.ClearQueue();
			if (Queue.Current is not null)
				await Queue.Current.CancelAsync();
		}

		private void btnClearFinished_Click(object sender, EventArgs e)
		{
			Queue.ClearCompleted();

			if (!Running)
				runningTimeLbl.Text = string.Empty;
		}

		private void CounterTimer_Tick(object? state)
		{
			string timeToStr(TimeSpan time)
			{
				string minsSecs = $"{time:mm\\:ss}";
				if (time.TotalHours >= 1)
					return $"{time.TotalHours:F0}:{minsSecs}";
				return minsSecs;
			}

			if (Running)
				Dispatcher.UIThread.Post(() => runningTimeLbl.Text = timeToStr(DateTime.Now - StartingTime));
		}

		#endregion
	}
}
