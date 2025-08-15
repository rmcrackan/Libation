using System.Diagnostics;

namespace AssertionHelper;

public static class AssertionExtensions
{
	[StackTraceHidden]
	public static T? Should<T>(this T? value) => value;

	[StackTraceHidden]
	public static void Be<T>(this T? value, T? expectedValue) where T : IEquatable<T>
		=> Assert.AreEqual(value, expectedValue);

	[StackTraceHidden]
	public static void BeNull<T>(this T? value) where T : class
		=> Assert.IsNull(value);

	[StackTraceHidden]
	public static void BeSameAs<T>(this T? value, T? otherValue)
		=> Assert.AreSame(value, otherValue);

	[StackTraceHidden]
	public static void BeFalse(this bool value)
		=> Assert.IsFalse(value);

	[StackTraceHidden]
	public static void BeTrue(this bool value)
		=> Assert.IsTrue(value);

	[StackTraceHidden]
	public static void HaveCount<T>(this IEnumerable<T?> value, int expected)
		=> Assert.HasCount(expected, value);

	[StackTraceHidden]
	public static void BeEquivalentTo<T>(this IEnumerable<T?>? value, IEnumerable<T?>? expectedValue)
		=> CollectionAssert.AreEquivalent(value, expectedValue, EqualityComparer<T?>.Default);
}
