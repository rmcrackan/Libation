using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class SeriesConfig : IEntityTypeConfiguration<Series>
{
	public void Configure(EntityTypeBuilder<Series> entity)
	{
		entity.HasKey(s => s.SeriesId);
		entity.HasIndex(s => s.AudibleSeriesId);

		entity
			.Metadata
			.FindNavigation(nameof(Series.BooksLink))
			?.SetPropertyAccessMode(PropertyAccessMode.Field);
	}
}