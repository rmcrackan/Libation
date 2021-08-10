using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LibationWinForms
{
	public abstract class AsyncNotifyPropertyChanged : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private int InstanceThreadId { get; } = Thread.CurrentThread.ManagedThreadId;
		private bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != InstanceThreadId;
		private SynchronizationContext SyncContext { get; } = SynchronizationContext.Current;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var propertyChangedArgs = new PropertyChangedEventArgs(propertyName);

			if (InvokeRequired)
			{
				SyncContext.Post(
					PostPropertyChangedCallback,
					new AsyncCompletedEventArgs(null, false, propertyChangedArgs));
			}
			else
			{
				OnPropertyChanged(propertyChangedArgs);
			}
		}
		private void PostPropertyChangedCallback(object asyncArgs)
		{
			var e = asyncArgs as AsyncCompletedEventArgs;

			OnPropertyChanged(e.UserState as PropertyChangedEventArgs);
		}
		private void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
	}
}
