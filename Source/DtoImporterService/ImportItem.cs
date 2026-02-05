using AudibleApi.Common;

namespace DtoImporterService;

public record ImportItem(Item DtoItem, string AccountId, string LocaleName)
{
	public override string ToString() => $"[{DtoItem.ProductId}] {DtoItem.Title}";
}
