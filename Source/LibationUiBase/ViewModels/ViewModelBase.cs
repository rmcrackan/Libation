using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibationUiBase.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
{
    public ViewModelBase()
    {
        this.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        this.PropertyChanging += (s, e) => OnPropertyChanging(e.PropertyName);
    }

    protected virtual void OnPropertyChanging(string propertyName) { }
    protected virtual void OnPropertyChanged(string propertyName) { }

    public TRet RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string propertyName = null)
    {
        //if (EqualityComparer<TRet>.Default.Equals(backingField, newValue)) return newValue;

        RaisePropertyChanging(propertyName);
        backingField = newValue;
        RaisePropertyChanged(propertyName);
        return newValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
    public event PropertyChangingEventHandler PropertyChanging;
    public void RaisePropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
}

