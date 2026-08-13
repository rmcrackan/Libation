using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationUiBase.GridView;
using System;

namespace LibationAvalonia.Views;

public partial class LiberateStatusButton : UserControl
{
	public event EventHandler? Click;

	public static readonly StyledProperty<LiberatedStatus> BookStatusProperty =
	AvaloniaProperty.Register<LiberateStatusButton, LiberatedStatus>(nameof(BookStatus));

	public static readonly StyledProperty<LiberatedStatus?> PdfStatusProperty =
	AvaloniaProperty.Register<LiberateStatusButton, LiberatedStatus?>(nameof(PdfStatus));

	public static readonly StyledProperty<bool> IsUnavailableProperty =
	AvaloniaProperty.Register<LiberateStatusButton, bool>(nameof(IsUnavailable));

	public static readonly StyledProperty<IImage?> ButtonImageProperty =
	AvaloniaProperty.Register<LiberateStatusButton, IImage?>(nameof(ButtonImage));

	public LiberatedStatus BookStatus { get => GetValue(BookStatusProperty); set => SetValue(BookStatusProperty, value); }
	public LiberatedStatus? PdfStatus { get => GetValue(PdfStatusProperty); set => SetValue(PdfStatusProperty, value); }
	public bool IsUnavailable { get => GetValue(IsUnavailableProperty); set => SetValue(IsUnavailableProperty, value); }

	/// <summary>The shared rendering of this entry's status, from <see cref="EntryStatus.ButtonImage"/>.</summary>
	public IImage? ButtonImage { get => GetValue(ButtonImageProperty); set => SetValue(ButtonImageProperty, value); }

	private readonly LiberateStatusButtonViewModel viewModel = new();

	public LiberateStatusButton()
	{
		InitializeComponent();
		button.DataContext = viewModel;

		DataContextChanged += LiberateStatusButton_DataContextChanged;

		//The icon is rendered for a specific theme, so it has to be re-rendered when the theme changes.
		ActualThemeVariantChanged += (_, _) => (DataContext as GridEntry)?.Liberate?.Invalidate(nameof(EntryStatus.ButtonImage));
	}

	private void LiberateStatusButton_DataContextChanged(object? sender, EventArgs e)
	{
		//Force book status recheck when an entry is scrolled into view.
		//This will force a recheck for a partially downloaded file.
		var status = DataContext as LibraryBookEntry;
		status?.Liberate?.Invalidate(nameof(status.Liberate.BookStatus), nameof(status.Liberate.ButtonImage));
	}

	private void Button_Click(object sender, RoutedEventArgs e) => Click?.Invoke(this, EventArgs.Empty);

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (change.Property == ButtonImageProperty)
			viewModel.ButtonImage = ButtonImage;

		viewModel.IsButtonEnabled = BookStatus is not LiberatedStatus.Error && (!IsUnavailable || (BookStatus is LiberatedStatus.Liberated && PdfStatus is null or LiberatedStatus.Liberated));

		base.OnPropertyChanged(change);
	}
}
