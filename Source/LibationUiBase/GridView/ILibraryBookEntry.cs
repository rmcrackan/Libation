namespace LibationUiBase.GridView
{
	public interface ILibraryBookEntry : IGridEntry
	{
		ISeriesEntry Parent { get; }
	}
}
