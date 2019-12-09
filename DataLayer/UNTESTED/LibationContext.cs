using DataLayer.Configurations;
using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class LibationContext : InterceptableDbContext
    {
        // IMPORTANT: USING DbSet<>
        // ========================
        // these run against the db. linq queries against these MUST be translatable to sql. primatives only. no POCOs or this error occurs:
        //     Unable to create a constant value of type 'DataLayer.Contributor'. Only primitive types or enumeration types are supported in this context.
        // to use full object-linq, load and use local
        // load full table:
        //     List<Contributor> contributors = ...;
        //     Contributors.Load();
        //     Contributors.Local.Where(a => contributors.Contains(a));
        // load only those in object:
        //     // overwrite collection
        //     Entry(product).Collection(x => x.Narrators).Load();
        //     product.Narrators = narrators;
        public DbSet<LibraryBook> Library { get; private set; }
        public DbSet<Book> Books { get; private set; }
        public DbSet<Contributor> Contributors { get; private set; }
        public DbSet<Series> Series { get; private set; }
        public DbSet<Category> Categories { get; private set; }

        public static LibationContext Create(string connectionString)
        {
            var factory = new LibationContextFactory();
            var context = factory.Create(connectionString);
            return context;
        }

        // see DesignTimeDbContextFactoryBase for info about ctors and connection strings/OnConfiguring()
        internal LibationContext(DbContextOptions options) : base(options) { }

        // called on each instantiation
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AddInterceptor(new TagPersistenceInterceptor());

            base.OnConfiguring(optionsBuilder);
        }

        // typically only called once per execution; NOT once per instantiation
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BookConfig());
            modelBuilder.ApplyConfiguration(new ContributorConfig());
            modelBuilder.ApplyConfiguration(new BookContributorConfig());
            modelBuilder.ApplyConfiguration(new LibraryBookConfig());
            modelBuilder.ApplyConfiguration(new SeriesConfig());
            modelBuilder.ApplyConfiguration(new SeriesBookConfig());
            modelBuilder.ApplyConfiguration(new CategoryConfig());

			// seeds go here. examples in scratch pad
			modelBuilder
				.Entity<Category>()
				.HasData(Category.GetEmpty());
			modelBuilder
				.Entity<Contributor>()
				.HasData(Contributor.GetEmpty());

			// views are now supported via "keyless entity types" (instead of "entity types" or the prev "query types"):
			// https://docs.microsoft.com/en-us/ef/core/modeling/keyless-entity-types
		}
	}
}
