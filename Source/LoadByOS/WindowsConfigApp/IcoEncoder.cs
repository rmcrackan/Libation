using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace WindowsConfigApp;

public class IcoEncoder
{
	public ReadOnlyCollection<int> ExportSizes { get; }
	public IcoEncoder() : this(512, 256, 128, 96, 64, 48, 32, 24) { }
	public IcoEncoder(params int[] icoSizes)
	{
		Array.Sort(icoSizes);
		ExportSizes = new(icoSizes);
	}

	public void Encode(SKBitmap image, Stream stream)
	{
		// https://stackoverflow.com/a/21389253

		//Knowing the image size ahead of time removes the
		//requirement of the output stream to support seeking.
		byte[][] iconPngs = new byte[ExportSizes.Count][];
		var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);

		for (int i = 0; i < ExportSizes.Count; i++)
		{
			int size = ExportSizes[i];
			var imageInfo = new SKImageInfo(size, size);
			using var resized = image.Resize(imageInfo, samplingOptions);
			using var skImage = SKImage.FromBitmap(resized);
			using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);			
			iconPngs[i] = data.ToArray();
		}

		//Disposing of the BinaryWriter disposes the soutput stream. Let the caller clean up.
		var bw = new BinaryWriter(stream);

		// Header
		bw.Write((short)0);                      // 0-1 : reserved
		bw.Write((short)1);                      // 2-3 : 1=ico, 2=cur
		bw.Write((short)ExportSizes.Count);      // 4-5 : number of images

		int dataStart = 6 + (16 * ExportSizes.Count);
		// Image directory
		for (int i = 0; i < ExportSizes.Count; i++)
		{
			var size = ExportSizes[i] < 256 ? ExportSizes[i] : 0;
			bw.Write((byte)size);                // 0 : width of image
			bw.Write((byte)size);                // 1 : height of image
			bw.Write((byte)0);                   // 2 : number of colors in palette
			bw.Write((byte)0);                   // 3 : reserved
			bw.Write((short)0);                  // 4 : number of color planes
			bw.Write((short)0);                  // 6 : bits per pixel
			bw.Write(iconPngs[i].Length);        // 8 : image size
			bw.Write(dataStart);                 // 12: offset of image data
			dataStart += iconPngs[i].Length;
		}

		// Image data
		for (int i = 0; i < ExportSizes.Count; i++)
			bw.Write(iconPngs[i]);
	}
}
