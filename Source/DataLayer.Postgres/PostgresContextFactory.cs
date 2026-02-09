using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer.Postgres;

public class PostgresContextFactory : IDesignTimeDbContextFactory<LibationContext>
{
	public LibationContext CreateDbContext(string[] args)
	{
		return LibationContextFactory.CreatePostgres(string.Empty);
	}
}
