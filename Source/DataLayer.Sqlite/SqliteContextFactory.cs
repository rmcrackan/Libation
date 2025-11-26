using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer.Sqlite
{
    public class SqliteContextFactory : IDesignTimeDbContextFactory<LibationContext>
    {
        public LibationContext CreateDbContext(string[] args)
        {
            return LibationContextFactory.CreateSqlite(string.Empty);
        }
    }
}
