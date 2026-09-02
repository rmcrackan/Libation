using AssertionHelper;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLiberator.Tests;

/// <summary>
/// A download must speak to the marketplace the book was scanned from, not only the one the account logged
/// into. Issue #2020: a Germany-registered account with UK ticked as an extra marketplace scanned UK titles
/// correctly, then asked <c>api.audible.de</c> for their licenses and got NotFound.
/// </summary>
[TestClass]
public class GetApiStoreLocaleTests
{
	[TestMethod]
	public void A_book_from_an_extra_marketplace_is_licensed_there_not_at_the_home_store()
		=> MockLibraryBook.CreateBook(localeName: "uk", title: "Kill Box")
			.StoreLocale().Name.Should().Be("uk");

	[TestMethod]
	public void A_book_from_the_registered_marketplace_is_still_licensed_there()
		=> MockLibraryBook.CreateBook(localeName: "germany", title: "Home Store Title")
			.StoreLocale().Name.Should().Be("germany");
}
