using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using LibationFileManager;

namespace ApplicationServices
{
    public class BulkSetDownloadStatus
    {
        private List<(string message, LiberatedStatus newStatus, IEnumerable<Book> Books)> actionSets { get; } = new();

        public int Count => actionSets.Count;

        public IEnumerable<string> Messages => actionSets.Select(a => a.message);
        public string AggregateMessage => $"Are you sure you want to set {Messages.Aggregate((a, b) => $"{a} and {b}")}?";

        private List<LibraryBook> _libraryBooks;
        private bool _setDownloaded;
        private bool _setNotDownloaded;

        public BulkSetDownloadStatus(List<LibraryBook> libraryBooks, bool setDownloaded, bool setNotDownloaded)
        {
            _libraryBooks = libraryBooks;
            _setDownloaded = setDownloaded;
            _setNotDownloaded = setNotDownloaded;
        }

        public int Discover()
        {
            var bookExistsList = _libraryBooks
                .Select(libraryBook => new
                {
                    libraryBook.Book,
                    FileExists = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId) is not null
                })
                .ToList();

            if (_setDownloaded)
            {
                var books2change = bookExistsList
                    .Where(a => a.FileExists && a.Book.UserDefinedItem.BookStatus != LiberatedStatus.Liberated)
                    .Select(a => a.Book)
                    .ToList();

                if (books2change.Any())
                    actionSets.Add((
                        $"{"book".PluralizeWithCount(books2change.Count)} to 'Downloaded'",
                        LiberatedStatus.Liberated,
                        books2change));
            }

            if (_setNotDownloaded)
            {
                var books2change = bookExistsList
                    .Where(a => !a.FileExists && a.Book.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated)
                    .Select(a => a.Book)
                    .ToList();

                if (books2change.Any())
                    actionSets.Add((
                        $"{"book".PluralizeWithCount(books2change.Count)} to 'Not Downloaded'",
                        LiberatedStatus.NotLiberated,
                        books2change));
            }

            return Count;
        }

        public void Execute()
        {
            foreach (var a in actionSets)
                a.Books.UpdateBookStatus(a.newStatus);
        }
    }
}
