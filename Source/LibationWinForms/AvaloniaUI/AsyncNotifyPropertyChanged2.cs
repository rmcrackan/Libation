using Dinah.Core.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibationWinForms.AvaloniaUI
{
	public abstract class AsyncNotifyPropertyChanged2 : INotifyPropertyChanged
	{
		// see also notes in Libation/Source/_ARCHITECTURE NOTES.txt :: MVVM
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=> Avalonia.Threading.Dispatcher.UIThread.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
	}
}
