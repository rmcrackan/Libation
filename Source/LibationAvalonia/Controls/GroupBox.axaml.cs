using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LibationAvalonia.Controls
{
	public partial class GroupBox : ContentControl
	{

		public static readonly StyledProperty<Thickness> BorderWidthProperty =
		AvaloniaProperty.Register<GroupBox, Thickness>(nameof(BorderWidth));

		public static readonly StyledProperty<string> LabelProperty =
		AvaloniaProperty.Register<GroupBox, string>(nameof(Label));
		public GroupBox()
		{
			InitializeComponent();
			BorderWidth = new Thickness(3);
			Label = "This is a groupbox label";
		}
		public Thickness BorderWidth
		{
			get { return GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public string Label
		{
			get { return GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}
	}
}
