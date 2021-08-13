using System;
using System.ComponentModel;
using System.Threading;

namespace LibationWinForms
{
	public class SynchronizeInvoker : ISynchronizeInvoke
	{
		public bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != InstanceThreadId;
		private int InstanceThreadId { get; set; } = Thread.CurrentThread.ManagedThreadId;
		private SynchronizationContext SyncContext { get; init; } = SynchronizationContext.Current;

		public SynchronizeInvoker()
		{
			SyncContext = SynchronizationContext.Current;
			if (SyncContext is null)
				throw new NullReferenceException($"Could not capture a current {nameof(SynchronizationContext)}");
		}

		public IAsyncResult BeginInvoke(Action action) => BeginInvoke(action, null);
		public IAsyncResult BeginInvoke(Delegate method) => BeginInvoke(method, null);
		public IAsyncResult BeginInvoke(Delegate method, object[] args)
		{
			var tme = new ThreadMethodEntry(method, args);

			if (InvokeRequired)
			{
				SyncContext.Post(ThreadMethodEntry.OnSendOrPostCallback, tme);
			}
			else
			{
				tme.Complete();
				tme.CompletedSynchronously = true;
			}
			return tme;
		}

		public object EndInvoke(IAsyncResult result)
		{
			if (result is not ThreadMethodEntry crossThread)
				throw new ArgumentException($"{nameof(result)} was not returned by {nameof(BeginInvoke)}");

			if (!crossThread.IsCompleted) 
				crossThread.AsyncWaitHandle.WaitOne();

			return crossThread.ReturnValue;
		}

		public object Invoke(Action action) => Invoke(action, null);
		public object Invoke(Delegate method) => Invoke(method, null);
		public object Invoke(Delegate method, object[] args)
		{
			var tme = new ThreadMethodEntry(method, args);

			if (InvokeRequired)
			{
				SyncContext.Send(ThreadMethodEntry.OnSendOrPostCallback, tme);
			}
			else
			{
				tme.Complete();
				tme.CompletedSynchronously = true;
			}

			return tme.ReturnValue;
		}

		private class ThreadMethodEntry : IAsyncResult
		{
			public object AsyncState => null;
			public bool CompletedSynchronously { get; internal set; }
			public bool IsCompleted { get; private set; }
			public object ReturnValue { get; private set; }
			public WaitHandle AsyncWaitHandle
			{
				get
				{
					if (resetEvent == null)
					{
						lock (invokeSyncObject)
						{
							if (resetEvent == null)
							{
								resetEvent = new ManualResetEvent(initialState: false);
							}
						}
					}
					return resetEvent;
				}
			}

			private object invokeSyncObject = new object();
			private Delegate method;
			private object[] args;
			private ManualResetEvent resetEvent;

			public ThreadMethodEntry(Delegate method, object[] args)
			{
				this.method = method;
				this.args = args;
				resetEvent = new ManualResetEvent(initialState: false);				
			}
			/// <summary>
			/// This callback executes on the SynchronizationContext thread.
			/// </summary>
			public static void OnSendOrPostCallback(object asyncArgs)
			{
				var e = asyncArgs as ThreadMethodEntry;

				e.Complete();
			}

			public void Complete()
			{
				try
				{
					switch (method)
					{
						case Action actiton:
							actiton();
							break;
						default:
							ReturnValue = method.DynamicInvoke(args);
							break;
					}
				}
				finally
				{
					IsCompleted = true;
					resetEvent.Set();
				}
			}

			~ThreadMethodEntry()
			{
				if (resetEvent != null)
				{
					resetEvent.Close();
				}
			}
		}
	}
}