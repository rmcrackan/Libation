using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public static class RemoveOrphansCommand
    {
        public static int RemoveOrphans(this LibationContext context)
            => context.Database.ExecuteSqlCommand(@"
                delete c
                from Contributors c
                left join BookContributor bc on c.ContributorId = bc.ContributorId
                left join Books b on bc.BookId = b.BookId
                where bc.ContributorId is null
                ");
    }
}
