using LibationUiBase.GridView;
using Prism.Events;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationUiBase.ViewModels.Player;

public class PlayerViewModel : ViewModelBase
{
    private readonly IEventAggregator eventAggregator;
    private BindingList<PlaylistEntryViewModel> playlistItems = new();
    /// <summary>
    /// Public for data binding reasons.  Do NOT manipulate directly.
    /// </summary>
    public BindingList<PlaylistEntryViewModel> PlaylistItems
    {
        get => playlistItems;
        set => RaiseAndSetIfChanged(ref playlistItems, value);
    }

    private PlaylistEntryViewModel selectedBook;
    public PlaylistEntryViewModel SelectedBook
    {
        get => selectedBook;
        set => RaiseAndSetIfChanged(ref selectedBook, value);
    }

    public RelayCommand MoveUpCommand;
    public RelayCommand MoveDownCommand;

    public PlayerViewModel(IEventAggregator eventAggregator)
    {
        MoveUpCommand = new RelayCommand(_ =>
        {
            playlistItems.MoveUp(SelectedBook);
            RenumberPlaylist();
            var temp = SelectedBook;
            SelectedBook = null;
            SelectedBook = temp;
        }, _ => SelectedBook != null && playlistItems.IndexOf(SelectedBook) > 0 &&
                IsInPlaylist(SelectedBook));

        MoveDownCommand = new RelayCommand(_ =>
        {
            playlistItems.MoveDown(SelectedBook);
            RenumberPlaylist();
            var temp = SelectedBook;
            SelectedBook = null;
            SelectedBook = temp;
        }, _ => SelectedBook != null && playlistItems.IndexOf(SelectedBook) < playlistItems.Count - 1 &&
                IsInPlaylist(SelectedBook));

        this.eventAggregator = eventAggregator;
    }

    private void RenumberPlaylist()
    {
        for (var i = 0; i < playlistItems.Count; i++)
            playlistItems[i].Sequence = i + 1;
    }

    public async ValueTask AddToPlaylist(ILibraryBookEntry book)
    {
        var plevm = ServiceLocator.Get<PlaylistEntryViewModel>();
        await plevm.Init(book);
        plevm.Sequence = playlistItems.Count + 1;
        playlistItems.Add(plevm);
        //eventAggregator.GetEvent<BookAddedToPlaylist>().Publish(book);
    }

    public ValueTask RemoveFromPlaylist(ILibraryBookEntry book)
    {
        var plevm = playlistItems.FirstOrDefault(b => b.BookEntry.AudibleProductId == book.Book.AudibleProductId);
        if (plevm != null)
        {
            playlistItems.Remove(plevm);
            InvalidateCommands();
            //eventAggregator.GetEvent<BookRemovedFromPlaylist>().Publish(book);
        }

        return ValueTask.CompletedTask;
    }

    private void InvalidateCommands()
    {
        MoveDownCommand.RaiseCanExecuteChanged();
        MoveUpCommand.RaiseCanExecuteChanged();
    }

    public bool IsInPlaylist(ILibraryBookEntry book) =>
        PlaylistItems.Any(item => item.BookEntry.AudibleProductId == book.AudibleProductId);

    public bool IsInPlaylist(PlaylistEntryViewModel book) =>
        PlaylistItems.Any(item => item.BookEntry.AudibleProductId == book.BookEntry.AudibleProductId);


    public override string ToString() => $"{nameof(PlayerViewModel)}: {string.Join(", ", playlistItems)}";

    protected override void OnPropertyChanging(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(SelectedBook):
                if (SelectedBook != null)
                    SelectedBook.IsCurrent = false;
                InvalidateCommands();
                break;
        }
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(SelectedBook):
                if (SelectedBook != null)
                    SelectedBook.IsCurrent = true;
                InvalidateCommands();
                break;
        }
    }
}
