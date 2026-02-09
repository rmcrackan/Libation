using System;
using System.Diagnostics;
using System.Threading;

namespace DataLayer;

/// <summary> Notifies clients that the object is being disposed. </summary>
public interface INotifyDisposed : IDisposable
{
	/// <summary> Event raised when the object is disposed. </summary>
	event EventHandler? ObjectDisposed;
}

/// <summary> Creates a single instance of <typeparamref name="TDisposable"/> at a time, blocking subsequent creations until the previous creations are disposed. </summary>
public static class InstanceQueue<TDisposable> where TDisposable : INotifyDisposed
{
	/// <summary> Synchronization object for access to <see cref="LastInLine"/>"/> </summary>
	private static Lock Locker { get; } = new();
	/// <summary> Ticket holder for the last instance creator in line. </summary>
	private static Ticket? LastInLine { get; set; }

	/// <summary> Waits for all previously created instances of <typeparamref name="TDisposable"/> to be disposed, then creates and returns a new instance of <typeparamref name="TDisposable"/> using the provided <paramref name="creator"/> factory. </summary>
	public static TDisposable WaitToCreateInstance(Func<TDisposable> creator)
	{
		Ticket ticket;
		lock (Locker)
		{
			ticket = LastInLine = new Ticket(creator, LastInLine);
		}

		return ticket.Fulfill();
	}

	/// <summary> A queue ticket for an instance creator. </summary>
	/// <param name="creator">Factory to create a new instance of <typeparamref name="TDisposable"/></param>
	/// <param name="inFront">The ticket immediately in preceding this new ticket. This new ticket must wait for <paramref name="inFront"/> to signal its instance has been disposed before creating a new instance of <typeparamref name="TDisposable"/></param>
	private class Ticket(Func<TDisposable> creator, Ticket? inFront) : IDisposable
	{
		/// <summary> Factory to create a new instance of <typeparamref name="TDisposable"/> </summary>
		private Func<TDisposable> Creator { get; } = creator;
		/// <summary> Ticket immediately in front of this one. This instance must wait for <see cref="InFront"/> to signal its instance has been disposed before creating a new instance of <typeparamref name="TDisposable"/></summary>
		private Ticket? InFront { get; } = inFront;
		/// <summary> Wait handle to signal when this ticket's created instance is disposed </summary>
		private EventWaitHandle WaitHandle { get; } = new(false, EventResetMode.ManualReset);
		/// <summary> This ticket's created instance of <typeparamref name="TDisposable"/> </summary>
		private TDisposable? CreatedInstance { get; set; }

		/// <summary> Disposes of this ticket and every ticket queued in front of it. </summary>
		public void Dispose()
		{
			WaitHandle.Dispose();
			InFront?.Dispose();
		}

		/// <summary>
		/// Waits for the <see cref="InFront"/> ticket's instance to be disposed, then creates and returns a new instance of <typeparamref name="TDisposable"/> using the <see cref="Creator"/> factory.
		/// </summary>
		public TDisposable Fulfill()
		{
#if DEBUG
			var sw = Stopwatch.StartNew();
#endif
			//Wait for the previous ticket's instance to be disposed, then dispose of the previous ticket.
			InFront?.WaitHandle.WaitOne(Timeout.Infinite);
			InFront?.Dispose();
#if DEBUG
			sw.Stop();
			Debug.WriteLine($"Waited {sw.ElapsedMilliseconds}ms to create instance of {typeof(TDisposable).Name}");
#endif
			CreatedInstance = Creator();
			CreatedInstance.ObjectDisposed += CreatedInstance_ObjectDisposed;
			return CreatedInstance;
		}

		private void CreatedInstance_ObjectDisposed(object? sender, EventArgs e)
		{
			Debug.WriteLine($"{typeof(TDisposable).Name} Disposed");
			if (CreatedInstance is not null)
			{
				CreatedInstance.ObjectDisposed -= CreatedInstance_ObjectDisposed;
				CreatedInstance = default;
			}

			lock (Locker)
			{
				if (this == LastInLine)
				{
					//There are no ticket holders waiting after this one.
					//This ticket is fulfilled and will never be waited on.
					LastInLine = null;
					Dispose();
				}
				else
				{
					//Signal the that this ticket has been fulfilled so that
					//the next ticket in line may proceed.
					WaitHandle.Set();
				}
			}
		}
	}
}
