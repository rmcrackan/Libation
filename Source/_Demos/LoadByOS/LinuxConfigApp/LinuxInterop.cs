using CrossPlatformClientExe;

namespace LinuxConfigApp;

internal class LinuxInterop : IInteropFunctions
{
	private string InitValue1 { get; }
	private int InitValue2 { get; }

	public LinuxInterop(string initValue1, int initValue2)
	{
		InitValue1 = initValue1;
		InitValue2 = initValue2;
	}

	public string TransformInit1() => InitValue1.ToLower();

	public int TransformInit2() => InitValue2 + InitValue2;

	public void CopyTextToClipboard(string text) => throw new PlatformNotSupportedException();
	public void ShowForm() => throw new PlatformNotSupportedException();
}
