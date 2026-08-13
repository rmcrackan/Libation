using LibationUiBase.StatusIcons;
using SkiaSharp;

namespace LibationUiBase.Tests;

[TestClass]
public class LiberateIconTests
{
	public static IEnumerable<object[]> AllIcons()
		=> LiberateIconDescriptor.All().Select(d => new object[] { d });

	[TestMethod]
	[DynamicData(nameof(AllIcons))]
	public void every_icon_renders_a_png(LiberateIconDescriptor descriptor)
	{
		var png = StatusImageGenerator.GetPng(descriptor);

		var (width, height) = PngSize(png);
		Assert.IsGreaterThan(0, width);
		Assert.IsGreaterThan(0, height);
	}

	[TestMethod]
	public void the_finite_set_of_icons_is_the_18_stoplights_plus_3_others_per_theme()
	{
		var all = LiberateIconDescriptor.All().ToList();

		Assert.AreEqual(42, all.Count);
		Assert.AreEqual(42, all.Distinct().Count());
		Assert.AreEqual(36, all.Count(d => d.Kind is LiberateIconKind.Book));
	}

	[TestMethod]
	public void every_icon_renders_differently()
	{
		var renderings = LiberateIconDescriptor.All()
			.Select(d => Convert.ToHexString(StatusImageGenerator.GetPng(d)))
			.ToList();

		Assert.AreEqual(renderings.Count, renderings.Distinct().Count());
	}

	[TestMethod]
	[DynamicData(nameof(AllIcons))]
	public void renderings_are_cached(LiberateIconDescriptor descriptor)
	{
		//The grid asks for these constantly, and re-rendering is far more expensive than a lookup.
		Assert.AreSame(StatusImageGenerator.GetPng(descriptor), StatusImageGenerator.GetPng(descriptor));
	}

	[TestMethod]
	public void stoplights_are_all_the_same_height()
	{
		var heights = LiberateIconDescriptor.All()
			.Where(d => d.Kind is LiberateIconKind.Book)
			.Select(d => PngSize(StatusImageGenerator.GetPng(d)).Height)
			.Distinct()
			.ToList();

		Assert.HasCount(1, heights);
	}

