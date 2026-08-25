using ApplicationServices;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase.StatusIcons;
using System;
using System.Collections.Generic;

namespace LibationUiBase.GridView;

//This Class holds all book entry status info to help the grid properly render entries.
//The reason this info is in here instead of GridEntry is because all of this info is needed
//for the "Liberate" column's display and sorting functions.
public class EntryStatus : ReactiveObject, IComparable
{
	public LiberatedStatus? PdfStatus => LibraryCommands.Pdf_Status(Book);
	public LiberatedStatus BookStatus
	{
		get
		{
			if (IsSeries) return default;

			if ((DateTime.Now - lastBookUpdate).TotalSeconds > 2)
			{
				//Cache the BookStatus so AudibleFileStorage.AaxcExists isn't
				//called multiple times per book while sorting the solumn.
				bookStatus = LibraryCommands.Liberated_Status(Book);
				lastBookUpdate = DateTime.Now;
			}

			return bookStatus;
		}
	}

	public bool Expanded
	{
		get => field;
		set
		{
			if (value != field)
			{
				field = value;
				Invalidate(nameof(Expanded), nameof(ButtonImage));
			}
		}
	}
	public bool IsSeries { get; }
	public bool IsEpisode { get; }
	public bool IsBook => !IsSeries && !IsEpisode;
	public bool IsUnavailable
		=> !IsSeries
		&& isAbsent
		&& (
			BookStatus is not LiberatedStatus.Liberated
			|| PdfStatus is not null and not LiberatedStatus.Liberated
		);
	public double Opacity => !IsSeries && Book.UserDefinedItem.Tags.ContainsInsensitive("hidden") ? 0.4 : 1;
	public object? ButtonImage => GetAndCacheIcon(GetLiberateIconDescriptor());
	public string ToolTip => GetTooltip();
	private Book Book { get; }

	private DateTime lastBookUpdate;
	private LiberatedStatus bookStatus;
	private readonly bool isAbsent;
	private readonly bool isAudiblePlus;
	private static readonly Dictionary<LiberateIconDescriptor, object?> iconCache = [];

	internal EntryStatus(LibraryBook libraryBook)
	{
		Book = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook)).Book;
		isAbsent = libraryBook.AbsentFromLastScan is true;
		isAudiblePlus = libraryBook.IsAudiblePlus;
		IsEpisode = Book.ContentType is ContentType.Episode;
		IsSeries = Book.ContentType is ContentType.Parent;
	}

	/// <summary>Refresh BookStatus (so partial download files are checked again in the filesystem) and raise PropertyChanged for property names.</summary>
	public void Invalidate(params string[] properties)
	{
		lastBookUpdate = default;
		foreach (var property in properties)
			RaisePropertyChanged(property);
	}

	/// <summary> Defines the Liberate column's sorting behavior </summary>
	public int CompareTo(object? obj)
	{
		if (obj is not EntryStatus second) return -1;

		if (IsSeries && !second.IsSeries) return -1;
		else if (!IsSeries && second.IsSeries) return 1;
		else if (IsSeries && second.IsSeries) return 0;
		else if (IsUnavailable && !second.IsUnavailable) return 1;
		else if (!IsUnavailable && second.IsUnavailable) return -1;
		else if (BookStatus == LiberatedStatus.Liberated && second.BookStatus != LiberatedStatus.Liberated) return -1;
		else if (BookStatus != LiberatedStatus.Liberated && second.BookStatus == LiberatedStatus.Liberated) return 1;

		var statusCompare = BookStatus.CompareTo(second.BookStatus);
		if (statusCompare != 0) return statusCompare;
		else if (PdfStatus is null && second.PdfStatus is null) return 0;
		else if (PdfStatus is null) return 1;
		else if (second.PdfStatus is null) return -1;
		else return PdfStatus.Value.CompareTo(second.PdfStatus.Value);
	}

	private LiberateIconDescriptor GetLiberateIconDescriptor()
	{
		var isDark = BaseUtil.IsDarkMode();

		if (IsSeries)
			return LiberateIconDescriptor.ForSeries(Expanded, isDark);

		if (BookStatus == LiberatedStatus.Error)
			return LiberateIconDescriptor.ForError(isDark);

		var lamp = BookStatus switch
		{
			LiberatedStatus.Liberated => StoplightLamp.Green,
			LiberatedStatus.PartialDownload => StoplightLamp.Yellow,
			LiberatedStatus.NotLiberated => StoplightLamp.Red,
			_ => throw new Exception("Unexpected liberation state")
		};

		var pdf = PdfStatus switch
		{
			LiberatedStatus.Liberated => PdfOverlay.Downloaded,
			LiberatedStatus.NotLiberated => PdfOverlay.NotDownloaded,
			LiberatedStatus.Error => PdfOverlay.NotDownloaded,
			null => PdfOverlay.None,
			_ => throw new Exception("Unexpected PDF state")
		};

		return LiberateIconDescriptor.ForBook(lamp, pdf, isAudiblePlus, isDark);
	}

	private string GetTooltip()
	{
		if (IsSeries)
			return Expanded ? "Click to Collapse" : "Click to Expand";

		if (IsUnavailable)
			return "This book cannot be downloaded\nbecause it wasn't found during\nthe most recent library scan";

		if (BookStatus == LiberatedStatus.Error)
			return "Book downloaded ERROR";

		string libState = BookStatus switch
		{
			LiberatedStatus.Liberated => "Liberated",
			LiberatedStatus.PartialDownload => "File has been at least\r\npartially downloaded",
			LiberatedStatus.NotLiberated => "Book download pending",
			_ => throw new Exception("Unexpected liberation state")
		};

		string pdfState = PdfStatus switch
		{
			LiberatedStatus.Liberated => "\r\nPDF downloaded",
			LiberatedStatus.NotLiberated => "\r\nPDF download pending",
			// Set when Audible granted a license carrying no PDF link, which is by far the likeliest way a
			// title arrives here, and says more than "ERROR" about why Libation has stopped asking.
			LiberatedStatus.Error => "\r\nPDF could not be downloaded\r\nand will not be tried again",
			null => "",
			_ => throw new Exception("Unexpected PDF state")
		};

		var plusState = isAudiblePlus ? "\r\nAudible Plus title" : "";

		var mouseoverText = libState + pdfState + plusState;

		if (BookStatus == LiberatedStatus.NotLiberated ||
			BookStatus == LiberatedStatus.PartialDownload ||
			PdfStatus == LiberatedStatus.NotLiberated)
			mouseoverText += "\r\nClick to download";

		return mouseoverText;
	}

	/// <summary>
	/// Load the shared rendering of an icon into this UI framework's image format. There are only a
	/// couple dozen icons, and grid entries are created on several threads, so cache them all.
	/// </summary>
	private static object? GetAndCacheIcon(LiberateIconDescriptor descriptor)
	{
		lock (iconCache)
		{
			if (!iconCache.TryGetValue(descriptor, out var icon))
				iconCache[descriptor] = icon = BaseUtil.LoadImage(StatusImageGenerator.GetPng(descriptor), PictureSize.Native);
			return icon;
		}
	}
}
