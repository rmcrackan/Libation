using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using FileLiberator;
using System;
using System.Threading.Tasks;

namespace LibationUiBase.SeriesView;

internal class WishlistButton : SeriesButton
{
	private bool instanceEnabled = true;

	private bool inWishList;

	public override bool HasButtonAction => !InLibrary;
	public override string DisplayText
		=> InLibrary ? "Already\r\nOwned"
		: InWishList ? "Remove\r\nFrom\r\nWishlist"
		: "Add to\r\nWishlist";

	public override bool Enabled
	{
		get => instanceEnabled;
		protected set => RaiseAndSetIfChanged(ref instanceEnabled, value);
	}

	private bool InWishList
	{
		get => inWishList;
		set
		{
			if (inWishList != value)
			{
				inWishList = value;
				RaisePropertyChanged(nameof(InWishList));
				RaisePropertyChanged(nameof(DisplayText));
			}
		}
	}

	internal WishlistButton(Item item, bool inLibrary, bool inWishList) : base(item, inLibrary)
	{
		this.inWishList = inWishList;
	}

	public override async Task PerformClickAsync(LibraryBook accountBook)
	{
		if (!Enabled || !HasButtonAction || Item.Asin is null) return;

		Enabled = false;

		try
		{
			Api api = await accountBook.GetApiAsync();

			if (InWishList)
			{
				await api.DeleteFromWishListAsync(Item.Asin);
				InWishList = false;
			}
			else
			{
				await api.AddToWishListAsync(Item.Asin);
				InWishList = true;
			}
		}
		catch (Exception ex)
		{
			var addRemove = InWishList ? "remove" : "add";
			var toFrom = InWishList ? "from" : "to";

			Serilog.Log.Logger.Error(ex, $"Failed to {addRemove} {{book}} {toFrom} wish list", new { Item.ProductId, Item.TitleWithSubtitle });
		}
		finally { Enabled = true; }
	}

	public override int CompareTo(object? ob)
	{
		if (ob is not WishlistButton other) return -1;

		int libcmp = other.InLibrary.CompareTo(InLibrary);
		return (libcmp == 0) ? other.InWishList.CompareTo(InWishList) : libcmp;
	}
}
