using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class CategoryConfig : IEntityTypeConfiguration<Category>
{
	public void Configure(EntityTypeBuilder<Category> entity)
	{
		entity.HasKey(c => c.CategoryId);
		entity.HasIndex(c => c.AudibleCategoryId);

		entity.Ignore(c => c.CategoryLadders);

		entity
			.HasMany(e => e._categoryLadders)
			.WithMany(e => e._categories);
	}
}