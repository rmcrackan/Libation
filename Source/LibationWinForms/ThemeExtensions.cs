using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms;

internal static class ThemeExtensions
{
	private static readonly Color LinkLabelNew = Color.FromKnownColor(KnownColor.Blue);
	private static readonly Color LinkLabelVisited = Color.FromKnownColor(KnownColor.Purple);
	private static readonly Color LinkLabelNew_Dark = Color.FromKnownColor(KnownColor.CornflowerBlue);
	private static readonly Color LinkLabelVisited_Dark = Color.FromKnownColor(KnownColor.Orchid);
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
}
