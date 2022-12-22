using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
        {
            entity.HasKey(c => c.CategoryId);
            entity.HasIndex(c => c.AudibleCategoryId);

            // seeds go here. examples in Dinah.EntityFrameworkCore.Tests\DbContextFactoryExample.cs
            entity.HasData(Category.GetEmpty());
        }
    }
}