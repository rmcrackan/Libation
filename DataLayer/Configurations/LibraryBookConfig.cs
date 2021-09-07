using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations
{
    internal class LibraryBookConfig : IEntityTypeConfiguration<LibraryBook>
    {
        public void Configure(EntityTypeBuilder<LibraryBook> entity)
        {
            // to allow same book (incl region) with diff acct.s:
            //
            // this file:
            // - composite key:
            //     entity.HasKey(b => new { b.BookId, b.Account });
            //     entity.HasIndex(b => b.BookId);
            //     entity.HasIndex(b => b.Account);
            // - change the below relationship since Book+LibraryBook would no longer be 1:1
            //
            // other files:
            // - change Book class since Book+LibraryBook would no longer be 1:1
            // - update LibraryBook import code
            // - would likely challenge assumptions throughout Libation which have been true up until now

            entity.HasKey(b => b.BookId);

            entity
                .HasOne(le => le.Book)
                .WithOne()
                .HasForeignKey<LibraryBook>(le => le.BookId);
        }
    }
}