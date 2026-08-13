using SkiaSharp;
using System;
using System.Collections.Generic;

namespace LibationUiBase.StatusIcons;

/// <summary>
/// Rasterizes the Liberate column's icons from <see cref="LiberateIconGeometry"/> so that every UI
/// displays the same image. Renderings are cached; there are only a couple dozen of them.
/// </summary>
public static class StatusImageGenerator
{
	/// <summary>
	/// Icons are rendered this many pixels per unit of artwork so they stay sharp on scaled displays.
	/// Consumers which size an image from its pixel dimensions must divide by this.
	/// </summary>
	public const int RenderScale = 2;

	/// <summary>Render <paramref name="descriptor"/>'s icon to a PNG.</summary>
	public static byte[] GetPng(LiberateIconDescriptor descriptor)
	{
		lock (cache)
		{
			if (!cache.TryGetValue(descriptor, out var png))
				cache[descriptor] = png = Render(descriptor);
			return png;
		}
	}

	private static readonly Dictionary<LiberateIconDescriptor, byte[]> cache = [];

	private static byte[] Render(LiberateIconDescriptor descriptor)
	{
		var (layers, size) = Compose(descriptor, LiberateIconPalette.For(descriptor.IsDark));

		var info = new SKImageInfo(
			(int)MathF.Ceiling(size.Width * RenderScale),
			(int)MathF.Ceiling(size.Height * RenderScale),
			SKColorType.Rgba8888,
			SKAlphaType.Premul);

		using var surface = SKSurface.Create(info);
		surface.Canvas.Clear(SKColors.Transparent);
		surface.Canvas.Scale(RenderScale);

		using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
		foreach (var (path, color) in layers)
		{
			paint.Color = color;
			surface.Canvas.DrawPath(path, paint);
			path.Dispose();
		}

		using var image = surface.Snapshot();
		using var png = image.Encode(SKEncodedImageFormat.Png, 100);
		return png.ToArray();
	}

	/// <summary>Lay the icon's paths out, bottom layer first, in artwork units.</summary>
	private static (List<(SKPath Path, SKColor Color)> Layers, SKSize Size) Compose(LiberateIconDescriptor descriptor, LiberateIconPalette palette)
	{
		var layers = new List<(SKPath, SKColor)>();

		switch (descriptor.Kind)
		{
			case LiberateIconKind.Error:
			{
				var error = ParsePath(LiberateIconGeometry.BookError);
				var size = MoveToOrigin(error);
				layers.Add((error, palette.Error));
				return (layers, size);
			}

			case LiberateIconKind.SeriesCollapsed:
			case LiberateIconKind.SeriesExpanded:
			{
				//The icon offers the action, not the state: an expanded series shows a minus to
				//collapse it, and a collapsed series shows a plus to expand it.
				var series = ParsePath(LiberateIconGeometry.SeriesMinus);
				if (descriptor.Kind is LiberateIconKind.SeriesCollapsed)
					series = Union(series, ParsePath(LiberateIconGeometry.SeriesPlusBar));
				var size = MoveToOrigin(series);
				layers.Add((series, palette.IconFill));
				return (layers, size);
			}

			default:
				return ComposeStoplight(descriptor, palette, layers);
		}
	}

	private static (List<(SKPath Path, SKColor Color)>, SKSize) ComposeStoplight(LiberateIconDescriptor descriptor, LiberateIconPalette palette, List<(SKPath, SKColor)> layers)
	{
		var body = ParsePath(LiberateIconGeometry.StoplightBody);
		var bodyBounds = body.TightBounds;
		var bodyScale = LiberateIconGeometry.StoplightHeight / bodyBounds.Height;
		var stoplightWidth = bodyBounds.Width * bodyScale;
		body.Transform(ScaleThenTranslate(bodyScale, -bodyBounds.Left * bodyScale, -bodyBounds.Top * bodyScale));

		var lampTop = descriptor.Lamp switch
		{
			StoplightLamp.Red => LiberateIconGeometry.RedLampTop,
			StoplightLamp.Yellow => LiberateIconGeometry.YellowLampTop,
			_ => LiberateIconGeometry.GreenLampTop
		};

		//The lamp goes under the body, so it shows through the bezel cut out of it.
		var lamp = new SKPath();
		lamp.AddRect(SKRect.Create(LiberateIconGeometry.LampLeft, lampTop, LiberateIconGeometry.LampWidth, LiberateIconGeometry.LampHeight));
		layers.Add((lamp, palette.Lamp(descriptor.Lamp)));
		layers.Add((body, palette.IconFill));

		var width = stoplightWidth;

		if (descriptor.Pdf is not PdfOverlay.None)
		{
			var pdf = ParsePath(LiberateIconGeometry.PdfDocument);
			if (descriptor.Pdf is PdfOverlay.NotDownloaded)
				pdf = Union(pdf, ParsePath(LiberateIconGeometry.PdfDownArrow));

			//Scale to a fixed width, then center vertically, so the document glyph is the same size
			//whether or not the download arrow hangs beneath it.
			var pdfBounds = pdf.TightBounds;
			var pdfScale = LiberateIconGeometry.PdfWidth / pdfBounds.Width;
			var left = stoplightWidth + LiberateIconGeometry.PdfLeftMargin;
			var top = (LiberateIconGeometry.StoplightHeight - pdfBounds.Height * pdfScale) / 2;
			pdf.Transform(ScaleThenTranslate(pdfScale, left - pdfBounds.Left * pdfScale, top - pdfBounds.Top * pdfScale));

			layers.Add((pdf, palette.IconFill));
			width = left + LiberateIconGeometry.PdfWidth;
		}

		return (layers, new SKSize(width, LiberateIconGeometry.StoplightHeight));
	}

	private static SKPath ParsePath(string svgPathData)
	{
		var path = SKPath.ParseSvgPathData(svgPathData)
			?? throw new InvalidOperationException($"Unparsable icon path data: {svgPathData}");
		path.FillType = SKPathFillType.EvenOdd;
		return path;
	}

	private static SKPath Union(SKPath first, SKPath second)
	{
		using (first)
		using (second)
			return first.Op(second, SKPathOp.Union)
				?? throw new InvalidOperationException("Could not union icon paths.");
	}

	/// <summary>Shift a path so its bounds start at the origin, and return those bounds' size.</summary>
	private static SKSize MoveToOrigin(SKPath path)
	{
		var bounds = path.TightBounds;
		path.Transform(SKMatrix.CreateTranslation(-bounds.Left, -bounds.Top));
		return new SKSize(bounds.Width, bounds.Height);
	}

	private static SKMatrix ScaleThenTranslate(float scale, float translateX, float translateY)
		=> new()
		{
			ScaleX = scale,
			ScaleY = scale,
			TransX = translateX,
			TransY = translateY,
			Persp2 = 1
		};
}
