using System;
using System.Collections.Generic;
using System.Text;

namespace LibationUiBase.Tests;

[TestClass]
public class SkiaVersionCheck
{
	/// <summary>
	/// Avalonia.Skia runtime links against native SkiaSharp symbols (e.g. sk_fontmgr_ref_default).
	/// Major upgrades such as SkiaSharp 4.x remove or rename these native exports, causing an
	/// EntryPointNotFoundException crash on launch for Avalonia apps on macOS and Linux (#2115).
	///
	/// LibationUiBase locks the SkiaSharp PackageReference to [3.119.4, 4.0.0) so package updaters
	/// and automated bots do not bump past 3.x. This test acts as a safety net in CI if the
	/// PackageReference is ever manually changed or overridden.
	/// </summary>
	[TestMethod]
	public void SkiaVersionIs3x()
	{
		var skiaAsmVersion = typeof(SkiaSharp.SKImage).Assembly.GetName().Version;
		Assert.IsNotNull(skiaAsmVersion);
		Assert.AreEqual(3, skiaAsmVersion.Major);
	}
}
