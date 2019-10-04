using System;
using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _scratch_pad
{
    ////// to use this as a console, open properties and change from class library => console
    //// DON'T FORGET TO REVERT IT
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        var user = new Student() { Name = "Dinah Cheshire" };
    //        var udi = new UserDef { UserDefId = 1, TagsRaw = "my,tags" };

    //        using (var context = new MyTestContextDesignTimeDbContextFactory().Create())
    //        {
    //            context.Add(user);
    //            //context.Add(udi);
    //            context.Update(udi);
    //            context.SaveChanges();
    //        }

    //        Console.WriteLine($"Student was saved in the database with id: {user.Id}");
    //    }
    //}

    public class MyTestContextDesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<MyTestContext>
    {
        protected override MyTestContext CreateNewInstance(DbContextOptions<MyTestContext> options) => new MyTestContext(options);
        protected override void UseDatabaseEngine(DbContextOptionsBuilder optionsBuilder, string connectionString) => optionsBuilder.UseSqlite(connectionString);
    }

    public class MyTestContext : DbContext
    {
        // see DesignTimeDbContextFactoryBase for info about ctors and connection strings/OnConfiguring()
        public MyTestContext(DbContextOptions<MyTestContext> options) : base(options) { }

        #region classes for OnModelCreating() seed example
        class Blog
        {
            public int BlogId { get; set; }
            public string Url { get; set; }
            public System.Collections.Generic.ICollection<Post> Posts { get; set; }
        }
        class Post
        {
            public int PostId { get; set; }
            public string Content { get; set; }
            public string Title { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
            public Name AuthorName { get; set; }
        }
        class Name
        {
            public string First { get; set; }
            public string Last { get; set; }
        }
        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // config
            modelBuilder.Entity<Blog>(entity => entity.Property(e => e.Url).IsRequired());
            modelBuilder.Entity<Order>().OwnsOne(p => p.OrderDetails, cb =>
            {
                cb.OwnsOne(c => c.BillingAddress);
                cb.OwnsOne(c => c.ShippingAddress);
            });
            modelBuilder.Entity<Post>(entity =>
                entity
                    .HasOne(d => d.Blog)
                    .WithMany(p => p.Posts)
                    .HasForeignKey("BlogId"));

            // BlogSeed
            modelBuilder.Entity<Blog>().HasData(new Blog { BlogId = 1, Url = "http://sample.com" });

            // PostSeed
            modelBuilder.Entity<Post>().HasData(new Post() { BlogId = 1, PostId = 1, Title = "First post", Content = "Test 1" });

            // AnonymousPostSeed
            modelBuilder.Entity<Post>().HasData(new { BlogId = 1, PostId = 2, Title = "Second post", Content = "Test 2" });

            // OwnedTypeSeed
            modelBuilder.Entity<Post>().OwnsOne(p => p.AuthorName).HasData(
                new { PostId = 1, First = "Andriy", Last = "Svyryd" },
                new { PostId = 2, First = "Diego", Last = "Vega" });
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<UserDef> UserDefs { get; set; }
        public DbSet<Order> Orders { get; set; }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class UserDef
    {
        public int UserDefId { get; set; }
        public string TagsRaw { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public OrderDetails OrderDetails { get; set; }
    }
    public class OrderDetails
    {
        public StreetAddress BillingAddress { get; set; }
        public StreetAddress ShippingAddress { get; set; }
    }
    public class StreetAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
    }
}
