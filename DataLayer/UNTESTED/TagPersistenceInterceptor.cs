using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core.Collections.Generic;
using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataLayer
{
    internal class TagPersistenceInterceptor : IDbInterceptor
    {
        public void Executed(DbContext context) { }

        public void Executing(DbContext context)
		{
			// persist tags:
			var modifiedEntities = context
				.ChangeTracker
				.Entries()
				.Where(p => p.State.In(EntityState.Modified, EntityState.Added))
				.ToList();

			persistTags(modifiedEntities);
		}

		private static void persistTags(List<EntityEntry> modifiedEntities)
		{
			var tagsCollection = modifiedEntities
				.Select(e => e.Entity as UserDefinedItem)
				// filter by null but NOT by blank. blank is the valid way to show the absence of tags
				.Where(a => a != null)
				.Select(t => (t.Book.AudibleProductId, t.Tags))
				.ToList();
			FileManager.TagsPersistence.Save(tagsCollection);
		}
	}
}
