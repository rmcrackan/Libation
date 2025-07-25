using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DataLayer
{
    public class LibationContextFactory : DesignTimeDbContextFactoryBase<LibationContext>
    {
        protected override LibationContext CreateNewInstance(DbContextOptions<LibationContext> options) => new LibationContext(options);
        protected override void UseDatabaseEngine(DbContextOptionsBuilder optionsBuilder, string connectionString)
            => optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .UseSqlite(connectionString, ob => ob.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
    }
}
