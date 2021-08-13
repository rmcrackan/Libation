using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LibationWinForms
{
	public abstract class AsyncNotifyPropertyChanged : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private CrossThreadSync<PropertyChangedEventArgs> ThreadSync { get; } = new CrossThreadSync<PropertyChangedEventArgs>();

		public AsyncNotifyPropertyChanged()
			=>ThreadSync.ObjectReceived += (_, args) => PropertyChanged?.Invoke(this, args);		

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
			=> ThreadSync.Post(new PropertyChangedEventArgs(propertyName));
	}
}
