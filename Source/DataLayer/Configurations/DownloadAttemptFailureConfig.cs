using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Configurations;

internal class DownloadAttemptFailureConfig : IEntityTypeConfiguration<DownloadAttemptFailure>
{
	public void Configure(EntityTypeBuilder<DownloadAttemptFailure> entity)
	{
		entity.HasKey(f => f.DownloadAttemptFailureId);

		// One row per title per account, upserted on each failure: the point is to remember the latest
		// verdict, not to accumulate a history.
		entity.HasIndex(f => new { f.Account, f.AudibleProductId }).IsUnique();
	}
}
