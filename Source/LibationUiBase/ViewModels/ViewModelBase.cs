using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibationUiBase.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
{
    public ViewModelBase()
    {
        this.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        this.PropertyChanging += (s, e) => OnPropertyChanged(e.PropertyName);
    }

    protected virtual void OnPropertyChanging(string propertyName) { }
    protected virtual void OnPropertyChanged(string propertyName) { }

    public TRet RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue)) return newValue;

        RaisePropertyChanging(propertyName);
        backingField = newValue;
        RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        return newValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
    public void RaisePropertyChanged(string propertyName) => RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
    
    public event PropertyChangingEventHandler PropertyChanging;
    public void RaisePropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);
    public void RaisePropertyChanging(string propertyName) => RaisePropertyChanging(new PropertyChangingEventArgs(propertyName));
}

