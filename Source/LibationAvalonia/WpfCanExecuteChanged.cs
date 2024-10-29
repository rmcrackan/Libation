using Avalonia.Labs.Input;
using Avalonia.Threading;
using LibationUiBase.ViewModels;
using System;

namespace LibationAvalonia
{
    public class WpfCanExecuteChanged : ICanExecuteChanged
    {
        public event EventHandler Event
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        public void Raise()
        {
            Dispatcher.UIThread.Post(CommandManager.InvalidateRequerySuggested);
        }
    }
}
