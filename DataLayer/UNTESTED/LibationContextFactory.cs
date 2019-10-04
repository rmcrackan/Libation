using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class LibationContextFactory : DesignTimeDbContextFactoryBase<LibationContext>
    {
        protected override LibationContext CreateNewInstance(DbContextOptions<LibationContext> options) => new LibationContext(options);
        protected override void UseDatabaseEngine(DbContextOptionsBuilder optionsBuilder, string connectionString) => optionsBuilder.UseSqlServer(connectionString);
    }
}
