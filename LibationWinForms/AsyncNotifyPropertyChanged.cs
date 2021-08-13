using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LibationWinForms
{
	public abstract class AsyncNotifyPropertyChanged : SynchronizeInvoker, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public AsyncNotifyPropertyChanged() { }

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=>BeginInvoke(PropertyChanged, new object[] { this, new PropertyChangedEventArgs(propertyName) });
	}
}
