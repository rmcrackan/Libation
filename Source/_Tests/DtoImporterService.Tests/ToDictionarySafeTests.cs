using AssertionHelper;
using DtoImporterService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

[assembly: Parallelize]

namespace DtoImporterService.Tests;

/// <summary>
/// Tests for <see cref="LibraryBookImporter.ToDictionarySafe{TKey,TSource}"/>.
///
/// The method is used to build two dictionaries during library import. In both
/// cases the key must be a composite of (ProductId, AccountId) so that two
/// accounts owning the same book each get their own entry. These tests verify
/// the deduplication and tie-breaking behaviour of the method independent of
/// EF Core or the Audible API types.
/// </summary>
[TestClass]
public class ToDictionarySafe_CompositeKey
{
    private static readonly System.Func<string, string, string> keepFirst = (a, _) => a;
    private static readonly System.Func<string, string, string> keepSecond = (_, b) => b;

    [TestMethod]
    public void UniqueKeys_AllEntriesKept()
    {
        var source = new[] { "a", "b", "c" };
        var result = LibraryBookImporter.ToDictionarySafe(source, s => s, keepFirst);

        result.Count.Should().Be(3);
        result["a"].Should().Be("a");
        result["b"].Should().Be("b");
        result["c"].Should().Be("c");
    }

    [TestMethod]
    public void DuplicateKey_TieBreakerCalled_KeepsFirst()
    {
        var source = new[] { "first", "second" };
        var result = LibraryBookImporter.ToDictionarySafe(source, _ => "same-key", keepFirst);

        result.Count.Should().Be(1);
        result["same-key"].Should().Be("first");
    }

    [TestMethod]
    public void DuplicateKey_TieBreakerCalled_KeepsSecond()
    {
        var source = new[] { "first", "second" };
        var result = LibraryBookImporter.ToDictionarySafe(source, _ => "same-key", keepSecond);

        result.Count.Should().Be(1);
        result["same-key"].Should().Be("second");
    }

    /// <summary>
    /// Regression: before the fix, both dictionaries were keyed by ProductId alone.
    /// Two items with the same ProductId but different AccountIds would collide,
    /// causing one account's entry to silently overwrite the other's.
    /// With a composite (ProductId, AccountId) key, both entries are preserved.
    /// </summary>
    [TestMethod]
    public void CompositeKey_SameProductId_DifferentAccountId_BothKept()
    {
        var source = new[]
        {
            ("book-X", "account-A", "value-A"),
            ("book-X", "account-B", "value-B"),
        };

        var result = LibraryBookImporter.ToDictionarySafe(
            source,
            item => (item.Item1, item.Item2),   // composite (ProductId, AccountId) key
            (a, _) => a);

        result.Count.Should().Be(2);
        result[("book-X", "account-A")].Should().Be(("book-X", "account-A", "value-A"));
        result[("book-X", "account-B")].Should().Be(("book-X", "account-B", "value-B"));
    }

    /// <summary>
    /// Regression: before the fix, keying by ProductId alone meant that two items
    /// for the same (ProductId, AccountId) pair — e.g. a book appearing as both a
    /// purchased title and an Audible Plus title — could not be resolved with a
    /// tieBreaker. With the composite key, duplicate (ProductId, AccountId) pairs
    /// are correctly deduplicated by the tieBreaker while distinct accounts are kept.
    /// </summary>
    [TestMethod]
    public void CompositeKey_SameProductIdAndAccountId_TieBreakerApplied()
    {
        var source = new[]
        {
            ("book-X", "account-A", "available"),
            ("book-X", "account-A", "unavailable"),
        };

        // prefer the "available" entry (first wins in this tieBreaker)
        var result = LibraryBookImporter.ToDictionarySafe(
            source,
            item => (item.Item1, item.Item2),
            (a, b) => a.Item3 == "available" ? a : b);

        result.Count.Should().Be(1);
        result[("book-X", "account-A")].Item3.Should().Be("available");
    }

    [TestMethod]
    public void EmptySource_ReturnsEmptyDictionary()
    {
        var result = LibraryBookImporter.ToDictionarySafe(
            System.Array.Empty<string>(),
            s => s,
            keepFirst);

        result.Count.Should().Be(0);
    }

    [TestMethod]
    public void SingleElement_ReturnsSingleEntry()
    {
        var result = LibraryBookImporter.ToDictionarySafe(
            new[] { "only" },
            s => s,
            keepFirst);

        result.Count.Should().Be(1);
        result["only"].Should().Be("only");
    }

    [TestMethod]
    public void ThreeAccountsSameBook_AllThreeKept()
    {
        var source = new[]
        {
            ("book-Z", "account-1", "v1"),
            ("book-Z", "account-2", "v2"),
            ("book-Z", "account-3", "v3"),
        };

        var result = LibraryBookImporter.ToDictionarySafe(
            source,
            item => (item.Item1, item.Item2),
            (a, _) => a);

        result.Count.Should().Be(3);
        result[("book-Z", "account-1")].Item3.Should().Be("v1");
        result[("book-Z", "account-2")].Item3.Should().Be("v2");
        result[("book-Z", "account-3")].Item3.Should().Be("v3");
    }

    [TestMethod]
    public void MixedDuplicatesAndUnique_HandledCorrectly()
    {
        // account-A owns book-X twice (dup for same account) and book-Y once
        // account-B owns book-X once (different account, same book)
        var source = new[]
        {
            ("book-X", "account-A", "first"),
            ("book-X", "account-A", "second"),  // dup same account → tieBreak
            ("book-Y", "account-A", "only-Y"),
            ("book-X", "account-B", "account-B-entry"),
        };

        var result = LibraryBookImporter.ToDictionarySafe(
            source,
            item => (item.Item1, item.Item2),
            keepFirst);

        result.Count.Should().Be(3);
        result[("book-X", "account-A")].Item3.Should().Be("first");   // tieBreak: kept first
        result[("book-Y", "account-A")].Item3.Should().Be("only-Y");
        result[("book-X", "account-B")].Item3.Should().Be("account-B-entry");
    }
}
