﻿using System;

namespace FileLiberator
{
    public interface IDecryptable : IProcessable
    {
        event EventHandler<string> DecryptBegin;

        event EventHandler<Action<byte[]>> RequestCoverArt;
        event EventHandler<string> TitleDiscovered;
        event EventHandler<string> AuthorsDiscovered;
        event EventHandler<string> NarratorsDiscovered;
        event EventHandler<byte[]> CoverImageFilepathDiscovered;
        event EventHandler<int> UpdateProgress;
        event EventHandler<TimeSpan> UpdateRemainingTime;

        event EventHandler<string> DecryptCompleted;
        void Cancel();
    }
}
