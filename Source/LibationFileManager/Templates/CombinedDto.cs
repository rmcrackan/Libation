using AaxDecrypter;

namespace LibationFileManager.Templates;

public class CombinedDto
{
	public LibraryBookDto LibraryBook { get; }
	public MultiConvertFileProperties? MultiConvert { get; }
	public CombinedDto(LibraryBookDto libraryBook, MultiConvertFileProperties? multiConvert = null)
	{
		LibraryBook = libraryBook;
		MultiConvert = multiConvert;
	}
}
