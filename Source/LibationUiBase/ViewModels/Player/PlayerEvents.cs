using LibationUiBase.GridView;
using Prism.Events;

namespace LibationUiBase.ViewModels.Player
{
    public class BookAddedToPlaylist : PubSubEvent<ILibraryBookEntry>
    {
        public ILibraryBookEntry Book { get; }

        public BookAddedToPlaylist() {}

        public BookAddedToPlaylist(ILibraryBookEntry book)
        {
            Book = book;
        }
    }

    public class BookRemovedFromPlaylist : PubSubEvent<ILibraryBookEntry>
    {
        public ILibraryBookEntry Book { get; }

        public BookRemovedFromPlaylist() {}

        public BookRemovedFromPlaylist(ILibraryBookEntry book)
        {
            Book = book;
        }
    }
}
