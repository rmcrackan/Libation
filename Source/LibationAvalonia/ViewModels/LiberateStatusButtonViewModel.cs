using ReactiveUI;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	public class LiberateStatusButtonViewModel : ViewModelBase
	{
		private bool isSeries;
		private bool isError;
		private bool isButtonEnabled;
		private bool expanded;
		private bool redVisible = true;
		private bool yellowVisible;
		private bool greenVisible;
		private bool pdfNotDownloadedVisible;
		private bool pdfDownloadedVisible;

		public bool IsError { get => isError; set => this.RaiseAndSetIfChanged(ref isError, value); }
		public bool IsButtonEnabled { get => isButtonEnabled; set => this.RaiseAndSetIfChanged(ref isButtonEnabled, value); }
		public bool IsSeries { get => isSeries; set => this.RaiseAndSetIfChanged(ref isSeries, value); }
		public bool Expanded { get => expanded; set => this.RaiseAndSetIfChanged(ref expanded, value); }
		public bool RedVisible { get => redVisible; set => this.RaiseAndSetIfChanged(ref redVisible, value); }
		public bool YellowVisible { get => yellowVisible; set => this.RaiseAndSetIfChanged(ref yellowVisible, value); }
		public bool GreenVisible { get => greenVisible; set => this.RaiseAndSetIfChanged(ref greenVisible, value); }
		public bool PdfDownloadedVisible { get => pdfDownloadedVisible; set => this.RaiseAndSetIfChanged(ref pdfDownloadedVisible, value); }
		public bool PdfNotDownloadedVisible { get => pdfNotDownloadedVisible; set => this.RaiseAndSetIfChanged(ref pdfNotDownloadedVisible, value); }
	}
}
