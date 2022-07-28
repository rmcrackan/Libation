using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.Threading
{
	internal static class AvaloniaThreadUtils
	{
		public static TResult Invoke<TResult>(this Dispatcher dispatcher, Func<TResult> function, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
		{
			using var source = new CancellationTokenSource();
			var task = dispatcher.InvokeAsync(function, dispatcherPriority);
			task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			dispatcher.MainLoop(source.Token);
			return task.Result;
		}

		public static void Invoke(this Dispatcher dispatcher, Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
		{
			using var source = new CancellationTokenSource();
			var task = dispatcher.InvokeAsync(action, dispatcherPriority);
			task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			Dispatcher.UIThread.MainLoop(source.Token);
		}

		public static T WaitOnUIAndGetResult<T>(this Task<T> task)
			=> WaitOnDispatcherAndGetResult(task, Dispatcher.UIThread);

		public static T WaitOnDispatcherAndGetResult<T>(this Task<T> task, Dispatcher dispatcher)
		{
			using var source = new CancellationTokenSource();
			task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			dispatcher.MainLoop(source.Token);
			return task.Result;
		}

		public static void WaitOnUI(this Task task)
			=> WaitOnDispatcher(task, Dispatcher.UIThread);
		public static void WaitOnDispatcher(this Task task, Dispatcher dispatcher)
		{
			using var source = new CancellationTokenSource();
			task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			dispatcher.MainLoop(source.Token);
		}
	}
}
