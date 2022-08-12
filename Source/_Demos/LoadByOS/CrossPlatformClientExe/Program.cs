using System;

namespace CrossPlatformClientExe
{
	class Program
	{
		[STAThread]
		public static void Main()
		{
			var interopInstance = new OSInteropProxy("this IS SOME text", 42);

			Console.WriteLine("X-Formed Value 1: {0}", interopInstance.TransformInit1());
			Console.WriteLine("X-Formed Value 2: {0}", interopInstance.TransformInit2());

			interopInstance.ShowForm();
			interopInstance.CopyTextToClipboard("This is copied text!");
		}
	}
}