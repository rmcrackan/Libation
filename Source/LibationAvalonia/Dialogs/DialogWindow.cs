using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public abstract class DialogWindow : Window
	{
		protected bool CancelOnEscape { get; set; } = true;
		protected bool SaveOnEnter { get; set; } = true;
		public bool SaveAndRestorePosition { get; set; } = true;
		public Control ControlToFocusOnShow { get; set; }
		protected override Type StyleKeyOverride => typeof(DialogWindow);

		public static readonly StyledProperty<bool> UseCustomTitleBarProperty =
		AvaloniaProperty.Register<DialogWindow, bool>(nameof(UseCustomTitleBar));

		public bool UseCustomTitleBar
		{
			get { return GetValue(UseCustomTitleBarProperty); }
			set { SetValue(UseCustomTitleBarProperty, value); }
		}

		public DialogWindow()
		{
			KeyDown += DialogWindow_KeyDown;
			Initialized += DialogWindow_Initialized;
			Opened += DialogWindow_Opened;
			Closing += DialogWindow_Closing;

			UseCustomTitleBar = Configuration.IsWindows;

			if (Design.IsDesignMode)
				RequestedThemeVariant = ThemeVariant.Dark;
		}

		private bool fixedMinHeight = false;
		private bool fixedMaxHeight = false;
		private bool fixedHeight = false;

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			const int customTitleBarHeight = 30;
			if (UseCustomTitleBar)
			{
				if (change.Property == MinHeightProperty && !fixedMinHeight)
				{
					fixedMinHeight = true;
					MinHeight += customTitleBarHeight;
					fixedMinHeight = false;
				}
				if (change.Property == MaxHeightProperty && !fixedMaxHeight)
				{
					fixedMaxHeight = true;
					MaxHeight += customTitleBarHeight;
					fixedMaxHeight = false;
				}
				if (change.Property == HeightProperty && !fixedHeight)
				{
					fixedHeight = true;
					Height += customTitleBarHeight;
					fixedHeight = false;
				}
			}
			base.OnPropertyChanged(change);
		}

		public DialogWindow(bool saveAndRestorePosition) : this()
		{
			SaveAndRestorePosition = saveAndRestorePosition;
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			if (!UseCustomTitleBar)
				return;

			var closeButton = e.NameScope.Find<Button>("DialogCloseButton");
			var border = e.NameScope.Get<Border>("DialogWindowTitleBorder");
			var titleBlock = e.NameScope.Get<TextBlock>("DialogWindowTitleTextBlock");
			var icon = e.NameScope.Get<Avalonia.Controls.Shapes.Path>("DialogWindowTitleIcon");

			closeButton.Click += CloseButton_Click;
			border.PointerPressed += Border_PointerPressed;
			icon.IsVisible = Icon != null;

			if (MinHeight == MaxHeight && MinWidth == MaxWidth)
			{
				CanResize = false;
				border.Margin = new Thickness(0);
				icon.Margin = new Thickness(8, 5, 0, 5);
			}
		}

		private void Border_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
		{
			if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
				BeginMoveDrag(e);
		}

		private void CloseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			CancelAndClose();
		}

		private void DialogWindow_Initialized(object sender, EventArgs e)
		{
			this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			if (SaveAndRestorePosition)
				this.RestoreSizeAndLocation(Configuration.Instance);
		}

		private void DialogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (SaveAndRestorePosition)
				this.SaveSizeAndLocation(Configuration.Instance);
		}

		private void DialogWindow_Opened(object sender, EventArgs e)
		{
			ControlToFocusOnShow?.Focus();
		}

		protected virtual void SaveAndClose() => Close(DialogResult.OK);
		protected virtual async Task SaveAndCloseAsync() => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(SaveAndClose);
		protected virtual void CancelAndClose() => Close(DialogResult.Cancel);
		protected virtual async Task CancelAndCloseAsync() => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(CancelAndClose);

		private async void DialogWindow_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
		{
			if (CancelOnEscape && e.Key == Avalonia.Input.Key.Escape)
				await CancelAndCloseAsync();
			else if (SaveOnEnter && e.Key == Avalonia.Input.Key.Return)
				await SaveAndCloseAsync();
		}
	}
}
