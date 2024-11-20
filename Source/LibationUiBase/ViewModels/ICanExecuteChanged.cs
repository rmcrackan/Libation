using System;

namespace LibationUiBase.ViewModels;

public interface ICanExecuteChanged
{
    event EventHandler Event;
    void Raise();
}