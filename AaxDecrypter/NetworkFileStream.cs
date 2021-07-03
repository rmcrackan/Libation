using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    /// <summary>
    /// A <see cref="CookieContainer"/> for a single Uri.
    /// </summary>
    public class SingleUriCookieContainer : CookieContainer
    {
        public SingleUriCookieContainer(Uri uri)
        {
            Uri = uri;
        }
        public Uri Uri { get; }

        public CookieCollection GetCookies() => base.GetCookies(Uri);
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
        public Uri Uri { get; }

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

        private HttpWebRequest HttpRequest { get; }
        private FileStream _writeFile { get; }
        private FileStream _readFile { get; }
        private Stream _networkStream { get; set; }
        private bool hasBegunDownloading { get; set; }
        private bool isCancelled { get; set; }
        private Action cancelDownloadCallback { get; set; }

        #endregion

        #region Constants

        //Download buffer size
        private const int DOWNLOAD_BUFF_SZ = 4 * 1024;

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
            CookieContainer = cookies ?? new SingleUriCookieContainer(uri);

            HttpRequest = WebRequest.CreateHttp(uri);

            HttpRequest.CookieContainer = CookieContainer;
            HttpRequest.Headers = RequestHeaders;
            //If NetworkFileStream is resuming, Header will already contain a range.
            HttpRequest.Headers.Remove("Range");
            HttpRequest.AddRange(WritePosition);

            _writeFile = new FileStream(SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)
            {
                Position = WritePosition
            };

            _readFile = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        #endregion

        #region Downloader

        /// <summary>
        /// Update the <see cref="JsonFilePersister"/>.
        /// </summary>
        private void Update()
        {
            RequestHeaders = HttpRequest.Headers;
            Updated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Begins downloading <see cref="Uri"/> to <see cref="SaveFilePath"/> in a background thread.
        /// </summary>
        public async Task BeginDownloading()
        {
            if (ContentLength != 0 && WritePosition == ContentLength)
                return;

            if (ContentLength != 0 && WritePosition > ContentLength)
                throw new Exception($"Specified write position (0x{WritePosition:X10}) is larger than the file size.");

            var response = await HttpRequest.GetResponseAsync() as HttpWebResponse;

            if (response.StatusCode != HttpStatusCode.PartialContent)
                throw new Exception($"Server at {Uri.Host} responded with unexpected status code: {response.StatusCode}.");

            if (response.Headers.GetValues("Accept-Ranges").FirstOrDefault(r => r.EqualsInsensitive("bytes")) is null)
                throw new Exception($"Server at {Uri.Host} does not support Http ranges");

            //Content length is the length of the range request, and it is only equal
            //to the complete file length if requesting Range: bytes=0-
            if (WritePosition == 0)
                ContentLength = response.ContentLength;

            _networkStream = response.GetResponseStream();

            //Download the file in the background.
            Thread downloadThread = new Thread(() => DownloadFile());
            downloadThread.Start();

            while(!File.Exists(SaveFilePath) && new FileInfo(SaveFilePath).Length > 1000)
            {
                Thread.Sleep(100);
            }

            hasBegunDownloading = true;
            return;
        }

        /// <summary>
        /// Downlod <see cref="Uri"/> to <see cref="SaveFilePath"/>.
        /// </summary>
        private void DownloadFile()
        {
            long downloadPosition = WritePosition;
            long nextFlush = downloadPosition + DATA_FLUSH_SZ;

            byte[] buff = new byte[DOWNLOAD_BUFF_SZ];
            do
            {
                int bytesRead = _networkStream.Read(buff, 0, DOWNLOAD_BUFF_SZ);
                _writeFile.Write(buff, 0, bytesRead);

                downloadPosition += bytesRead;

                if (downloadPosition > nextFlush)
                {
                    _writeFile.Flush();
                    WritePosition = downloadPosition;
                    Update();

                    nextFlush = downloadPosition + DATA_FLUSH_SZ;
                }

            } while (downloadPosition < ContentLength && !isCancelled);

            _writeFile.Close();
            WritePosition = downloadPosition;
            Update();
            _networkStream.Close();

            if (!isCancelled && WritePosition < ContentLength)
                throw new Exception("File download ended before finishing.");

            if (WritePosition > ContentLength)
                throw new Exception("Downloaded file is larger than expected.");

            cancelDownloadCallback?.Invoke();
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

                var result = new SingleUriCookieContainer(new Uri(jObj["Uri"].Value<string>()))
                {
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
                {
                    result.Add(kvp.Key, kvp.Value.Value<string>());
                }

                return result;
            }

            public override bool CanWrite => true;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JObject jObj = new JObject();
                Type type = value.GetType();
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
        public override long Length => ContentLength;

        [JsonIgnore]
        public override long Position { get => _readFile.Position; set => Seek(value, SeekOrigin.Begin); }

        [JsonIgnore]
        public override bool CanTimeout => base.CanTimeout;

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
                throw new Exception($"Must call {nameof(BeginDownloading)} before attempting to read {nameof(NetworkFileStream)};");

            //read operation will block until file contains enough data
            //to fulfil the request.
            return _readFile.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!hasBegunDownloading)
                throw new Exception($"Must call {nameof(BeginDownloading)} before attempting to read {nameof(NetworkFileStream)};");

            //read operation will block until file contains enough data
            //to fulfil the request.
            return await _readFile.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;

            switch (origin)
            {
                case SeekOrigin.Current:
                    newPosition = Position + offset;
                    break;
                case SeekOrigin.End:
                    newPosition = ContentLength + offset;
                    break;
                default:
                    newPosition = offset;
                    break;
            }

            ReadToPosition(newPosition);

            _readFile.Position = newPosition;
            return newPosition;
        }

        /// <summary>
        /// Ensures that the file has downloaded to at least <paramref name="neededPosition"/>, then returns.
        /// </summary>
        /// <param name="neededPosition">The minimum required data length in <see cref="SaveFilePath"/>.</param>
        private void ReadToPosition(long neededPosition)
        {
            long totalBytesRead = _readFile.Position;

            byte[] buff = new byte[DOWNLOAD_BUFF_SZ];
            do
            {
                totalBytesRead += Read(buff, 0, DOWNLOAD_BUFF_SZ);
            } while (totalBytesRead < neededPosition);
        }
        public override void Close()
        {
            isCancelled = true;
            cancelDownloadCallback = () =>
            {
                _readFile.Close();
                _writeFile.Close();
                _networkStream?.Close();
                Update();
            };
        }

        #endregion
    }
}
