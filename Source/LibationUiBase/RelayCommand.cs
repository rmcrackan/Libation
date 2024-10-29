using System;
using System.Windows.Input;
using LibationUiBase.ViewModels;

namespace LibationUiBase;

public class RelayCommand : ICommand
{
    private Action<object> execute;
    private Func<object, bool> canExecute;
    private ICanExecuteChanged canExecuteChanged;

    public event EventHandler CanExecuteChanged
    {
        add { canExecuteChanged.Event += value; }
        remove { canExecuteChanged.Event -= value; }
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
        canExecuteChanged = ServiceLocator.Get<ICanExecuteChanged>();
    }

    public bool CanExecute(object parameter)
    {
        return canExecute == null || canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        canExecuteChanged.Raise();
    }
}