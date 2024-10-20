using ReactiveUI;
using System.Collections.ObjectModel;

namespace LibationAvalonia.ViewModels.Player;

public class PlayerViewModel : ViewModelBase
{
    private ObservableCollection<PlaylistEntryViewModel> items = new();
    public ObservableCollection<PlaylistEntryViewModel> Items
    {
        get => items;
        set => this.RaiseAndSetIfChanged(ref items, value);
    }


}