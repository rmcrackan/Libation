using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class DownloadHistoryConfig : IEntityTypeConfiguration<DownloadHistory>
{
	public void Configure(EntityTypeBuilder<DownloadHistory> entity)
	{
		entity.HasKey(dh => dh.DownloadHistoryId);

		// Every read is a range query over the rolling window, and every write prunes by age.
		entity.HasIndex(dh => dh.CompletedAtUtcTicks);
	}
}
