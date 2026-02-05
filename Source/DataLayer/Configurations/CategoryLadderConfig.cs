using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class CategoryLadderConfig : IEntityTypeConfiguration<CategoryLadder>
{
	public void Configure(EntityTypeBuilder<CategoryLadder> entity)
	{
		entity.HasKey(cl => cl.CategoryLadderId);

		entity.Ignore(cl => cl.Categories);

		entity
			.HasMany(cl => cl._categories)
			.WithMany(c => c._categoryLadders);

		entity
			.Metadata
			.FindNavigation(nameof(CategoryLadder.BooksLink))
			?.SetPropertyAccessMode(PropertyAccessMode.Field);
	}
}
