using System.Globalization;
using LibationUiBase.Resources;

namespace LibationUiBase.Tests;

[TestClass]
public static class EnglishUiText
{
	/// <summary>Tests assert the English UI text, so read it in English whatever the machine's language.</summary>
	[AssemblyInitialize]
	public static void PinToEnglish(TestContext _) => Strings.Culture = CultureInfo.InvariantCulture;
}
