using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationUiBase.GridView;
using System;

namespace LibationAvalonia.Views
{
	public partial class LiberateStatusButton : UserControl
	{
		public event EventHandler Click;

		public static readonly StyledProperty<LiberatedStatus> BookStatusProperty =
		AvaloniaProperty.Register<LiberateStatusButton, LiberatedStatus>(nameof(BookStatus));

		public static readonly StyledProperty<LiberatedStatus?> PdfStatusProperty =
		AvaloniaProperty.Register<LiberateStatusButton, LiberatedStatus?>(nameof(PdfStatus));

		public static readonly StyledProperty<bool> IsUnavailableProperty =
		AvaloniaProperty.Register<LiberateStatusButton, bool>(nameof(IsUnavailable));

		public static readonly StyledProperty<bool> ExpandedProperty =
		AvaloniaProperty.Register<LiberateStatusButton, bool>(nameof(Expanded));

		public static readonly StyledProperty<bool> IsSeriesProperty =
		AvaloniaProperty.Register<LiberateStatusButton, bool>(nameof(IsSeries));

		public LiberatedStatus BookStatus { get => GetValue(BookStatusProperty); set => SetValue(BookStatusProperty, value); }
		public LiberatedStatus? PdfStatus { get => GetValue(PdfStatusProperty); set => SetValue(PdfStatusProperty, value); }
		public bool IsUnavailable { get => GetValue(IsUnavailableProperty); set => SetValue(IsUnavailableProperty, value); }
		public bool Expanded { get => GetValue(ExpandedProperty); set => SetValue(ExpandedProperty, value); }
		public bool IsSeries { get => GetValue(IsSeriesProperty); set => SetValue(IsSeriesProperty, value); }

		private readonly LiberateStatusButtonViewModel viewModel = new();

		public LiberateStatusButton()
		{
			InitializeComponent();
			button.DataContext = viewModel;

			if (Design.IsDesignMode)
			{
				BookStatus = LiberatedStatus.PartialDownload;
				PdfStatus = null;
				IsSeries = true;
			}

			DataContextChanged += LiberateStatusButton_DataContextChanged;
		}

		private void LiberateStatusButton_DataContextChanged(object sender, EventArgs e)
		{
			//Force book status recheck when an entry is scrolled into view.
			//This will force a recheck for a paprtially downloaded file.
			var status = DataContext as ILibraryBookEntry;
			status?.Liberate.Invalidate(nameof(status.Liberate.BookStatus));
		}

		private void Button_Click(object sender, RoutedEventArgs e) => Click?.Invoke(this, EventArgs.Empty);

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			if (change.Property == BookStatusProperty)
			{
				viewModel.IsError = BookStatus is LiberatedStatus.Error;
				viewModel.RedVisible = BookStatus is LiberatedStatus.NotLiberated;
				viewModel.YellowVisible = BookStatus is LiberatedStatus.PartialDownload;
				viewModel.GreenVisible = BookStatus is LiberatedStatus.Liberated;
			}
			else if (change.Property == PdfStatusProperty)
			{
				viewModel.PdfDownloadedVisible = PdfStatus is LiberatedStatus.Liberated;
				viewModel.PdfNotDownloadedVisible = PdfStatus is LiberatedStatus.NotLiberated;
			}
			else if (change.Property == IsSeriesProperty)
			{
				viewModel.IsSeries = IsSeries;
			}
			else if (change.Property == ExpandedProperty)
			{
				viewModel.Expanded = Expanded;
			}

			viewModel.IsButtonEnabled = !viewModel.IsError && (!IsUnavailable || (BookStatus is LiberatedStatus.Liberated && PdfStatus is null or LiberatedStatus.Liberated));

			base.OnPropertyChanged(change);
		}
	}
}
