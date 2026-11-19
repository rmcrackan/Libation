using Avalonia;
using Avalonia.Controls;
using LibationUiBase.Resources;

namespace LibationAvalonia.Controls;

public partial class GroupBox : ContentControl
{

	public static readonly StyledProperty<string> LabelProperty =
	AvaloniaProperty.Register<GroupBox, string>(nameof(Label));
	public GroupBox()
	{
		InitializeComponent();
		Label = Strings.GroupBox_ThisIsAGroupboxLabel;
	}

	public string Label
	{
		get { return GetValue(LabelProperty); }
		set { SetValue(LabelProperty, value); }
	}
}
