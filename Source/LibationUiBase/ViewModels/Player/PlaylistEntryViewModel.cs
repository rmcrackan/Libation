using LibationUiBase.GridView;
using System.Threading.Tasks;

namespace LibationUiBase.ViewModels.Player;

public class PlaylistEntryViewModel : ViewModelBase
{
    const string CurrentIndicator = "▶";

    private int sequence;
    public int Sequence
    {
        get => sequence;
        set => RaiseAndSetIfChanged(ref sequence, value);
    }

    private string seriesName = string.Empty;
    public string SeriesName
    {
        get => seriesName;
        set => RaiseAndSetIfChanged(ref seriesName, value);
    }

    public string Title => bookEntry?.Title;
    public string Series => bookEntry?.Series;

    private bool isCurrent;
    public bool IsCurrent
    {
        get => isCurrent;
        set => this.RaiseAndSetIfChanged(ref isCurrent, value);
    }

    // "►" indicator for winforms
    private string isCurrentStr = string.Empty;
    public string IsCurrentStr
    {
        get => isCurrentStr;
        set => this.RaiseAndSetIfChanged(ref isCurrentStr, value);
    }

    private ILibraryBookEntry bookEntry;
    public ILibraryBookEntry BookEntry
    {
        get => bookEntry;
        set => RaiseAndSetIfChanged(ref bookEntry, value);
    }

    public async ValueTask Init(ILibraryBookEntry bookEntry)
    {
        BookEntry = bookEntry;
    }

    public override string ToString() => Title;

    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(IsCurrent):
                IsCurrentStr = IsCurrent ? CurrentIndicator : null;
                break;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is PlaylistEntryViewModel plevm && bookEntry != null)
            return bookEntry?.AudibleProductId == plevm?.bookEntry.AudibleProductId;
        return false;
    }
}