using FileManager;

#nullable enable
namespace AaxDecrypter;

public record TempFile
{
	public LongPath FilePath { get; init; }
	public string Extension { get; }
	public MultiConvertFileProperties? PartProperties { get; init; }
	public TempFile(LongPath filePath, string? extension = null)
	{
		FilePath = filePath;
		extension ??= System.IO.Path.GetExtension(filePath);
		Extension = FileUtility.GetStandardizedExtension(extension).ToLowerInvariant();
	}
}
