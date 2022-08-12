using System;

namespace CrossPlatformClientExe
{
	public interface IInteropFunctions
	{
		public string TransformInit1();
		public int TransformInit2();
		public void CopyTextToClipboard(string text);
		public void ShowForm();
	}
}
