using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class ContributorConfig : IEntityTypeConfiguration<Contributor>
{
	public void Configure(EntityTypeBuilder<Contributor> entity)
	{
		entity.HasKey(c => c.ContributorId);
		entity.HasIndex(c => c.Name);

		//entity.OwnsOne(b => b.AuthorProperty);
		// ... in separate table

		entity
			.Metadata
			.FindNavigation(nameof(Contributor.BooksLink))
			?.SetPropertyAccessMode(PropertyAccessMode.Field);

		// seeds go here. examples in Dinah.EntityFrameworkCore.Tests\DbContextFactoryExample.cs
		entity.HasData(Contributor.GetEmpty());
	}
}