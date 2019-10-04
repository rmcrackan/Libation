using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class SupplementConfig : IEntityTypeConfiguration<Supplement>
    {
        public void Configure(EntityTypeBuilder<Supplement> entity)
        {
            entity.HasKey(s => s.SupplementId);
            entity
                .HasOne(s => s.Book)
                .WithMany(b => b.Supplements)
                .HasForeignKey(s => s.BookId);
        }
    }
}
