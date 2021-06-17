using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class LibraryBookConfig : IEntityTypeConfiguration<LibraryBook>
    {
        public void Configure(EntityTypeBuilder<LibraryBook> entity)
        {
            entity.HasKey(b => b.BookId);

            entity
                .HasOne(le => le.Book)
                .WithOne()
                .HasForeignKey<LibraryBook>(le => le.BookId);
        }
    }
}