using ApplicationServices;
using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;

namespace LibationUiBase.GridView
{
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
			& isAbsent
			& (
				BookStatus is not LiberatedStatus.Liberated
				|| PdfStatus is not null and not LiberatedStatus.Liberated
			);
		public double Opacity => !IsSeries && Book.UserDefinedItem.Tags.ContainsInsensitive("hidden") ? 0.4 : 1;
		public object? ButtonImage => GetLiberateIcon();
		public string ToolTip => GetTooltip();
		private Book Book { get; }

		private DateTime lastBookUpdate;
		private LiberatedStatus bookStatus;
		private readonly bool isAbsent;
		private static readonly Dictionary<string, object?> iconCache = new();

		internal EntryStatus(LibraryBook libraryBook)
		{
			Book = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook)).Book;
			isAbsent = libraryBook.AbsentFromLastScan is true;
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

		private object? GetLiberateIcon()
		{
			if (IsSeries)
				return Expanded ? GetAndCacheResource("minus") : GetAndCacheResource("plus");

			if (BookStatus == LiberatedStatus.Error)
				return GetAndCacheResource("error");

			string image_lib = BookStatus switch
			{
				LiberatedStatus.Liberated => "green",
				LiberatedStatus.PartialDownload => "yellow",
				LiberatedStatus.NotLiberated => "red",
				_ => throw new Exception("Unexpected liberation state")
			};

			string image_pdf = PdfStatus switch
			{
				LiberatedStatus.Liberated => "_pdf_yes",
				LiberatedStatus.NotLiberated => "_pdf_no",
				LiberatedStatus.Error => "_pdf_no",
				null => "",
				_ => throw new Exception("Unexpected PDF state")
			};

			return GetAndCacheResource($"liberate_{image_lib}{image_pdf}");
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
				LiberatedStatus.NotLiberated => "Book NOT downloaded",
				_ => throw new Exception("Unexpected liberation state")
			};

			string pdfState = PdfStatus switch
			{
				LiberatedStatus.Liberated => "\r\nPDF downloaded",
				LiberatedStatus.NotLiberated => "\r\nPDF NOT downloaded",
				LiberatedStatus.Error => "\r\nPDF downloaded ERROR",
				null => "",
				_ => throw new Exception("Unexpected PDF state")
			};

			var mouseoverText = libState + pdfState;

			if (BookStatus == LiberatedStatus.NotLiberated ||
				BookStatus == LiberatedStatus.PartialDownload ||
				PdfStatus == LiberatedStatus.NotLiberated)
				mouseoverText += "\r\nClick to complete";

			return mouseoverText;
		}

		private object? GetAndCacheResource(string rescName)
		{
			if (!iconCache.ContainsKey(rescName))
				iconCache[rescName] = BaseUtil.LoadResourceImage(rescName);
			return iconCache[rescName];
		}
	}
}
