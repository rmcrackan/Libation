using DataLayer;

namespace ApplicationServices
{
	public static class TagUpdater
	{
		public static int IndexChangedTags(Book book)
		{
			// update disconnected entity
			using var context = LibationContext.Create();
			context.Update(book);
			var qtyChanges = context.SaveChanges();

			// this part is tags-specific
			if (qtyChanges > 0)
				SearchEngineActions.UpdateBookTags(book);

			return qtyChanges;
		}
	}
}
