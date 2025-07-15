using DataLayer;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.ProcessQueue;

#nullable enable
namespace LibationAvalonia.ViewModels;

public class ProcessBookViewModel : ProcessBookViewModelBase
{

	public ProcessBookViewModel(LibraryBook libraryBook, LogMe logme) : base(libraryBook, logme) { }

	protected override object? LoadImageFromBytes(byte[] bytes, PictureSize pictureSize)
		=> AvaloniaUtils.TryLoadImageOrDefault(bytes, pictureSize);

}