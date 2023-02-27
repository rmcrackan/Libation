using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WindowsConfigApp
{
	internal static partial class FolderIcon
	{
		// https://stackoverflow.com/a/21389253
		public static byte[] ToIcon(this Image img)
		{
			using var ms = new MemoryStream();
			using var bw = new BinaryWriter(ms);
			// Header
			bw.Write((short)0);   // 0-1 : reserved
			bw.Write((short)1);   // 2-3 : 1=ico, 2=cur
			bw.Write((short)1);   // 4-5 : number of images
								  // Image directory
			var w = img.Width;
			if (w >= 256) w = 0;
			bw.Write((byte)w);    // 0 : width of image
			var h = img.Height;
			if (h >= 256) h = 0;
			bw.Write((byte)h);    // 1 : height of image
			bw.Write((byte)0);    // 2 : number of colors in palette
			bw.Write((byte)0);    // 3 : reserved
			bw.Write((short)0);   // 4 : number of color planes
			bw.Write((short)0);   // 6 : bits per pixel
			var sizeHere = ms.Position;
			bw.Write((int)0);     // 8 : image size
			var start = (int)ms.Position + 4;
			bw.Write(start);      // 12: offset of image data
								  // Image data
			img.Save(ms, new PngEncoder());
			var imageSize = (int)ms.Position - start;
			ms.Seek(sizeHere, SeekOrigin.Begin);
			bw.Write(imageSize);
			ms.Seek(0, SeekOrigin.Begin);

			// And load it
			return ms.ToArray();
		}

		public static void DeleteIcon(this DirectoryInfo directoryInfo) => DeleteIcon(directoryInfo.FullName);
		public static void DeleteIcon(string dir)
		{
			string[] array = new string[3] { "desktop.ini", "Icon.ico", ".hidden" };
			foreach (string path in array)
			{
				string text = Path.Combine(dir, path);
				if (File.Exists(text))
				{
					File.SetAttributes(text, File.GetAttributes(text) | FileAttributes.Normal);
					new FileInfo(text).IsReadOnly = false;
					File.Delete(text);
				}
			}

			refresh();
		}

		// https://github.com/dimuththarindu/FIC-Folder-Icon-Changer/blob/master/project/FIC/Classes/IconCustomizer.cs

		public static void SetIcon(this DirectoryInfo directoryInfo, string icoPath, string folderType)
			=> SetIcon(directoryInfo.FullName, icoPath, folderType);

		public static void SetIcon(string dir, string icoPath, string folderType)
		{
			var desktop_ini = Path.Combine(dir, "desktop.ini");
			var Icon_ico = Path.Combine(dir, "Icon.ico");
			var hidden = Path.Combine(dir, ".hidden");

			//deleting existing files
			DeleteIcon(dir);

			//copying Icon file //overwriting
			File.Copy(icoPath, Icon_ico, true);

			//writing configuration file
			string[] desktopLines = { "[.ShellClassInfo]", "IconResource=Icon.ico,0", "[ViewState]", "Mode=", "Vid=", $"FolderType={folderType}" };
			File.WriteAllLines(desktop_ini, desktopLines);

			//configure file 2            
			string[] hiddenLines = { "desktop.ini", "Icon.ico" };
			File.WriteAllLines(hidden, hiddenLines);

			//making system files
			File.SetAttributes(desktop_ini, File.GetAttributes(desktop_ini) | FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);
			File.SetAttributes(Icon_ico, File.GetAttributes(Icon_ico) | FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);
			File.SetAttributes(hidden, File.GetAttributes(hidden) | FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);

			// this strangely completes the process. also hides these 3 hidden system files, even if "show hidden items" is checked
			File.SetAttributes(dir, File.GetAttributes(dir) | FileAttributes.ReadOnly);

			refresh();
		}

		private static void refresh() => SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero); //SHCNE_ASSOCCHANGED SHCNF_IDLIST


		[DllImport("shell32.dll", SetLastError = true)]
		private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
	}
}
