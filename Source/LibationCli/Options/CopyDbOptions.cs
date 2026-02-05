using CommandLine;
using DataLayer;
using LibationFileManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
    [Verb("copydb", HelpText = "Copy the local sqlite database to postgres.")]
    public class CopyDbOptions : OptionsBase
	{
		[Option(shortName: 'c', longName: "connectionString", HelpText = "Postgres Database connection string")]
		public string? PostgresConnectionString { get; set; }
		protected override async Task ProcessAsync()
        {
            var srcConnectionString = SqliteStorage.ConnectionString;
            var destConnectionString = PostgresConnectionString ?? Configuration.Instance.PostgresqlConnectionString;
            if (string.IsNullOrEmpty(destConnectionString))
            {
                Console.Error.WriteLine("Postgres connection string is not set. Please provide it using --connectionString or set it in the configuration.");
                Environment.ExitCode = (int)ExitCode.RunTimeError;
                return;
            }

            Console.WriteLine("Copying database to Postgres...");
            Console.WriteLine("Source: " + srcConnectionString);
            Console.WriteLine("Destination: " + destConnectionString);
            Console.WriteLine();

            using var source = LibationContextFactory.CreateSqlite(srcConnectionString);
            using var destination = LibationContextFactory.CreatePostgres(destConnectionString);

            await source.Database.MigrateAsync();

            try
            {
                Console.WriteLine("Creating destination database...");
                await destination.Database.MigrateAsync();
                Console.WriteLine("Destination database recreated.");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error recreating destination database: {ex}");
                Environment.ExitCode = (int)ExitCode.RunTimeError;
                return;
            }

            try
            {
                // Load all data from source with all navigation properties
                // EF Core will track all relationships automatically
                Console.WriteLine("Loading data from source database...");

                var books = await source.Books
                    .Include(b => b.UserDefinedItem)
                    .Include(b => b.Supplements)
                    .ToListAsync();
                Console.WriteLine($"Loaded {books.Count} books");

                var libraryBooks = await source.LibraryBooks.ToListAsync();
                Console.WriteLine($"Loaded {libraryBooks.Count} library books");

                var contributors = await source.Contributors.ToListAsync();
                Console.WriteLine($"Loaded {contributors.Count} contributors");

                var series = await source.Series.ToListAsync();
                Console.WriteLine($"Loaded {series.Count} series");

                var categories = await source.Categories.ToListAsync();
                Console.WriteLine($"Loaded {categories.Count} categories");

                var categoryLadders = await source.CategoryLadders.ToListAsync();
                Console.WriteLine($"Loaded {categoryLadders.Count} category ladders");

                // Load junction tables explicitly
                var bookContributors = await source.Set<BookContributor>().ToListAsync();
                Console.WriteLine($"Loaded {bookContributors.Count} book-contributor links");

                var seriesBooks = await source.Set<SeriesBook>().ToListAsync();
                Console.WriteLine($"Loaded {seriesBooks.Count} series-book links");

                var bookCategories = await source.Set<BookCategory>().ToListAsync();
                Console.WriteLine($"Loaded {bookCategories.Count} book-category links");

                Console.WriteLine();
                Console.WriteLine("Copying data to destination database...");

                // Add everything to destination context
                // Order matters due to foreign keys: independent tables first
                destination.Contributors.AddRange(contributors.Where(c => !c.IsEmpty));
                destination.Series.AddRange(series);
                destination.Categories.AddRange(categories);
                destination.CategoryLadders.AddRange(categoryLadders);
                destination.Books.AddRange(books);
                destination.LibraryBooks.AddRange(libraryBooks);

                // Add junction tables
                destination.Set<BookContributor>().AddRange(bookContributors);
                destination.Set<SeriesBook>().AddRange(seriesBooks);
                destination.Set<BookCategory>().AddRange(bookCategories);

                // Save all changes
                await destination.SaveChangesAsync();

                Console.WriteLine("All data copied successfully.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error copying database: {ex}");
                Environment.ExitCode = (int)ExitCode.RunTimeError;
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Database copy completed successfully.");
        }
    }
}
