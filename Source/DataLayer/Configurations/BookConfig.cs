using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace DataLayer.Configurations
{
    internal class BookConfig : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> entity)
        {
            entity.HasKey(b => b.BookId);
            entity.HasIndex(b => b.AudibleProductId);

            entity.OwnsOne(b => b.Rating);

            entity.Property(nameof(Book._audioFormat));
            //
            // CRUCIAL: ignore unmapped collections, even get-only
            //
            entity.Ignore(nameof(Book.Authors));
            entity.Ignore(nameof(Book.Narrators));
            entity.Ignore(nameof(Book.AudioFormat));
            entity.Ignore(nameof(Book.TitleWithSubtitle));
            entity.Ignore(b => b.Categories);

            // OwnsMany: "Can only ever appear on navigation properties of other entity types.
            //  Are automatically loaded, and can only be tracked by a DbContext alongside their owner."
            entity
                .OwnsMany(b => b.Supplements, b_s =>
                {
                    b_s.WithOwner(s => s.Book)
                        .HasForeignKey(s => s.BookId);
                    b_s.HasKey(s => s.SupplementId);
                });
            // even though it's owned, we need to map its backing field
            entity
                .Metadata
                .FindNavigation(nameof(Book.Supplements))
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            // owns it 1:1, store in separate table
            entity
                .OwnsOne(b => b.UserDefinedItem, b_udi =>
                {
                    b_udi.WithOwner(udi => udi.Book)
                        .HasForeignKey(udi => udi.BookId);
                    b_udi.Property(udi => udi.BookId).ValueGeneratedNever();
                    b_udi.ToTable(nameof(Book.UserDefinedItem));

                    b_udi.Property(udi => udi.LastDownloaded);
                    b_udi
                        .Property(udi => udi.LastDownloadedVersion)
                        .HasConversion(ver => ver.ToString(), str => Version.Parse(str));

                    // owns it 1:1, store in same table
                    b_udi.OwnsOne(udi => udi.Rating);
                });
        }
    }
}