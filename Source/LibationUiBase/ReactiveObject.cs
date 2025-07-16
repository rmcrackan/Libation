using Dinah.Core.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable enable
namespace LibationUiBase;

public class ReactiveObject : SynchronizeInvoker, INotifyPropertyChanged, INotifyPropertyChanging
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public event PropertyChangingEventHandler? PropertyChanging;

	public void RaisePropertyChanging(PropertyChangingEventArgs args) => Invoke(() => PropertyChanging?.Invoke(this, args));
	public void RaisePropertyChanging(string propertyName) => RaisePropertyChanging(new PropertyChangingEventArgs(propertyName));
	public void RaisePropertyChanged(PropertyChangedEventArgs args) => Invoke(() => PropertyChanged?.Invoke(this, args));
	public void RaisePropertyChanged(string propertyName) => RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));

	public TRet RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string? propertyName = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

		if (!EqualityComparer<TRet>.Default.Equals(backingField, newValue))
		{
			RaisePropertyChanging(propertyName);
			backingField = newValue;
			RaisePropertyChanged(propertyName!);
		}

		return newValue;
	}
}
