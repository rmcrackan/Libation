using System;

namespace FileLiberator
{
    public interface IAudioDecodable : IStreamProcessable
    {
        event EventHandler<Action<byte[]>> RequestCoverArt;
        event EventHandler<string> TitleDiscovered;
        event EventHandler<string> AuthorsDiscovered;
        event EventHandler<string> NarratorsDiscovered;
        event EventHandler<byte[]> CoverImageDiscovered;
        void Cancel();
    }
}
