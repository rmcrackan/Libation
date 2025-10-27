using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;

namespace DataLayer
{
    public class LibationContextFactory
    {
        public static void ConfigureOptions(NpgsqlDbContextOptionsBuilder options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            options.MigrationsAssembly("DataLayer.Postgres");
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }

        public static LibationContext CreatePostgres(string connectionString)
        {
            var options = new DbContextOptionsBuilder<LibationContext>();

            options.UseNpgsql(connectionString, ConfigureOptions);

            return new LibationContext(options.Options);
        }

        public static LibationContext CreateSqlite(string connectionString)
        {
            var options = new DbContextOptionsBuilder<LibationContext>();

            options
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .UseSqlite(connectionString, options =>
                {
                    options.MigrationsAssembly("DataLayer.Sqlite");
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

            return new LibationContext(options.Options);
        }
    }
}
