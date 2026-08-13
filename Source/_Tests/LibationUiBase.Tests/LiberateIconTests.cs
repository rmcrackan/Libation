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
	public void the_finite_set_of_icons_is_the_9_stoplights_plus_3_others_per_theme()
	{
		var all = LiberateIconDescriptor.All().ToList();

		Assert.AreEqual(24, all.Count);
		Assert.AreEqual(24, all.Distinct().Count());
		Assert.AreEqual(18, all.Count(d => d.Kind is LiberateIconKind.Book));
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
		var withoutPdf = PngSize(StatusImageGenerator.GetPng(LiberateIconDescriptor.ForBook(StoplightLamp.Green, PdfOverlay.None, isDark: false)));
		var withPdf = PngSize(StatusImageGenerator.GetPng(LiberateIconDescriptor.ForBook(StoplightLamp.Green, pdf, isDark: false)));

		Assert.IsGreaterThan(withoutPdf.Width, withPdf.Width);
		Assert.AreEqual(withoutPdf.Height, withPdf.Height);
	}

	[TestMethod]
	public void series_and_error_icons_do_not_vary_by_lamp_or_pdf()
	{
		//Otherwise every lamp/PDF combination would get its own cache entry for the same image.
		Assert.AreEqual(LiberateIconDescriptor.ForSeries(expanded: true, isDark: false), LiberateIconDescriptor.ForSeries(expanded: true, isDark: false));
		Assert.AreEqual(default, LiberateIconDescriptor.ForError(isDark: false).Lamp);
		Assert.AreEqual(default, LiberateIconDescriptor.ForError(isDark: false).Pdf);
		Assert.AreEqual(default, LiberateIconDescriptor.ForSeries(expanded: true, isDark: false).Lamp);
		Assert.AreEqual(default, LiberateIconDescriptor.ForSeries(expanded: true, isDark: false).Pdf);
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
