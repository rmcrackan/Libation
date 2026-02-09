using DataLayer.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DataLayer;

public class LibationContext : DbContext, INotifyDisposed
{
	// IMPORTANT: USING DbSet<>
	// ========================
	// these run against the db. linq queries against these MUST be translatable to sql. primatives only. no POCOs or this error occurs:
	//     Unable to create a constant value of type 'DataLayer.Contributor'. Only primitive types or enumeration types are supported in this context.
	// to use full object-linq, load and use Local. HOWEVER, Local is only hashed/indexed on PK. All other searches are very slow
	// load full table:
	//     List<Contributor> contributors = ...;
	//     Contributors.Load();
	//     Contributors.Local.Where(a => contributors.Contains(a));
	// load only those in object:
	//     // overwrite collection
	//     Entry(product).Collection(x => x.Narrators).Load();
	//     product.Narrators = narrators;
	public DbSet<LibraryBook> LibraryBooks { get; private set; }
	public DbSet<Book> Books { get; private set; }
	public DbSet<Contributor> Contributors { get; private set; }
	public DbSet<Series> Series { get; private set; }
	public DbSet<Category> Categories { get; private set; }
	public DbSet<CategoryLadder> CategoryLadders { get; private set; }

	public event EventHandler? ObjectDisposed;
	public override void Dispose()
	{
		base.Dispose();
		ObjectDisposed?.Invoke(this, EventArgs.Empty);
	}
	public override async ValueTask DisposeAsync()
	{
		await base.DisposeAsync();
		ObjectDisposed?.Invoke(this, EventArgs.Empty);
	}

	// see DesignTimeDbContextFactoryBase for info about ctors and connection strings/OnConfiguring()
	public LibationContext(DbContextOptions options) : base(options) { }

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
		modelBuilder.ApplyConfiguration(new CategoryLadderConfig());
		modelBuilder.ApplyConfiguration(new BookCategoryConfig());

		// views are now supported via "keyless entity types" (instead of "entity types" or the prev "query types"):
		// https://docs.microsoft.com/en-us/ef/core/modeling/keyless-entity-types
	}
}
