namespace LibationUiBase.Tests;

[TestClass]
public class ExceptionDisplayTests
{
    /// <summary>Outer exception with a fixed <see cref="Exception.StackTrace"/> so golden-string tests are stable.</summary>
    private sealed class ExceptionWithFixedStack : Exception
    {
        private string _fixedStack { get; }
        public ExceptionWithFixedStack(string message, Exception? innerException, string fixedStack)
            : base(message, innerException) => _fixedStack = fixedStack;
        public override string? StackTrace => _fixedStack;
    }

    /// <summary>
    /// Golden output for <see cref="ExceptionDisplay.FormatMessageAndStackTrace"/> when there are fourteen
    /// <see cref="Exception.InnerException"/> links (fifteen messages: outer L0 … deepest L14): first ten inners,
    /// two omitted (L11–L12), then the two deepest (L13–L14), then the outer stack trace.
    /// </summary>
    [TestMethod]
    public void _FullText_14Levels()
    {
        const string stack = "<<GOLDEN-TEST-STACK-TRACE>>";
        var messages = Enumerable.Range(0, 15).Select(i => $"L{i}").ToArray();
        var ex = CreateChainWithFixedStack(stack, messages);

        const string expected = """
L0

Inner exception: L1

Inner exception: L2

Inner exception: L3

Inner exception: L4

Inner exception: L5

Inner exception: L6

Inner exception: L7

Inner exception: L8

Inner exception: L9

Inner exception: L10

2 inner exceptions omitted.

Inner exception: L13

Inner exception: L14

<<GOLDEN-TEST-STACK-TRACE>>
""";
        Assert.AreEqual(expected.ReplaceLineEndings("\n"), Format(ex));
    }

    /// <inheritdoc cref="CreateChain"/>
    /// <remarks>Same chain shape as <see cref="CreateChain"/>, but the outer exception reports <paramref name="stackTrace"/> from <see cref="Exception.StackTrace"/>.</remarks>
    private static Exception CreateChainWithFixedStack(string stackTrace, params string[] messages)
    {
        if (messages.Length == 0)
            throw new ArgumentException("At least one message is required.", nameof(messages));

        var innermost = new Exception(messages[^1]);
        for (var i = messages.Length - 2; i >= 1; i--)
            innermost = new Exception(messages[i], innermost);
        return new ExceptionWithFixedStack(messages[0], innermost, stackTrace);
    }

    /// <summary>messages[0] = outer; each following string is the next <see cref="Exception.InnerException"/> message.</summary>
    private static Exception CreateChain(params string[] messages)
    {
        if (messages.Length == 0)
            throw new ArgumentException("At least one message is required.", nameof(messages));

        var innermost = new Exception(messages[^1]);
        for (var i = messages.Length - 2; i >= 0; i--)
            innermost = new Exception(messages[i], innermost);
        return innermost;
    }

    private static string Format(Exception ex) => ExceptionDisplay.FormatMessageAndStackTrace(ex).ReplaceLineEndings("\n");

    [TestMethod]
    public void NoInnerException_ContainsOuterMessageAndStack()
    {
        var ex = new Exception("outer only");
        var text = Format(ex);

        Assert.StartsWith("outer only\n", text);
        Assert.IsFalse(text.Contains("Inner exception:", StringComparison.Ordinal));
        if (ex.StackTrace is not null)
            Assert.IsTrue(text.Contains(ex.StackTrace, StringComparison.Ordinal));
    }

    [TestMethod]
    public void OneInner_ShowsInnerLine()
    {
        var ex = CreateChain("outer", "inner-a");
        var text = Format(ex);

        var expectedHead = """
            outer

            Inner exception: inner-a


            """.ReplaceLineEndings("\n");
        Assert.AreEqual(expectedHead + ex.StackTrace, text);
    }

    [TestMethod]
    public void TenInners_ShowsAllTen()
    {
        var messages = new[] { "m0", "m1", "m2", "m3", "m4", "m5", "m6", "m7", "m8", "m9", "m10" };
        var ex = CreateChain(messages);
        var text = Format(ex);

        for (var i = 1; i <= 10; i++)
            Assert.IsTrue(text.Contains($"Inner exception: m{i}\n", StringComparison.Ordinal), $"missing inner m{i}");
        Assert.IsFalse(text.Contains("omitted", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void ElevenInners_FirstTenThenDeepestOnly_NoOmitLine()
    {
        var messages = Enumerable.Range(0, 12).Select(i => $"n{i}").ToArray();
        var ex = CreateChain(messages);
        var text = Format(ex);

        for (var i = 1; i <= 10; i++)
            Assert.IsTrue(text.Contains($"Inner exception: n{i}\n", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("omitted", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(text.Contains("Inner exception: n11\n", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TwelveInners_FirstTenThenLastTwo_NoOmitLine()
    {
        var messages = Enumerable.Range(0, 13).Select(i => $"p{i}").ToArray();
        var ex = CreateChain(messages);
        var text = Format(ex);

        for (var i = 1; i <= 10; i++)
            Assert.IsTrue(text.Contains($"Inner exception: p{i}\n", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("omitted", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(text.Contains("Inner exception: p11\n", StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("Inner exception: p12\n", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThirteenInners_FirstTen_OmitOne_ThenLastTwo()
    {
        var messages = Enumerable.Range(0, 14).Select(i => $"q{i}").ToArray();
        var ex = CreateChain(messages);
        var text = Format(ex);

        for (var i = 1; i <= 10; i++)
            Assert.IsTrue(text.Contains($"Inner exception: q{i}\n", StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("1 inner exception omitted.\n", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Inner exception: q11\n", StringComparison.Ordinal), "omitted inner should not appear");
        Assert.IsTrue(text.Contains("Inner exception: q12\n", StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("Inner exception: q13\n", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ManyInners_OmitCountPlural()
    {
        var depth = 20;
        var messages = Enumerable.Range(0, depth + 1).Select(i => $"x{i}").ToArray();
        var ex = CreateChain(messages);
        var text = Format(ex);

        var omitted = depth - 10 - 2;
        Assert.IsTrue(text.Contains($"{omitted} inner exceptions omitted.\n", StringComparison.Ordinal));
        Assert.IsTrue(text.Contains($"Inner exception: x{depth - 1}\n", StringComparison.Ordinal));
        Assert.IsTrue(text.Contains($"Inner exception: x{depth}\n", StringComparison.Ordinal));
    }
}
