using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Configurations
{
	internal class BookCategoryConfig : IEntityTypeConfiguration<BookCategory>
	{
		public void Configure(EntityTypeBuilder<BookCategory> entity)
		{
			entity.HasKey(bc => new { bc.BookId, bc.CategoryLadderId });

			entity.HasIndex(bc => bc.BookId);
			entity.HasIndex(bc => bc.CategoryLadderId);

			entity
				.HasOne(bc => bc.Book)
				.WithMany(b => b.CategoriesLink)
				.HasForeignKey(bc => bc.BookId);

			entity
				.HasOne(bc => bc.CategoryLadder)
				.WithMany(c => c.BooksLink)
				.HasForeignKey(bc => bc.CategoryLadderId);
		}
	}
}
