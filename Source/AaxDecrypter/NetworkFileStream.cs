using Dinah.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	/// <summary>A resumable, simultaneous file downloader and reader. </summary>
	public class NetworkFileStream : Stream, IUpdatable
	{
		public event EventHandler Updated;

		#region Public Properties

		/// <summary> Location to save the downloaded data. </summary>
		[JsonProperty(Required = Required.Always)]
		public string SaveFilePath { get; }

		/// <summary> Http(s) address of the file to download. </summary>
		[JsonProperty(Required = Required.Always)]
		public Uri Uri { get; private set; }

		/// <summary> Http headers to be sent to the server with the request. </summary>
		[JsonProperty(Required = Required.Always)]
		public Dictionary<string, string> RequestHeaders { get; private set; }

		/// <summary> The position in <see cref="SaveFilePath"/> that has been written and flushed to disk. </summary>
		[JsonProperty(Required = Required.Always)]
		public long WritePosition { get; private set; }

		/// <summary> The total length of the <see cref="Uri"/> file to download. </summary>
		[JsonProperty(Required = Required.Always)]
		public long ContentLength { get; private set; }

		[JsonIgnore]
		public bool IsCancelled => _cancellationSource.IsCancellationRequested;

		[JsonIgnore]
		public Task DownloadTask { get; private set; }

		private long _speedLimit = 0;
		/// <summary>bytes per second</summary>
		public long SpeedLimit { get => _speedLimit; set => _speedLimit = value <= 0 ? 0 : Math.Max(value, MIN_BYTES_PER_SECOND); }

		#endregion

		#region Private Properties
		private FileStream _writeFile { get; }
		private FileStream _readFile { get; }
		private CancellationTokenSource _cancellationSource { get; } = new();
		private EventWaitHandle _downloadedPiece { get; set; }

		#endregion

		#region Constants

		//Download buffer size
		private const int DOWNLOAD_BUFF_SZ = 32 * 1024;

		//NetworkFileStream will flush all data in _writeFile to disk after every
		//DATA_FLUSH_SZ bytes are written to the file stream.
		private const int DATA_FLUSH_SZ = 1024 * 1024;

		//Number of times per second the download rate is checkd and throttled
		private const int THROTTLE_FREQUENCY = 8;

		//Minimum throttle rate. The minimum amount of data that can be throttled
		//on each iteration of the download loop is DOWNLOAD_BUFF_SZ.
		public const int MIN_BYTES_PER_SECOND = DOWNLOAD_BUFF_SZ * THROTTLE_FREQUENCY;

		#endregion

		#region Constructor

		/// <summary> A resumable, simultaneous file downloader and reader. </summary>
		/// <param name="saveFilePath">Path to a location on disk to save the downloaded data from <paramref name="uri"/></param>
		/// <param name="uri">Http(s) address of the file to download.</param>
		/// <param name="writePosition">The position in <paramref name="uri"/> to begin downloading.</param>
		/// <param name="requestHeaders">Http headers to be sent to the server with the <see cref="HttpWebRequest"/>.</param>
		public NetworkFileStream(string saveFilePath, Uri uri, long writePosition = 0, Dictionary<string, string> requestHeaders = null)
		{
			SaveFilePath = ArgumentValidator.EnsureNotNullOrWhiteSpace(saveFilePath, nameof(saveFilePath));
			Uri = ArgumentValidator.EnsureNotNull(uri, nameof(uri));
			WritePosition = ArgumentValidator.EnsureGreaterThan(writePosition, nameof(writePosition), -1);

			if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
				throw new ArgumentException($"Specified {nameof(saveFilePath)} directory \"{Path.GetDirectoryName(saveFilePath)}\" does not exist.");

			RequestHeaders = requestHeaders ?? new();

			_writeFile = new FileStream(SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)
			{
				Position = WritePosition
			};

			_readFile = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			SetUriForSameFile(uri);
		}

		#endregion

		#region Downloader

		/// <summary> Update the <see cref="Dinah.Core.IO.JsonFilePersister{T}"/>. </summary>
		private void OnUpdate()
		{
			RequestHeaders["Range"] = $"bytes={WritePosition}-";
			try
			{
				Updated?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "An error was encountered while saving the download progress to JSON");
			}
		}

		/// <summary> Set a different <see cref="System.Uri"/> to the same file targeted by this instance of <see cref="NetworkFileStream"/> </summary>
		/// <param name="uriToSameFile">New <see cref="System.Uri"/> host must match existing host.</param>
		public void SetUriForSameFile(Uri uriToSameFile)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(uriToSameFile?.AbsoluteUri, nameof(uriToSameFile));

			if (uriToSameFile.Host != Uri.Host)
				throw new ArgumentException($"New uri to the same file must have the same host.\r\n Old Host :{Uri.Host}\r\nNew Host: {uriToSameFile.Host}");
			if (DownloadTask is not null)
				throw new InvalidOperationException("Cannot change Uri after download has started.");

			Uri = uriToSameFile;
			RequestHeaders["Range"] = $"bytes={WritePosition}-";
		}

		/// <summary> Begins downloading <see cref="Uri"/> to <see cref="SaveFilePath"/> in a background thread. </summary>
		/// <returns>The downloader <see cref="Task"/></returns>
		public async Task BeginDownloadingAsync()
		{
			if (ContentLength != 0 && WritePosition == ContentLength)
			{
				DownloadTask = Task.CompletedTask;
				return;
			}

			if (ContentLength != 0 && WritePosition > ContentLength)
				throw new WebException($"Specified write position (0x{WritePosition:X10}) is larger than  {nameof(ContentLength)} (0x{ContentLength:X10}).");

			var request = new HttpRequestMessage(HttpMethod.Get, Uri);

			foreach (var header in RequestHeaders)
				request.Headers.Add(header.Key, header.Value);

			var response = await new HttpClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationSource.Token);

			if (response.StatusCode != HttpStatusCode.PartialContent)
				throw new WebException($"Server at {Uri.Host} responded with unexpected status code: {response.StatusCode}.");

			//Content length is the length of the range request, and it is only equal
			//to the complete file length if requesting Range: bytes=0-
			if (WritePosition == 0)
				ContentLength = response.Content.Headers.ContentLength.GetValueOrDefault();

			var networkStream = await response.Content.ReadAsStreamAsync(_cancellationSource.Token);
			_downloadedPiece = new EventWaitHandle(false, EventResetMode.AutoReset);

			//Download the file in the background.

			DownloadTask = Task.Run(() => DownloadFile(networkStream), _cancellationSource.Token);
		}

		/// <summary> Download <see cref="Uri"/> to <see cref="SaveFilePath"/>.</summary>
		private async Task DownloadFile(Stream networkStream)
		{
			var downloadPosition = WritePosition;
			var nextFlush = downloadPosition + DATA_FLUSH_SZ;
			var buff = new byte[DOWNLOAD_BUFF_SZ];

			try
			{
				DateTime startTime = DateTime.Now;
				long bytesReadSinceThrottle = 0;
				int bytesRead;
				do
				{
					bytesRead = await networkStream.ReadAsync(buff, _cancellationSource.Token);
					await _writeFile.WriteAsync(buff, 0, bytesRead, _cancellationSource.Token);

					downloadPosition += bytesRead;

					if (downloadPosition > nextFlush)
					{
						await _writeFile.FlushAsync(_cancellationSource.Token);
						WritePosition = downloadPosition;
						OnUpdate();
						nextFlush = downloadPosition + DATA_FLUSH_SZ;
						_downloadedPiece.Set();
					}

					#region throttle

					bytesReadSinceThrottle += bytesRead;

					if (SpeedLimit >= MIN_BYTES_PER_SECOND && bytesReadSinceThrottle > SpeedLimit / THROTTLE_FREQUENCY)
					{
						var delayMS = (int)(startTime.AddSeconds(1d / THROTTLE_FREQUENCY) - DateTime.Now).TotalMilliseconds;
						if (delayMS > 0)
							await Task.Delay(delayMS, _cancellationSource.Token);

						startTime = DateTime.Now;
						bytesReadSinceThrottle = 0;
					}

					#endregion

				} while (downloadPosition < ContentLength && !IsCancelled && bytesRead > 0);

				WritePosition = downloadPosition;

				if (!IsCancelled && WritePosition < ContentLength)
					throw new WebException($"Downloaded size (0x{WritePosition:X10}) is less than {nameof(ContentLength)} (0x{ContentLength:X10}).");

				if (WritePosition > ContentLength)
					throw new WebException($"Downloaded size (0x{WritePosition:X10}) is greater than {nameof(ContentLength)} (0x{ContentLength:X10}).");
			}
			catch (TaskCanceledException)
			{
				Serilog.Log.Information("Download was cancelled");
			}
			finally
			{
				networkStream.Close();
				_writeFile.Close();
				_downloadedPiece.Set();
				OnUpdate();
			}
		}

		#endregion

		#region Download Stream Reader

		[JsonIgnore]
		public override bool CanRead => _readFile.CanRead;

		[JsonIgnore]
		public override bool CanSeek => _readFile.CanSeek;

		[JsonIgnore]
		public override bool CanWrite => false;

		[JsonIgnore]
		public override long Length
		{
			get
			{
				if (DownloadTask is null)
					throw new InvalidOperationException($"Background downloader must first be started by calling {nameof(BeginDownloadingAsync)}");
				return ContentLength;
			}
		}

		[JsonIgnore]
		public override long Position { get => _readFile.Position; set => Seek(value, SeekOrigin.Begin); }

		[JsonIgnore]
		public override bool CanTimeout => false;

		[JsonIgnore]
		public override int ReadTimeout { get => base.ReadTimeout; set => base.ReadTimeout = value; }

		[JsonIgnore]
		public override int WriteTimeout { get => base.WriteTimeout; set => base.WriteTimeout = value; }

		public override void Flush() => throw new InvalidOperationException();
		public override void SetLength(long value) => throw new InvalidOperationException();
		public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (DownloadTask is null)
				throw new InvalidOperationException($"Background downloader must first be started by calling {nameof(BeginDownloadingAsync)}");

			var toRead = Math.Min(count, Length - Position);
			WaitToPosition(Position + toRead);
			return IsCancelled ? 0 : _readFile.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			var newPosition = origin switch
			{
				SeekOrigin.Current => Position + offset,
				SeekOrigin.End => ContentLength + offset,
				_ => offset,
			};

			WaitToPosition(newPosition);
			return _readFile.Position = newPosition;
		}

		/// <summary>Blocks until the file has downloaded to at least <paramref name="requiredPosition"/>, then returns. </summary>
		/// <param name="requiredPosition">The minimum required flushed data length in <see cref="SaveFilePath"/>.</param>
		private void WaitToPosition(long requiredPosition)
		{
			while (WritePosition < requiredPosition
				&& DownloadTask?.IsCompleted is false
				&& !IsCancelled)
			{
				_downloadedPiece.WaitOne(50);
			}
		}

		private bool disposed = false;

		/*
		 * https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.dispose?view=net-7.0
		 * 
		 * In derived classes, do not override the Close() method, instead, put all of the
		 * Stream cleanup logic in the Dispose(Boolean) method.
		 */
		protected override void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				_cancellationSource.Cancel();
				DownloadTask?.GetAwaiter().GetResult();
				_downloadedPiece?.Dispose();
				_cancellationSource?.Dispose();
				_readFile.Dispose();
				_writeFile.Dispose();
				OnUpdate();
			}

			disposed = true;
			base.Dispose(disposing);
		}

		#endregion
	}
}
