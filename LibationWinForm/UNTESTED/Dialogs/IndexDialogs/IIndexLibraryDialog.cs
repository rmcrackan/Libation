namespace LibationWinForm
{
    public interface IIndexLibraryDialog : IRunnableDialog
    {
        int TotalBooksProcessed { get; }
        int NewBooksAdded { get; }
    }
}
