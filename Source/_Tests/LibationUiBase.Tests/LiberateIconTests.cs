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
	public void the_badge_never_makes_the_icon_taller(PdfOverlay pdf)
	{
		//It sits flush with the top edge rather than above it, so the stoplight is drawn at exactly
		//the same size for a Plus title as for a purchased one.
		var purchased = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: false, isDark: false)));
		var plus = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: true, isDark: false)));

		Assert.AreEqual(purchased.Height, plus.Height);
	}

	[TestMethod]
	[DataRow(PdfOverlay.Downloaded)]
	[DataRow(PdfOverlay.NotDownloaded)]
	public void the_badge_costs_no_width_on_an_icon_which_has_a_pdf(PdfOverlay pdf)
	{
		//The badge overlaps the PDF instead of displacing it, so the grid's widest icon is the same
		//as it was before badges existed, and the PDF lines up across Plus and purchased rows.
		var purchased = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: false, isDark: false)));
		var plus = PngSize(StatusImageGenerator.GetPng(Book(pdf, isPlus: true, isDark: false)));

		Assert.AreEqual(purchased.Width, plus.Width);
	}

	[TestMethod]
	public void the_badge_widens_an_icon_with_nothing_to_overlap()
	{
		//With no PDF beside it there is nothing to overlap, so the badge hangs off the right.
		var purchased = PngSize(StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: false, isDark: false)));
		var plus = PngSize(StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: true, isDark: false)));

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
	[DataRow(false)]
	[DataRow(true)]
	public void a_rim_of_nothing_separates_the_badge_from_the_stoplight(bool isDark)
	{
		var png = StatusImageGenerator.GetPng(Book(PdfOverlay.None, isPlus: true, isDark));
		var badge = FindColor(png, LiberateIconPalette.For(isDark).PlusBadge).Bounds;

		using var bitmap = SKBitmap.Decode(png);
		Assert.IsNotNull(bitmap);

		//Walking left off the badge along its centre line reaches the stoplight. The gap in between
		//has to be knocked out of the stoplight rather than filled, or the two run together.
		var transparent = 0;
		var x = badge.Left - 1;
		for (; x >= 0 && bitmap.GetPixel(x, badge.MidY).Alpha != byte.MaxValue; x--)
			if (bitmap.GetPixel(x, badge.MidY).Alpha == 0)
				transparent++;

		Assert.IsGreaterThanOrEqualTo(0, x, "Walked off the icon without reaching the stoplight.");
		Assert.IsGreaterThan(2, transparent, "The badge is not separated from the stoplight by a hole.");
	}

	[TestMethod]
	[DataRow(PdfOverlay.None)]
	[DataRow(PdfOverlay.Downloaded)]
	[DataRow(PdfOverlay.NotDownloaded)]
	public void the_rim_keeps_the_stoplight_and_pdf_out_of_the_badge(PdfOverlay pdf)
	{
		//The badge is allowed to overlap both, so the rim is the only thing stopping them running
		//together. Nothing but the badge may be painted inside it.
		var palette = LiberateIconPalette.For(isDark: false);
		var png = StatusImageGenerator.GetPng(Book(pdf, isPlus: true, isDark: false));
		var badge = FindColor(png, palette.PlusBadge).Bounds;

		using var bitmap = SKBitmap.Decode(png);
		Assert.IsNotNull(bitmap);

		//Probe a few pixels past the badge's painted edge, which lands inside the gap. Deriving this
		//from the rim's own width would collapse the probe to nothing if the rim were removed.
		var radius = badge.Width / 2f + 3;

		for (var x = Math.Max(0, (int)(badge.MidX - radius)); x < Math.Min(bitmap.Width, badge.MidX + radius); x++)
			for (var y = Math.Max(0, (int)(badge.MidY - radius)); y < Math.Min(bitmap.Height, badge.MidY + radius); y++)
			{
				var dx = x - badge.MidX;
				var dy = y - badge.MidY;
				if (dx * dx + dy * dy > radius * radius)
					continue;

				var pixel = bitmap.GetPixel(x, y);
				Assert.AreNotEqual(palette.IconFill, pixel, $"stoplight or PDF ink inside the badge's rim at {x},{y}");
				Assert.AreNotEqual(palette.Green, pixel, $"lamp inside the badge's rim at {x},{y}");
			}
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
