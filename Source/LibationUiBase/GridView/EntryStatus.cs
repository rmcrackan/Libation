using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LibationUiBase.GridView
{
	public interface IEntryStatus
	{
		static abstract EntryStatus Create(LibraryBook libraryBook);
	}

	//This Class holds all book entry status info to help the grid properly render the items. It 
	public abstract class EntryStatus : SynchronizeInvoker, IComparable, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public LiberatedStatus? PdfStatus => LibraryCommands.Pdf_Status(Book);
		public LiberatedStatus BookStatus
		{
			get
			{
				if (IsSeries) return default;

				if ((DateTime.Now - lastBookUpdate).TotalSeconds > 2)
				{
					bookStatus = LibraryCommands.Liberated_Status(Book);
					lastBookUpdate = DateTime.Now;
				}

				return bookStatus;
			}
		}

		public bool Expanded { get; set; }
		public bool IsSeries { get; }
		public bool IsEpisode { get; }
		public bool IsBook => !IsSeries && !IsEpisode;
		public bool IsUnavailable => !IsSeries & isAbsent & (BookStatus is not LiberatedStatus.Liberated || PdfStatus is not null and not LiberatedStatus.Liberated);
		public double Opacity => !IsSeries && Book.UserDefinedItem.Tags.ContainsInsensitive("hidden") ? 0.4 : 1;
		public abstract object BackgroundBrush { get; }
		public object ButtonImage => GetLiberateIcon();
		public string ToolTip => GetTooltip();
		protected Book Book { get; }

		private DateTime lastBookUpdate;
		private LiberatedStatus bookStatus;
		private bool isAbsent;
		private static readonly Dictionary<string, object> iconCache = new();

		protected EntryStatus(LibraryBook libraryBook)
		{
			Book = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook)).Book;
			isAbsent = libraryBook.AbsentFromLastScan is true;
			IsEpisode = Book.ContentType is ContentType.Episode;
			IsSeries = Book.ContentType is ContentType.Parent;
		}

		internal protected abstract object LoadImage(byte[] picture);
		protected abstract object GetResourceImage(string rescName);
		public void RaisePropertyChanged(PropertyChangedEventArgs args) => this.UIThreadSync(() => PropertyChanged?.Invoke(this, args));
		public void RaisePropertyChanged(string propertyName) => RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
		public void Invalidate(params string[] properties)
		{
			lastBookUpdate = default;
			foreach (var property in properties)
				RaisePropertyChanged(property);
		}


		/// <summary> Defines the Liberate column's sorting behavior </summary>
		public int CompareTo(object obj)
		{
			if (obj is not EntryStatus second) return -1;

			if (IsSeries && !second.IsSeries) return -1;
			else if (!IsSeries && second.IsSeries) return 1;
			else if (IsSeries && second.IsSeries) return 0;
			else if (IsUnavailable && !second.IsUnavailable) return 1;
			else if (!IsUnavailable && second.IsUnavailable) return -1;
			else if (BookStatus == LiberatedStatus.Liberated && second.BookStatus != LiberatedStatus.Liberated) return -1;
			else if (BookStatus != LiberatedStatus.Liberated && second.BookStatus == LiberatedStatus.Liberated) return 1;
			else return BookStatus.CompareTo(second.BookStatus);
		}

		private object GetLiberateIcon()
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
				return Expanded ? "Click to Collpase" : "Click to Expand";

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

		private object GetAndCacheResource(string rescName)
		{
			if (!iconCache.ContainsKey(rescName))
				iconCache[rescName] = GetResourceImage(rescName);
			return iconCache[rescName];
		}
	}
}
