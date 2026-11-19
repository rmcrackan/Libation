using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public abstract class DialogWindow : Window
{
	protected bool CancelOnEscape { get; set; } = true;
	protected bool SaveOnEnter { get; set; } = true;
	public bool SaveAndRestorePosition { get; set; }
	public Control? ControlToFocusOnShow { get; set; }
	protected override Type StyleKeyOverride => typeof(DialogWindow);
	public DialogResult DialogResult { get; private set; } = DialogResult.None;

	public DialogWindow(bool saveAndRestorePosition = true)
	{
		SaveAndRestorePosition = saveAndRestorePosition;
		KeyDown += DialogWindow_KeyDown;
		Loaded += DialogWindow_Loaded;
		Opened += DialogWindow_Opened;
		Closing += DialogWindow_Closing;

		if (Design.IsDesignMode)
		{
			var themeVariant = Configuration.CreateMockInstance().ThemeVariant;
			RequestedThemeVariant = themeVariant switch
			{
				Configuration.Theme.Dark => ThemeVariant.Dark,
				Configuration.Theme.Light => ThemeVariant.Light,
				_ => ThemeVariant.Default,
			};
		}
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		if (MinHeight == MaxHeight && MinWidth == MaxWidth)
		{
			CanResize = false;
			CanMinimize = false;
			CanMaximize = false;
			Height = MinHeight;
			Width = MinWidth;
		}
	}

	private void DialogWindow_Loaded(object? sender, EventArgs e)
	{
		this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
		if (SaveAndRestorePosition)
			this.RestoreSizeAndLocation(Configuration.Instance);
	}

	private void DialogWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
	{
		if (SaveAndRestorePosition)
			this.SaveSizeAndLocation(Configuration.Instance);
	}

	private void DialogWindow_Opened(object? sender, EventArgs e)
	{
		ControlToFocusOnShow?.Focus();
	}

	/// <summary>
	/// Once the window opens, widens it, up to the screen's width, until <paramref name="control"/> fits at its natural width.
	/// For rows of buttons or labels sized for English: a longer translation would otherwise run off the edge.
	/// </summary>
	protected void WidenToFit(Control control)
	{
		Opened += (_, _) =>
		{
			control.Measure(Avalonia.Size.Infinity);
			var missing = control.DesiredSize.Width - control.Bounds.Width - control.Margin.Left - control.Margin.Right;
			control.InvalidateMeasure();
			if (missing < 1)
				return;

			var screenWidth = Screens.ScreenFromWindow(this) is { } screen ? screen.WorkingArea.Width / screen.Scaling : double.PositiveInfinity;
			var width = Math.Min(Bounds.Width + Math.Ceiling(missing), screenWidth);
			MaxWidth = Math.Max(MaxWidth, width);
			Width = width;
		};
	}

	public void Close(DialogResult dialogResult)
	{
		DialogResult = dialogResult;
		base.Close(dialogResult);
	}

	protected virtual void SaveAndClose() => Close(DialogResult.OK);
	protected virtual async Task SaveAndCloseAsync() => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(SaveAndClose);
	protected virtual void CancelAndClose() => Close(DialogResult.Cancel);
	protected virtual async Task CancelAndCloseAsync() => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(CancelAndClose);

	private async void DialogWindow_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
	{
		if (CancelOnEscape && e.Key == Avalonia.Input.Key.Escape)
			await CancelAndCloseAsync();
		else if (SaveOnEnter && e.Key == Avalonia.Input.Key.Return)
			await SaveAndCloseAsync();
	}
}
