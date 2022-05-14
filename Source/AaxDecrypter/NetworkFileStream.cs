using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace AaxDecrypter
{
	/// <summary>
	/// A <see cref="CookieContainer"/> for a single Uri.
	/// </summary>
	public class SingleUriCookieContainer : CookieContainer
	{
		private Uri baseAddress;
		public Uri Uri
		{
			get => baseAddress;
			set
			{
				baseAddress = new UriBuilder(value.Scheme, value.Host).Uri;
			}
		}

		public CookieCollection GetCookies()
		{
			return GetCookies(Uri);
		}
	}

	/// <summary>
	/// A resumable, simultaneous file downloader and reader.
	/// </summary>
	public class NetworkFileStream : Stream, IUpdatable
	{
		public event EventHandler Updated;

		#region Public Properties

		/// <summary>
		/// Location to save the downloaded data.
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public string SaveFilePath { get; }

		/// <summary>
		/// Http(s) address of the file to download.
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public Uri Uri { get; private set; }

		/// <summary>
		/// All cookies set by caller or by the remote server.
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public SingleUriCookieContainer CookieContainer { get; }

		/// <summary>
		/// Http headers to be sent to the server with the request.
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public WebHeaderCollection RequestHeaders { get; private set; }

		/// <summary>
		/// The position in <see cref="SaveFilePath"/> that has been written and flushed to disk.
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public long WritePosition { get; private set; }

		/// <summary>
		/// The total length of the <see cref="Uri"/> file to download.
		/// </summary>
		[JsonProperty(Required = Required.Always)]
		public long ContentLength { get; private set; }

		#endregion

		#region Private Properties
		private HttpWebRequest HttpRequest { get; set; }
		private FileStream _writeFile { get; }
		private FileStream _readFile { get; }
		private Stream _networkStream { get; set; }
		private bool hasBegunDownloading { get; set; }
		public bool IsCancelled { get; private set; }
		private EventWaitHandle downloadEnded { get; set; }
		private EventWaitHandle downloadedPiece { get; set; }

		#endregion

		#region Constants

		//Download buffer size
		private const int DOWNLOAD_BUFF_SZ = 32 * 1024;

		//NetworkFileStream will flush all data in _writeFile to disk after every
		//DATA_FLUSH_SZ bytes are written to the file stream.
		private const int DATA_FLUSH_SZ = 1024 * 1024;

		#endregion

		#region Constructor

		/// <summary>
		/// A resumable, simultaneous file downloader and reader.
		/// </summary>
		/// <param name="saveFilePath">Path to a location on disk to save the downloaded data from <paramref name="uri"/></param>
		/// <param name="uri">Http(s) address of the file to download.</param>
		/// <param name="writePosition">The position in <paramref name="uri"/> to begin downloading.</param>
		/// <param name="requestHeaders">Http headers to be sent to the server with the <see cref="HttpWebRequest"/>.</param>
		/// <param name="cookies">A <see cref="SingleUriCookieContainer"/> with cookies to send with the <see cref="HttpWebRequest"/>. It will also be populated with any cookies set by the server. </param>
		public NetworkFileStream(string saveFilePath, Uri uri, long writePosition = 0, WebHeaderCollection requestHeaders = null, SingleUriCookieContainer cookies = null)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(saveFilePath, nameof(saveFilePath));
			ArgumentValidator.EnsureNotNullOrWhiteSpace(uri?.AbsoluteUri, nameof(uri));
			ArgumentValidator.EnsureGreaterThan(writePosition, nameof(writePosition), -1);

			if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
				throw new ArgumentException($"Specified {nameof(saveFilePath)} directory \"{Path.GetDirectoryName(saveFilePath)}\" does not exist.");

			SaveFilePath = saveFilePath;
			Uri = uri;
			WritePosition = writePosition;
			RequestHeaders = requestHeaders ?? new WebHeaderCollection();
			CookieContainer = cookies ?? new SingleUriCookieContainer { Uri = uri };

			_writeFile = new FileStream(SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)
			{
				Position = WritePosition
			};

			_readFile = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			SetUriForSameFile(uri);
		}

		#endregion

		#region Downloader

		/// <summary>
		/// Update the <see cref="JsonFilePersister"/>.
		/// </summary>
		private void Update()
		{
			RequestHeaders = HttpRequest.Headers;
			Updated?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Set a different <see cref="System.Uri"/> to the same file targeted by this instance of <see cref="NetworkFileStream"/>
		/// </summary>
		/// <param name="uriToSameFile">New <see cref="System.Uri"/> host must match existing host.</param>
		public void SetUriForSameFile(Uri uriToSameFile)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(uriToSameFile?.AbsoluteUri, nameof(uriToSameFile));

			if (uriToSameFile.Host != Uri.Host)
				throw new ArgumentException($"New uri to the same file must have the same host.\r\n Old Host :{Uri.Host}\r\nNew Host: {uriToSameFile.Host}");
			if (hasBegunDownloading)
				throw new InvalidOperationException("Cannot change Uri after download has started.");

			Uri = uriToSameFile;
			HttpRequest = WebRequest.CreateHttp(Uri);

			HttpRequest.CookieContainer = CookieContainer;
			HttpRequest.Headers = RequestHeaders;
			//If NetworkFileStream is resuming, Header will already contain a range.
			HttpRequest.Headers.Remove("Range");
			HttpRequest.AddRange(WritePosition);
		}

		/// <summary>
		/// Begins downloading <see cref="Uri"/> to <see cref="SaveFilePath"/> in a background thread.
		/// </summary>
		private void BeginDownloading()
		{
			downloadEnded = new EventWaitHandle(false, EventResetMode.ManualReset);

			if (ContentLength != 0 && WritePosition == ContentLength)
			{
				hasBegunDownloading = true;
				downloadEnded.Set();
				return;
			}

			if (ContentLength != 0 && WritePosition > ContentLength)
				throw new WebException($"Specified write position (0x{WritePosition:X10}) is larger than  {nameof(ContentLength)} (0x{ContentLength:X10}).");

			var response = HttpRequest.GetResponse() as HttpWebResponse;

			if (response.StatusCode != HttpStatusCode.PartialContent)
				throw new WebException($"Server at {Uri.Host} responded with unexpected status code: {response.StatusCode}.");

			//Content length is the length of the range request, and it is only equal
			//to the complete file length if requesting Range: bytes=0-
			if (WritePosition == 0)
				ContentLength = response.ContentLength;

			_networkStream = response.GetResponseStream();
			downloadedPiece = new EventWaitHandle(false, EventResetMode.AutoReset);

			//Download the file in the background.
			new Thread(() => DownloadFile())
			{ IsBackground = true }
			.Start();

			hasBegunDownloading = true;
			return;
		}

		/// <summary>
		/// Downlod <see cref="Uri"/> to <see cref="SaveFilePath"/>.
		/// </summary>
		private void DownloadFile()
		{
			var downloadPosition = WritePosition;
			var nextFlush = downloadPosition + DATA_FLUSH_SZ;

			var buff = new byte[DOWNLOAD_BUFF_SZ];
			do
			{
				Thread.Sleep(10);
				var bytesRead = _networkStream.Read(buff, 0, DOWNLOAD_BUFF_SZ);
				_writeFile.Write(buff, 0, bytesRead);

				downloadPosition += bytesRead;

				if (downloadPosition > nextFlush)
				{
					_writeFile.Flush();
					WritePosition = downloadPosition;
					Update();
					nextFlush = downloadPosition + DATA_FLUSH_SZ;
					downloadedPiece.Set();
				}

			} while (downloadPosition < ContentLength && !IsCancelled);

			_writeFile.Close();
			_networkStream.Close();
			WritePosition = downloadPosition;
			Update();

			downloadedPiece.Set();
			downloadEnded.Set();

			if (!IsCancelled && WritePosition < ContentLength)
				throw new WebException($"Downloaded size (0x{WritePosition:X10}) is less than {nameof(ContentLength)} (0x{ContentLength:X10}).");

			if (WritePosition > ContentLength)
				throw new WebException($"Downloaded size (0x{WritePosition:X10}) is greater than {nameof(ContentLength)} (0x{ContentLength:X10}).");

		}

		#endregion

		#region Json Connverters

		public static JsonSerializerSettings GetJsonSerializerSettings()
		{
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new CookieContainerConverter());
			settings.Converters.Add(new WebHeaderCollectionConverter());
			return settings;
		}

		internal class CookieContainerConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType)
				=> objectType == typeof(SingleUriCookieContainer);

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				var jObj = JObject.Load(reader);

				var result = new SingleUriCookieContainer()
				{
					Uri = new Uri(jObj["Uri"].Value<string>()),
					Capacity = jObj["Capacity"].Value<int>(),
					MaxCookieSize = jObj["MaxCookieSize"].Value<int>(),
					PerDomainCapacity = jObj["PerDomainCapacity"].Value<int>()
				};

				var cookieList = jObj["Cookies"].ToList();

				foreach (var cookie in cookieList)
				{
					result.Add(
						new Cookie
						{
							Comment = cookie["Comment"].Value<string>(),
							HttpOnly = cookie["HttpOnly"].Value<bool>(),
							Discard = cookie["Discard"].Value<bool>(),
							Domain = cookie["Domain"].Value<string>(),
							Expired = cookie["Expired"].Value<bool>(),
							Expires = cookie["Expires"].Value<DateTime>(),
							Name = cookie["Name"].Value<string>(),
							Path = cookie["Path"].Value<string>(),
							Port = cookie["Port"].Value<string>(),
							Secure = cookie["Secure"].Value<bool>(),
							Value = cookie["Value"].Value<string>(),
							Version = cookie["Version"].Value<int>(),
						});
				}

				return result;
			}

			public override bool CanWrite => true;

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var cookies = value as SingleUriCookieContainer;
				var obj = (JObject)JToken.FromObject(value);
				var container = cookies.GetCookies();
				var propertyNames = container.Select(c => JToken.FromObject(c));
				obj.AddFirst(new JProperty("Cookies", new JArray(propertyNames)));
				obj.WriteTo(writer);
			}
		}

		internal class WebHeaderCollectionConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType)
				=> objectType == typeof(WebHeaderCollection);

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				var jObj = JObject.Load(reader);
				var result = new WebHeaderCollection();

				foreach (var kvp in jObj)
					result.Add(kvp.Key, kvp.Value.Value<string>());

				return result;
			}

			public override bool CanWrite => true;

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				var jObj = new JObject();
				var type = value.GetType();
				var headers = value as WebHeaderCollection;
				var jHeaders = headers.AllKeys.Select(k => new JProperty(k, headers[k]));
				jObj.Add(jHeaders);
				jObj.WriteTo(writer);
			}
		}

		#endregion

		#region Download Stream Reader

		[JsonIgnore]
		public override bool CanRead => true;

		[JsonIgnore]
		public override bool CanSeek => true;

		[JsonIgnore]
		public override bool CanWrite => false;

		[JsonIgnore]
		public override long Length
		{
			get
			{
				if (!hasBegunDownloading)
					BeginDownloading();
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

		public override void Flush() => throw new NotImplementedException();
		public override void SetLength(long value) => throw new NotImplementedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!hasBegunDownloading)
				BeginDownloading();

			var toRead = Math.Min(count, Length - Position);
			WaitToPosition(Position + toRead);
			return _readFile.Read(buffer, offset, count);
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

		/// <summary>
		/// Blocks until the file has downloaded to at least <paramref name="requiredPosition"/>, then returns.
		/// </summary>
		/// <param name="requiredPosition">The minimum required flished data length in <see cref="SaveFilePath"/>.</param>
		private void WaitToPosition(long requiredPosition)
		{
			while (requiredPosition > WritePosition && !IsCancelled && hasBegunDownloading && !downloadedPiece.WaitOne(1000)) ;
		}

		public override void Close()
		{
			IsCancelled = true;

			while (downloadEnded is not null && !downloadEnded.WaitOne(1000)) ;

			_readFile.Close();
			_writeFile.Close();
			_networkStream?.Close();
			Update();
		}

		#endregion
		~NetworkFileStream()
		{
			downloadEnded?.Close();
			downloadedPiece?.Close();
		}
	}
}
