using System;
using System.Collections.Generic;
using System.Text;

namespace LibationUiBase.Tests;

[TestClass]
public class SkiaVersionCheck
{

	[TestMethod]
	public void SkiaVersionIs3x()
	{
		var skiaAsmVersion = typeof(SkiaSharp.SKImage).Assembly.GetName().Version;
		Assert.IsNotNull(skiaAsmVersion);
		Assert.AreEqual(3, skiaAsmVersion.Major);
	}
}
