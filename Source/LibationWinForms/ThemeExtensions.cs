using LibationUiBase.ProcessQueue;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms;

internal static class ThemeExtensions
{
	private static readonly Color LinkLabelNew = Color.FromKnownColor(KnownColor.Blue);
	private static readonly Color LinkLabelVisited = Color.FromKnownColor(KnownColor.Purple);
	private static readonly Color LinkLabelNew_Dark = Color.FromKnownColor(KnownColor.CornflowerBlue);
	private static readonly Color LinkLabelVisited_Dark = Color.FromKnownColor(KnownColor.Orchid);
	private static readonly Color FailedColor = Color.LightCoral;
	private static readonly Color FailedColor_Dark = Color.FromArgb(0x50, 0x27, 0x27);
	private static readonly Color CancelledColor = Color.Khaki;
	private static readonly Color CancelledColor_Dark = Color.FromArgb(0x4e, 0x4b, 0x15);
	private static readonly Color SuccessColor = Color.PaleGreen;
	private static readonly Color SuccessColor_Dark = Color.FromArgb(0x1c, 0x3e, 0x20);

	public static Color LinkColor => Application.IsDarkModeEnabled ? LinkLabelNew_Dark : LinkLabelNew;
	public static Color VisitedLinkColor => Application.IsDarkModeEnabled ? LinkLabelVisited_Dark : LinkLabelVisited;
	extension(LinkLabel ll)
	{
		public void SetLinkLabelColors()
		{
			ll.VisitedLinkColor = VisitedLinkColor;
			ll.LinkColor = LinkColor;
		}
	}

	extension(ProcessBookStatus status)
	{
		public Color GetColor() => status switch
		{
			ProcessBookStatus.Completed => Application.IsDarkModeEnabled ? SuccessColor_Dark : SuccessColor,
			ProcessBookStatus.Cancelled => Application.IsDarkModeEnabled ? CancelledColor_Dark : CancelledColor,
			ProcessBookStatus.Queued => SystemColors.Control,
			ProcessBookStatus.Working => SystemColors.Control,
			_ => Application.IsDarkModeEnabled ? FailedColor_Dark : FailedColor
		};
	}
}
