using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core.Collections.Generic;
using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    internal class TagPersistenceInterceptor : IDbInterceptor
    {
        public void Executed(DbContext context) { }

        public void Executing(DbContext context)
		{
			var tagsCollection
				= context
				.ChangeTracker
				.Entries()
				.Where(e => e.State.In(EntityState.Modified, EntityState.Added))
				.Select(e => e.Entity as UserDefinedItem)
				.Where(udi => udi != null)
				// do NOT filter out entires with blank tags. blank is the valid way to show the absence of tags
				.Select(t => (t.Book.AudibleProductId, t.Tags))
				.ToList();

			FileManager.TagsPersistence.Save(tagsCollection);
		}
	}
}