	[TestMethod]
	[DataRow(PdfOverlay.Downloaded)]
	[DataRow(PdfOverlay.NotDownloaded)]
	public void a_pdf_overlay_widens_the_stoplight(PdfOverlay pdf)
	{
		var withoutPdf = PngSize(StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: false, isDark: false)));
		var withPdf = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: false, isDark: false)));

		Assert.IsGreaterThan(withoutPdf.Width, withPdf.Width);
		Assert.AreEqual(withoutPdf.Height, withPdf.Height);
	}

	[TestMethod]
	public void series_and_error_icons_do_not_vary_by_lamp_pdf_or_plus()
	{
		//Otherwise every lamp/PDF/Plus combination would get its own cache entry for the same image.
		Assert.AreEqual(LiberateIconDescriptor.ForSeries(expanded: true, isDark: false), LiberateIconDescriptor.ForSeries(expanded: true, isDark: false));
		Assert.AreEqual(default, LiberateIconDescriptor.ForError(isDark: false).Lamp);
		Assert.AreEqual(default, LiberateIconDescriptor.ForError(isDark: false).Pdf);
		Assert.IsFalse(LiberateIconDescriptor.ForError(isDark: false).IsPlus);
		Assert.AreEqual(default, LiberateIconDescriptor.ForSeries(expanded: true, isDark: false).Lamp);
		Assert.AreEqual(default, LiberateIconDescriptor.ForSeries(expanded: true, isDark: false).Pdf);
		Assert.IsFalse(LiberateIconDescriptor.ForSeries(expanded: true, isDark: false).IsPlus);
	}

	#region Audible Plus badge

	[TestMethod]
	[DataRow(PdfOverlay.None)]
	[DataRow(PdfOverlay.Downloaded)]
	[DataRow(PdfOverlay.NotDownloaded)]
	public void the_plus_badge_widens_the_icon_without_making_it_taller(PdfOverlay pdf)
	{
		//The badge hangs off the stoplight's right, and sits flush with the top edge rather than
		//above it, so the stoplight itself is drawn identically for a Plus and a purchased title.
		var purchased = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: false, isDark: false)));
		var plus = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: true, isDark: false)));

		Assert.AreEqual(purchased.Height, plus.Height);
		Assert.IsGreaterThan(purchased.Width, plus.Width);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void only_plus_titles_get_a_badge(bool isDark)
	{
		var badgeColor = LiberateIconPalette.For(isDark).PlusBadge;

		foreach (var pdf in Enum.GetValues<PdfOverlay>())
		{
			Assert.AreEqual(0, FindColor(StatusImageGenerator.GetPng(Book(pdf, isPlus: false, isDark)), badgeColor).Count);
			Assert.IsGreaterThan(0, FindColor(StatusImageGenerator.GetPng(Book(pdf, isPlus: true, isDark)), badgeColor).Count);
		}
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void the_badge_is_a_circle_in_the_upper_right_corner(bool isDark)
	{
		var png = StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: true, isDark));
		var (width, height) = PngSize(png);
		var badge = FindColor(png, LiberateIconPalette.For(isDark).PlusBadge).Bounds;

		//A circle's ink is as wide as it is tall. Antialiasing frays the outermost pixel or two of
		//every edge, so these compare shapes and margins rather than exact coordinates.
		Assert.IsLessThanOrEqualTo(2, Math.Abs(badge.Width - badge.Height));

		//Upper: it rides the icon's top edge and stays out of the bottom half.
		Assert.IsLessThanOrEqualTo(3, badge.Top);
		Assert.IsLessThan(height / 2, badge.Bottom);

		//Right: it hangs off the stoplight, whose right edge is where a purchased icon ends, and
		//reaches the icon's own right edge, since the badge is what widened it.
		var stoplightWidth = PngSize(StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: false, isDark))).Width;
		Assert.IsGreaterThan(stoplightWidth, badge.Right);
		Assert.IsGreaterThan(width - 4, badge.Right);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void the_badges_plus_is_painted_a_solid_color_rather_than_punched_out(bool isDark)
	{
		//The plus is white on light and black on dark. Either way it is opaque: knocking it out to
		//transparent would show the grid row through the badge instead.
		var png = StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: true, isDark));
		var badge = FindColor(png, LiberateIconPalette.For(isDark).PlusBadge).Bounds;

		using var bitmap = SKBitmap.Decode(png);
		Assert.IsNotNull(bitmap);

		Assert.AreEqual(isDark ? SKColors.Black : SKColors.White, bitmap.GetPixel(badge.MidX, badge.MidY));
	}

	[TestMethod]
	[DataRow(PdfOverlay.Downloaded)]
	[DataRow(PdfOverlay.NotDownloaded)]
	public void the_pdf_glyph_is_pushed_clear_of_the_badge(PdfOverlay pdf)
	{
		//Otherwise the PDF's top-left corner would collide with the badge hanging off the stoplight.
		var palette = LiberateIconPalette.For(isDark: false);
		var png = StatusImageGenerator.GetPng(Book(pdf, isPlus: true, isDark: false));
		var badge = FindColor(png, palette.PlusBadge).Bounds;

		//The part of the badge which hangs past the stoplight body - a purchased no-PDF icon ends at
		//the body's right edge - should have no PDF ink behind it.
		var bodyRight = PngSize(StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: false, isDark: false))).Width;
		using var bitmap = SKBitmap.Decode(png);
		Assert.IsNotNull(bitmap);

		for (var x = bodyRight; x < badge.Right; x++)
			for (var y = badge.Top; y < badge.Bottom; y++)
				Assert.AreNotEqual(palette.IconFill, bitmap.GetPixel(x, y), $"PDF ink inside the badge's rows at {x},{y}");
	}

	#endregion

	private static LiberateIconDescriptor Book(PdfOverlay pdf, bool isPlus, bool isDark)
		=> LiberateIconDescriptor.ForBook(StoplightLamp.Green, pdf, isPlus, isDark);

	/// <summary>The bounding box of, and number of, pixels painted exactly <paramref name="color"/>.</summary>
	private static (SKRectI Bounds, int Count) FindColor(byte[] png, SKColor color)
	{
		using var bitmap = SKBitmap.Decode(png);
		Assert.IsNotNull(bitmap);

		int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
		var count = 0;

		for (var x = 0; x < bitmap.Width; x++)
			for (var y = 0; y < bitmap.Height; y++)
				if (bitmap.GetPixel(x, y) == color)
				{
					left = Math.Min(left, x);
					top = Math.Min(top, y);
					right = Math.Max(right, x + 1);
					bottom = Math.Max(bottom, y + 1);
					count++;
				}

		return (count == 0 ? SKRectI.Empty : new SKRectI(left, top, right, bottom), count);
	}

	[TestMethod]
	public void a_collapsed_series_shows_a_plus_and_an_expanded_one_a_minus()
	{
		//The plus is the minus plus a bar, so it is strictly the inkier of the two. Asserting that
		//pins which way round they go: the icon offers the action, not the state.
		var collapsed = InkedPixels(StatusImageGenerator.GetPng(LiberateIconDescriptor.ForSeries(expanded: false, isDark: false)));
		var expanded = InkedPixels(StatusImageGenerator.GetPng(LiberateIconDescriptor.ForSeries(expanded: true, isDark: false)));

		Assert.IsGreaterThan(expanded, collapsed);
	}

	[TestMethod]
	public void icons_are_rendered_above_their_logical_size()
	{
		//WinForms sizes the icon from its pixel dimensions, so the scale has to divide evenly.
		var (width, height) = PngSize(StatusImageGenerator.GetPng(LiberateIconDescriptor.ForError(isDark: false)));

		Assert.IsGreaterThan(1, StatusImageGenerator.RenderScale);
		Assert.AreEqual(0, width % StatusImageGenerator.RenderScale);
		Assert.AreEqual(0, height % StatusImageGenerator.RenderScale);
	}

	/// <summary>Count how many pixels the icon painted on.</summary>
	private static int InkedPixels(byte[] png)
	{
		using var bitmap = SKBitmap.Decode(png);
		Assert.IsNotNull(bitmap);

		var inked = 0;
		for (var x = 0; x < bitmap.Width; x++)
			for (var y = 0; y < bitmap.Height; y++)
				if (bitmap.GetPixel(x, y).Alpha > 0)
					inked++;
		return inked;
	}

	/// <summary>Read the dimensions out of a PNG's IHDR, which also asserts that it is a PNG.</summary>
	private static (int Width, int Height) PngSize(byte[] png)
	{
		Assert.IsNotNull(png);
		Assert.IsGreaterThan(24, png.Length);
		CollectionAssert.AreEqual(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A }, png[..8]);

		return (
			System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)),
			System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
	}
}
