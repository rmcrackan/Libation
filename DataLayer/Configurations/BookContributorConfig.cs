using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class BookContributorConfig : IEntityTypeConfiguration<BookContributor>
    {
        public void Configure(EntityTypeBuilder<BookContributor> entity)
        {
            entity.HasKey(bc => new { bc.BookId, bc.ContributorId, bc.Role });

            entity.HasIndex(b => b.BookId);
            entity.HasIndex(b => b.ContributorId);

            entity
                .HasOne(bc => bc.Book)
                .WithMany(b => b.ContributorsLink)
                .HasForeignKey(bc => bc.BookId);
            entity
                .HasOne(bc => bc.Contributor)
                .WithMany(c => c.BooksLink)
                .HasForeignKey(bc => bc.ContributorId);
        }
    }
}