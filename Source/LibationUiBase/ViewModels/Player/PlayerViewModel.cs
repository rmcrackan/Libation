using LibationUiBase.GridView;
using Prism.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationUiBase.ViewModels.Player;

public class PlayerViewModel : ViewModelBase
{
    const string CurrentIndicator = "▶";

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
        }, _ => SelectedBook != null && playlistItems.IndexOf(SelectedBook) > 0);

        MoveDownCommand = new RelayCommand(_ =>
        {
            playlistItems.MoveDown(SelectedBook);
            RenumberPlaylist();
        }, _ => SelectedBook != null && playlistItems.IndexOf(SelectedBook) < playlistItems.Count - 1);

        this.eventAggregator = eventAggregator;
    }

    private void RenumberPlaylist()
    {
        for (var i = 0; playlistItems.Count > 0; i++) 
            playlistItems[i].Sequence = i + 1;
    }

    public async ValueTask AddToPlaylist(ILibraryBookEntry book)
    {
        var plevm = ServiceLocator.Get<PlaylistEntryViewModel>();
        await plevm.Init(book);
        plevm.Sequence = playlistItems.Count + 1;
        playlistItems.Add(plevm);
        eventAggregator.GetEvent<BookAddedToPlaylist>().Publish(book);
    }

    public ValueTask RemoveFromPlaylist(ILibraryBookEntry book)
    {
        var plevm = playlistItems.FirstOrDefault(b => b.BookEntry.AudibleProductId == book.Book.AudibleProductId);
        if (plevm != null)
        {
            playlistItems.Remove(plevm);
            eventAggregator.GetEvent<BookRemovedFromPlaylist>().Publish(book);
        }

        return ValueTask.CompletedTask;
    }

    public bool IsInPlaylist(ILibraryBookEntry book) =>
        PlaylistItems.Any(item => item.BookEntry.AudibleProductId == book.AudibleProductId);

    public override string ToString() =>
        string.Join(", ", playlistItems);

    protected override void OnPropertyChanging(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(SelectedBook):
                SelectedBook.IsCurrent = false;
                SelectedBook.IsCurrentStr = null;
                MoveUpCommand.RaiseCanExecuteChanged();
                break;
        }
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(SelectedBook):
                SelectedBook.IsCurrent = true;
                SelectedBook.IsCurrentStr = CurrentIndicator;
                break;
        }
    }
}