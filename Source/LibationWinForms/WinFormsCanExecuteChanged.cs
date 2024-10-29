using LibationUiBase.ViewModels;
using System;

namespace LibationWinForms
{
    public class WinFormsCanExecuteChanged : ICanExecuteChanged
    {
        public event EventHandler Event;

        public void Raise()
        {
            Event?.Invoke(null, EventArgs.Empty);
        }
    }
}
