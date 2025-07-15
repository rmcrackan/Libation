using DataLayer;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.ProcessQueue;

namespace LibationWinForms.ProcessQueue;

public class ProcessBookViewModel : ProcessBookViewModelBase
{
	public ProcessBookViewModel(LibraryBook libraryBook, LogMe logme) : base(libraryBook, logme) { }

	protected override object LoadImageFromBytes(byte[] bytes, PictureSize pictureSize)
		=> WinFormsUtil.TryLoadImageOrDefault(bytes, PictureSize._80x80);

	public string BookText => $"{Title}\r\nBy {Author}\r\nNarrated by {Narrator}";
}
