using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class UserDefinedItemConfig : IEntityTypeConfiguration<UserDefinedItem>
    {
        public void Configure(EntityTypeBuilder<UserDefinedItem> entity)
        {
            entity.OwnsOne(p => p.Rating);
        }
    }
}
