using System;
using System.ComponentModel;
using System.Threading;

namespace LibationWinForms
{
	internal class CrossThreadSync<T>
	{
		public event EventHandler<T> ObjectReceived;
		private int InstanceThreadId { get; set; } = Thread.CurrentThread.ManagedThreadId;
		private SynchronizationContext SyncContext { get; set; } = SynchronizationContext.Current;
		private bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != InstanceThreadId;

		public void ResetContext()
		{
			SyncContext = SynchronizationContext.Current;
			InstanceThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		public void Send(T obj)
		{
			if (InvokeRequired)
				SyncContext.Send(SendOrPostCallback, new AsyncCompletedEventArgs(null, false, obj));
			else
				ObjectReceived?.Invoke(this, obj);
		}

		public void Post(T obj)
		{
			if (InvokeRequired)
				SyncContext.Post(SendOrPostCallback, new AsyncCompletedEventArgs(null, false, obj));
			else
				ObjectReceived?.Invoke(this, obj);
		}

		private void SendOrPostCallback(object asyncArgs)
		{
			var e = asyncArgs as AsyncCompletedEventArgs;

			var userObject = (T)e.UserState;

			ObjectReceived?.Invoke(this, userObject);
		}
	}
}
