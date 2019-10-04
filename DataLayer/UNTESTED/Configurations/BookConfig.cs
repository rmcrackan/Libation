using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class BookConfig : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> entity)
        {
            entity.HasKey(b => b.BookId);
            entity.HasIndex(b => b.AudibleProductId);

            entity.OwnsOne(b => b.Rating);

            //
            // CRUCIAL: ignore unmapped collections, even get-only
            //
            entity.Ignore(nameof(Book.Authors));
            entity.Ignore(nameof(Book.Narrators));
            //// these don't seem to matter
            //entity.Ignore(nameof(Book.AuthorNames));
            //entity.Ignore(nameof(Book.NarratorNames));
            //entity.Ignore(nameof(Book.HasPdfs));

            // OwnsMany: "Can only ever appear on navigation properties of other entity types.
            //  Are automatically loaded, and can only be tracked by a DbContext alongside their owner."
            entity.OwnsMany(b => b.Supplements);
            // even though it's owned, we need to map its backing field
            entity
                .Metadata
                .FindNavigation(nameof(Book.Supplements))
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            // owns it 1:1, but store in separate table
            entity
                .OwnsOne(b => b.UserDefinedItem, b_udi => b_udi.ToTable(nameof(Book.UserDefinedItem)));
            // UserDefinedItem must link back to book so we know how to log changed tags.
            // ie: when a tag changes, we need to get the parent book's product id
            entity
                .HasOne(b => b.UserDefinedItem)
                .WithOne(udi => udi.Book)
                .HasForeignKey<UserDefinedItem>(udi => udi.BookId);

            entity
                .Metadata
                .FindNavigation(nameof(Book.ContributorsLink))
                // PropertyAccessMode.Field : Contributions is a get-only property, not a field, so use its backing field
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            entity
                .Metadata
                .FindNavigation(nameof(Book.SeriesLink))
                // PropertyAccessMode.Field : Series is a get-only property, not a field, so use its backing field
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            entity
                .HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId);
        }
    }
}