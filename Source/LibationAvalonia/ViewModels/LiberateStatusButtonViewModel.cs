using Avalonia.Media;
using ReactiveUI;

namespace LibationAvalonia.ViewModels;

public class LiberateStatusButtonViewModel : ViewModelBase
{
	public bool IsButtonEnabled { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public IImage? ButtonImage { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
}
