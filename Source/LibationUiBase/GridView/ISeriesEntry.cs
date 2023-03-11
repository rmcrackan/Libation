using System.Collections.Generic;

namespace LibationUiBase.GridView
{
	public interface ISeriesEntry : IGridEntry
	{
		List<ILibraryBookEntry> Children { get; }
		void ChildRemoveUpdate();
		void RemoveChild(ILibraryBookEntry libraryBookEntry);
	}
}
