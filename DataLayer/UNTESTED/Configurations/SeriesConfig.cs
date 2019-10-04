using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class SeriesConfig : IEntityTypeConfiguration<Series>
    {
        public void Configure(EntityTypeBuilder<Series> entity)
        {
            entity.HasKey(b => b.SeriesId);
            entity.HasIndex(b => b.AudibleSeriesId);

            entity
                .Metadata
                .FindNavigation(nameof(Series.BooksLink))
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}