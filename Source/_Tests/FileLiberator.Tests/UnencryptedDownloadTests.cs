using AaxDecrypter;
using AAXClean;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mpeg4Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

/// <summary>
/// From issue #2105: the download loop logs and swallows its errors, so an unencrypted download that stopped
/// early still completed normally and the partial file was handed on as the finished book.
/// </summary>
[TestClass]
public class UnencryptedDownloadTests
{
	private const int FileSize = 4 * 1024 * 1024;

	private string root = string.Empty;

	[TestInitialize]
	public void CreateRoot() => root = Directory.CreateTempSubdirectory("UnencryptedDownloadTests").FullName;

	[TestCleanup]
	public void DeleteRoot() => Directory.Delete(root, true);

	[TestMethod]
	public async Task a_download_that_ends_early_fails_and_keeps_the_partial_file_for_resume()
	{
		using var server = new RangeServer(firstResponseBytes: FileSize / 2);
		var (success, downloader, outputs) = await DownloadAsync(server.Url);

		Assert.IsFalse(success);
		Assert.IsNotNull(downloader.FailureException);
		Assert.IsFalse(outputs.Any(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)), "the partial file was handed on as output");
		var partial = new FileInfo(Path.Combine(root, "cache", $"{Options.ProductId}.aaxc"));
		Assert.IsTrue(partial.Exists, "the partial file was not kept for resume");
		Assert.IsTrue(partial.Length < FileSize);
	}

	[TestMethod]
	public async Task a_complete_download_succeeds_with_the_whole_file()
	{
		using var server = new RangeServer(firstResponseBytes: FileSize);
		var (success, _, outputs) = await DownloadAsync(server.Url);

		Assert.IsTrue(success);
		var output = outputs.Single(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(FileSize, new FileInfo(output).Length);
	}

	private async Task<(bool success, UnencryptedAudiobookDownloader downloader, List<string> outputs)> DownloadAsync(string url)
	{
		var downloader = new UnencryptedAudiobookDownloader(Path.Combine(root, "out"), Path.Combine(root, "cache"), new Options(url));
		var outputs = new List<string>();
		downloader.TempFileCreated += (_, f) => outputs.Add(f.FilePath);
		var success = await downloader.RunAsync();
		return (success, downloader, outputs);
	}

	/// <summary>
	/// Serves <see cref="FileSize"/> bytes from the requested Range start. The first response sends
	/// <c>firstResponseBytes</c> and closes. Later responses send headers and close, so a retry makes no progress.
	/// </summary>
	private sealed class RangeServer : IDisposable
	{
		private readonly TcpListener listener = new(IPAddress.Loopback, 0);
		private readonly CancellationTokenSource stop = new();
		private readonly byte[] payload = new byte[FileSize];
		private readonly int firstResponseBytes;
		private int requests;

		public string Url => $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/book.mp3";

		public RangeServer(int firstResponseBytes)
		{
			this.firstResponseBytes = firstResponseBytes;
			new Random(1).NextBytes(payload);
			listener.Start();
			_ = Task.Run(ServeAsync);
		}

		private async Task ServeAsync()
		{
			while (!stop.IsCancellationRequested)
			{
				using var client = await listener.AcceptTcpClientAsync(stop.Token);
				var stream = client.GetStream();
				using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
				long start = 0;
				string? line;
				while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
					if (line.StartsWith("Range:", StringComparison.OrdinalIgnoreCase))
						start = long.Parse(line.Split('=')[1].TrimEnd('-'));

				var header = $"HTTP/1.1 206 Partial Content\r\nContent-Type: audio/mpeg\r\nContent-Length: {FileSize - start}\r\nContent-Range: bytes {start}-{FileSize - 1}/{FileSize}\r\nConnection: close\r\n\r\n";
				await stream.WriteAsync(Encoding.ASCII.GetBytes(header));
				if (Interlocked.Increment(ref requests) == 1)
					await stream.WriteAsync(payload.AsMemory((int)start, firstResponseBytes - (int)start));
				await stream.FlushAsync();
				client.Client.Shutdown(SocketShutdown.Both);
			}
		}

		public void Dispose()
		{
			stop.Cancel();
			listener.Stop();
		}
	}

	private sealed class Options(string url) : IDownloadOptions
	{
		public const string ProductId = "B0TEST2105";

		public event EventHandler<long>? DownloadSpeedChanged { add { } remove { } }
		public string DownloadUrl => url;
		public string UserAgent => "UnencryptedDownloadTests";
		public KeyData[]? DecryptionKeys => null;
		public TimeSpan RuntimeLength => TimeSpan.FromMinutes(10);
		public OutputFormat OutputFormat => OutputFormat.Mp3;
		public bool StripUnabridged => false;
		public bool CreateCueSheet => false;
		public long DownloadSpeedBps => 0;
		public ChapterInfo ChapterInfo => new();
		public bool FixupFile => false;
		public string? AudibleProductId => ProductId;
		public string? Title => null;
		public string? Subtitle => null;
		public string? Publisher => null;
		public string? Language => null;
		public string? SeriesName => null;
		public string? SeriesNumber => null;
		public NAudio.Lame.LameConfig? LameConfig => null;
		public bool Downsample => false;
		public bool MatchSourceBitrate => false;
		public bool MoveMoovToBeginning => false;
		public string GetMultipartTitle(MultiConvertFileProperties props) => throw new NotSupportedException();
		public FileType? InputType => null;
	}
}
