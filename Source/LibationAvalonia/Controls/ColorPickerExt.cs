using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace LibationAvalonia.Controls;

/// <summary> A <see cref="ColorPicker"/> which reports when its drop-down flyout opens and closes. </summary>
public class ColorPickerExt : ColorPicker
{
	protected override Type StyleKeyOverride => typeof(ColorPicker);

	public event EventHandler? FlyoutOpened;
	public event EventHandler? FlyoutClosed;

	private FlyoutBase? Flyout;

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		if (Flyout is not null)
		{
			Flyout.Opened -= Flyout_Opened;
			Flyout.Closed -= Flyout_Closed;
		}

		//The ColorPicker's template is a DropDownButton whose flyout hosts the editor.
		Flyout = this.GetVisualDescendants().OfType<DropDownButton>().FirstOrDefault()?.Flyout;

		if (Flyout is not null)
		{
			Flyout.Opened += Flyout_Opened;
			Flyout.Closed += Flyout_Closed;
		}
	}

	private void Flyout_Opened(object? sender, EventArgs e) => FlyoutOpened?.Invoke(this, e);

	private void Flyout_Closed(object? sender, EventArgs e) => FlyoutClosed?.Invoke(this, e);
}
