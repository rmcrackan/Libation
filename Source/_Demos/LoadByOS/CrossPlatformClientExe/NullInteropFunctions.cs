namespace CrossPlatformClientExe;

internal class NullInteropFunctions : IInteropFunctions
{
	public NullInteropFunctions(params object[] values) { }

	public string TransformInit1() => throw new PlatformNotSupportedException();
	public int TransformInit2() => throw new PlatformNotSupportedException();
	public void CopyTextToClipboard(string text) => throw new PlatformNotSupportedException();
	public void ShowForm() => throw new PlatformNotSupportedException();
}
