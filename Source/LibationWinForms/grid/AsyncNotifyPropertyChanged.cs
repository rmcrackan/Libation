using Dinah.Core.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibationWinForms
{
	public abstract class AsyncNotifyPropertyChanged : SynchronizeInvoker, INotifyPropertyChanged
	{
		// see also notes in Libation/Source/__ARCHITECTURE NOTES.txt :: MVVM
		public event PropertyChangedEventHandler PropertyChanged;

		// per standard INotifyPropertyChanged pattern:
		// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=> this.UIThreadAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
	}
}
