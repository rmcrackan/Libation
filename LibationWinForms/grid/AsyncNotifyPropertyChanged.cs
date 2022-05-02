using Dinah.Core.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibationWinForms
{
	public abstract class AsyncNotifyPropertyChanged : SynchronizeInvoker, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=> this.UIThreadAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
	}
}
