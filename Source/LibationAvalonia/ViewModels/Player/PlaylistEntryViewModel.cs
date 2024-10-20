using ReactiveUI;

namespace LibationAvalonia.ViewModels.Player;

public class PlaylistEntryViewModel : ViewModelBase
{
    private int sequence;
    public int Sequence
    {
        get => sequence;
        set => this.RaiseAndSetIfChanged(ref sequence, value);
    }

    private string seriesName = string.Empty;
    public string SeriesName
    {
        get => seriesName;
        set => this.RaiseAndSetIfChanged(ref seriesName, value);
    }

    private string title = string.Empty;
    public string Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }
}