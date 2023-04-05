using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace LibationAvalonia.Controls
{
	public partial class GroupBox : ContentControl
	{

		public static readonly StyledProperty<string> LabelProperty =
		AvaloniaProperty.Register<GroupBox, string>(nameof(Label));
		public GroupBox()
		{
			InitializeComponent();
			Label = "This is a groupbox label";
		}

		public string Label
		{
			get { return GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}
	}
}
