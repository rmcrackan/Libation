using ReactiveUI;

namespace LibationAvalonia.ViewModels;

public class LiberateStatusButtonViewModel : ViewModelBase
{
	public bool IsError { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool IsButtonEnabled { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool IsSeries { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool Expanded { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool RedVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = true;
	public bool YellowVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool GreenVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool PdfDownloadedVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool PdfNotDownloadedVisible { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
}
