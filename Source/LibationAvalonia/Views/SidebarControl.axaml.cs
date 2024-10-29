using ApplicationServices;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationUiBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LibationAvalonia.Views
{
    public partial class SidebarControl : UserControl
    {
        private TrackedQueue<ProcessBookViewModel> Queue => _processQueue.Queue;
        private ProcessQueueViewModel _processQueue = ServiceLocator.Get<ProcessQueueViewModel>();
        
        public SidebarControl()
        {
            InitializeComponent();

            ProcessBookControl.PositionButtonClicked += ProcessBookControl2_ButtonClicked;
            ProcessBookControl.CancelButtonClicked += ProcessBookControl2_CancelButtonClicked;

            #region Design Mode Testing
            if (Design.IsDesignMode)
            {
                var vm = ServiceLocator.Get<ProcessQueueViewModel>();
                var Logger = LogMe.RegisterForm(vm);
                DataContext = vm;
                using var context = DbContexts.GetContext();
                List<ProcessBookViewModel> testList = new()
                {
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4IM1G"), Logger)
                    {
                        Result = ProcessBookResult.FailedAbort,
                        Status = ProcessBookStatus.Failed,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4IWVG"), Logger)
                    {
                        Result = ProcessBookResult.FailedSkip,
                        Status = ProcessBookStatus.Failed,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4JA2Q"), Logger)
                    {
                        Result = ProcessBookResult.FailedRetry,
                        Status = ProcessBookStatus.Failed,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4NUPO"), Logger)
                    {
                        Result = ProcessBookResult.ValidationFail,
                        Status = ProcessBookStatus.Failed,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4NMX4"), Logger)
                    {
                        Result = ProcessBookResult.Cancelled,
                        Status = ProcessBookStatus.Cancelled,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4NOZ0"), Logger)
                    {
                        Result = ProcessBookResult.Success,
                        Status = ProcessBookStatus.Completed,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017WJ5ZK6"), Logger)
                    {
                        Result = ProcessBookResult.None,
                        Status = ProcessBookStatus.Working,
                    },
                    new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4IM1G"), Logger)
                    {
                        Result = ProcessBookResult.None,
                        Status = ProcessBookStatus.Queued,
                    },
                };

                vm.Queue.Enqueue(testList);
                vm.Queue.MoveNext();
                vm.Queue.MoveNext();
                vm.Queue.MoveNext();
                vm.Queue.MoveNext();
                vm.Queue.MoveNext();
                vm.Queue.MoveNext();
                vm.Queue.MoveNext();
                return;
            }
            #endregion
        }

        public void NumericUpDown_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter && sender is Avalonia.Input.IInputElement input) input.Focus();
        }

        #region Control event handlers

        private async void ProcessBookControl2_CancelButtonClicked(ProcessBookViewModel item)
        {
            if (item is not null)
                await item.CancelAsync();
            Queue.RemoveQueued(item);
        }

        private void ProcessBookControl2_ButtonClicked(ProcessBookViewModel item, QueuePosition queueButton)
        {
            Queue.MoveQueuePosition(item, queueButton);
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

            if (!_processQueue.Running)
                _processQueue.RunningTime = string.Empty;
        }

        public void ClearLogBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _processQueue.LogEntries.Clear();
        }

        private async void LogCopyBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string logText = string.Join("\r\n", _processQueue.LogEntries.Select(r => $"{r.LogDate.ToShortDateString()} {r.LogDate.ToShortTimeString()}\t{r.LogMessage}"));
            await App.MainWindow.Clipboard.SetTextAsync(logText);
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

            if (!_processQueue.Running)
                _processQueue.RunningTime = string.Empty;
        }

        #endregion
    }

    public class DecimalConverter : IValueConverter
    {
        public static readonly DecimalConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sourceText && targetType.IsAssignableTo(typeof(decimal?)))
            {
                if (sourceText == "∞") return 0;

                for (int i = sourceText.Length; i > 0; i--)
                {
                    if (decimal.TryParse(sourceText[..i], out var val))
                        return val;
                }

                return 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal val)
            {
                return
                    val == 0 ? "∞"
                    : (
                        val >= 10 ? ((long)val).ToString()
                        : val >= 1 ? val.ToString("F1")
                        : val.ToString("F2")
                    ) + " MB/s";
            }
            return value?.ToString();
        }
    }
}
