using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    /// <summary>
    /// Provides a <see cref="TagLib.File.IFileAbstraction"/> for a file over Http.
    /// </summary>
    class NetworkFileAbstraction : TagLib.File.IFileAbstraction
    {
        private NetworkFileStream aaxNetworkStream;

        public NetworkFileAbstraction( NetworkFileStream networkFileStream)
        {
            Name = networkFileStream.SaveFilePath;
            aaxNetworkStream = networkFileStream;
        }
        public string Name { get; private set; }

        public Stream ReadStream => aaxNetworkStream;

        public Stream WriteStream => throw new NotImplementedException();

        public void CloseStream(Stream stream)
        {
            aaxNetworkStream.Close();
        }
    }
}
