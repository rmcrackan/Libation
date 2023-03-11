using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase
{
	public class IcoEncoder : IImageEncoder
	{
		public bool SkipMetadata { get; init; } = true;

		public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
		{
			// https://stackoverflow.com/a/21389253

			using var ms = new MemoryStream();
			//Knowing the image size ahead of time removes the
			//requirement of the output stream to support seeking.
			image.SaveAsPng(ms);

			//Disposing of the BinaryWriter disposes the soutput stream. Let the caller clean up.
			var bw = new BinaryWriter(stream);

			// Header
			bw.Write((short)0);   // 0-1 : reserved
			bw.Write((short)1);   // 2-3 : 1=ico, 2=cur
			bw.Write((short)1);   // 4-5 : number of images

			// Image directory
			var w = image.Width;
			if (w >= 256) w = 0;
			bw.Write((byte)w);                      // 0 : width of image
			var h = image.Height;
			if (h >= 256) h = 0;
			bw.Write((byte)h);                      // 1 : height of image
			bw.Write((byte)0);                      // 2 : number of colors in palette
			bw.Write((byte)0);                      // 3 : reserved
			bw.Write((short)0);                     // 4 : number of color planes
			bw.Write((short)0);                     // 6 : bits per pixel
			bw.Write((int)ms.Position);             // 8 : image size
			bw.Write((int)stream.Position + 4);     // 12: offset of image data
			ms.Position = 0;
			ms.CopyTo(stream);                      // Image data
		}

		public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
			=> throw new NotImplementedException();
	}
}
