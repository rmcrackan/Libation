using Dinah.Core;
using System;

namespace DataLayer;

public class LibraryBook
{
	internal int BookId { get; private set; }
	public Book Book { get; private set; }

	public DateTime DateAdded { get; private set; }
	public string Account { get; private set; }

	public bool IsDeleted { get; set; }
	public bool AbsentFromLastScan { get; set; }

	public DateTime? IncludedUntil { get; private set; }
	public bool IsAudiblePlus { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private LibraryBook() { }
#pragma warning restore CS8618
	public LibraryBook(Book book, DateTime dateAdded, string account)
	{
		ArgumentValidator.EnsureNotNull(book, nameof(book));
		ArgumentValidator.EnsureNotNull(account, nameof(account));

		Book = book;
		DateAdded = dateAdded;
		Account = account;
	}

	public void SetAccount(string account) => Account = account;
	public void SetIncludedUntil(DateTime? includedUntil) => IncludedUntil = includedUntil;
	public void SetIsAudiblePlus(bool isAudiblePlus) => IsAudiblePlus = isAudiblePlus;
	public override string ToString() => $"{DateAdded:d} {Book}";
}