using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer.Postgres
{
    public class SqliteContextFactory : IDesignTimeDbContextFactory<LibationContext>
    {
        public LibationContext CreateDbContext(string[] args)
        {
            return LibationContextFactory.CreateSqlite(string.Empty);
        }
    }
}
