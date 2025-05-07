using ReactiveUI;

namespace HangoverAvalonia.ViewModels;

public class CheckBoxViewModel : ViewModelBase
{
	private bool _isChecked;
	public bool IsChecked { get => _isChecked; set => this.RaiseAndSetIfChanged(ref _isChecked, value); }
	private object _bookText;
	public object Item { get => _bookText; set => this.RaiseAndSetIfChanged(ref _bookText, value); }
}
