using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class SeriesBookConfig : IEntityTypeConfiguration<SeriesBook>
{
	public void Configure(EntityTypeBuilder<SeriesBook> entity)
	{
		entity.HasKey(sb => new { sb.SeriesId, sb.BookId });

		entity.HasIndex(sb => sb.SeriesId);
		entity.HasIndex(sb => sb.BookId);

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