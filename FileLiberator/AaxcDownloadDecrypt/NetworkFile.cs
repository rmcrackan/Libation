using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileLiberator.AaxcDownloadDecrypt
{
    /// <summary>
    /// Provides a <see cref="TagLib.File.IFileAbstraction"/> for a file over Http.
    /// </summary>
    class NetworkFileAbstraction : TagLib.File.IFileAbstraction
    {
        private NetworkFileStream aaxNetworkStream;

        public static async Task<NetworkFileAbstraction> CreateAsync(HttpClient client, Uri webFileUri)
        {
            var response = await client.GetAsync(webFileUri, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Can't read file from client.");

            var contentLength = response.Content.Headers.ContentLength ?? 0;

            var networkStream = await response.Content.ReadAsStreamAsync();

            var networkFile = new NetworkFileAbstraction(Path.GetFileName(webFileUri.LocalPath), networkStream, contentLength);

            return networkFile;
        }

        private NetworkFileAbstraction(string fileName, Stream netStream, long contentLength)
        {
            Name = fileName;
            aaxNetworkStream = new NetworkFileStream(netStream, contentLength);
        }
        public string Name { get; private set; }

        public Stream ReadStream => aaxNetworkStream;

        public Stream WriteStream => throw new NotImplementedException();

        public void CloseStream(Stream stream)
        {
            aaxNetworkStream.Close();
        }

        private class NetworkFileStream : Stream
        {
            private const int BUFF_SZ = 2 * 1024;

            private FileStream _fileBacker;

            private Stream _networkStream;

            private long networkBytesRead = 0;

            private long _contentLength;
            public NetworkFileStream(Stream netStream, long contentLength)
            {
                _networkStream = netStream;
                _contentLength = contentLength;
                _fileBacker = File.Create(Path.GetTempFileName(), BUFF_SZ, FileOptions.DeleteOnClose);
            }
            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite => false;

            public override long Length => _contentLength;

            public override long Position { get => _fileBacker.Position; set => Seek(value, 0); }

            public override void Flush()
            {
                throw new NotImplementedException();
            }
            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                long requiredLength = Position + offset + count;

                if (requiredLength > networkBytesRead)
                    readWebFileToPosition(requiredLength);

                return _fileBacker.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long newPosition = (long)origin + offset;

                if (newPosition > networkBytesRead)
                    readWebFileToPosition(newPosition);

                _fileBacker.Position = newPosition;
                return newPosition;
            }

            public override void Close()
            {
                _fileBacker.Close();
                _networkStream.Close();
            }
            /// <summary>
            /// Read more data from <see cref="_networkStream"/> into <see cref="_fileBacker"/> as needed. 
            /// </summary>
            /// <param name="requiredLength"></param>
            private void readWebFileToPosition(long requiredLength)
            {
                byte[] buff = new byte[BUFF_SZ];

                long backerPosition = _fileBacker.Position;

                _fileBacker.Position = networkBytesRead;

                while (networkBytesRead < requiredLength)
                {
                    int bytesRead = _networkStream.Read(buff, 0, BUFF_SZ);
                    _fileBacker.Write(buff, 0, bytesRead);
                    networkBytesRead += bytesRead;
                }

                _fileBacker.Position = backerPosition;
            }
        }
    }
}
