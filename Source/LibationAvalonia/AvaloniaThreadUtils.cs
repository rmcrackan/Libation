using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.Threading
{
	internal static class AvaloniaThreadUtils
	{
		public static TResult Invoke<TResult>(this Dispatcher dispatcher, Func<TResult> function, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
			=> WaitOnDispatcherAndGetResult(dispatcher.InvokeAsync(function, dispatcherPriority), dispatcher);

		public static void Invoke(this Dispatcher dispatcher, Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
			=> WaitOnDispatcher(dispatcher.InvokeAsync(action, dispatcherPriority), dispatcher);

		public static TResult WaitOnUIAndGetResult<TResult>(this Task<TResult> task)
			=> WaitOnDispatcherAndGetResult(task, Dispatcher.UIThread);

		public static void WaitOnUI(this Task task)
			=> WaitOnDispatcher(task, Dispatcher.UIThread);

		public static TResult WaitOnDispatcherAndGetResult<TResult>(this Task<TResult> task, Dispatcher dispatcher)
		{
			using var source = new CancellationTokenSource();
			task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			dispatcher.MainLoop(source.Token);
			return task.Result;
		}

		public static void WaitOnDispatcher(this Task task, Dispatcher dispatcher)
		{
			using var source = new CancellationTokenSource();
			task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			dispatcher.MainLoop(source.Token);
		}
	}
}
