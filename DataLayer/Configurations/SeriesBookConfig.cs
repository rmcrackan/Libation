using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class SeriesBookConfig : IEntityTypeConfiguration<SeriesBook>
    {
        public void Configure(EntityTypeBuilder<SeriesBook> entity)
        {
            entity.HasKey(bc => new { bc.SeriesId, bc.BookId });

            entity.HasIndex(b => b.SeriesId);
            entity.HasIndex(b => b.BookId);

            entity
                .HasOne(sb => sb.Series)
                .WithMany(s => s.BooksLink)
                .HasForeignKey(sb => sb.SeriesId);
            entity
                .HasOne(sb => sb.Book)
                .WithMany(b => b.SeriesLink)
                .HasForeignKey(sb => sb.BookId);
        }
    }
}