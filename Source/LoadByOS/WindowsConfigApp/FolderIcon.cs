using SixLabors.ImageSharp;
using System.IO;

namespace WindowsConfigApp;

internal static partial class FolderIcon
{
	static readonly IcoEncoder IcoEncoder = new();
	public static byte[] ToIcon(this Image img)
	{
		using var ms = new MemoryStream();
		img.Save(ms, IcoEncoder);
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
	}

	// https://github.com/dimuththarindu/FIC-Folder-Icon-Changer/blob/master/project/FIC/Classes/IconCustomizer.cs

	public static void SetIcon(this DirectoryInfo directoryInfo, byte[] icon, string folderType)
		=> SetIcon(directoryInfo.FullName, icon, folderType);

	public static void SetIcon(string dir, byte[] icon, string folderType)
	{
		var desktop_ini = Path.Combine(dir, "desktop.ini");
		var Icon_ico = Path.Combine(dir, "Icon.ico");

		//deleting existing files
		DeleteIcon(dir);

		//copying Icon file //overwriting
		File.WriteAllBytes(Icon_ico, icon);

		//writing configuration file
		string[] desktopLines = { "[.ShellClassInfo]", "ConfirmFileOp=0", "IconResource=Icon.ico,0", "[ViewState]", "Mode=", "Vid=", $"FolderType={folderType}" };
		File.WriteAllLines(desktop_ini, desktopLines);

		File.SetAttributes(desktop_ini, File.GetAttributes(desktop_ini) | FileAttributes.Hidden | FileAttributes.ReadOnly);
		File.SetAttributes(Icon_ico, File.GetAttributes(Icon_ico) | FileAttributes.Hidden | FileAttributes.ReadOnly);

		//https://learn.microsoft.com/en-us/windows/win32/shell/how-to-customize-folders-with-desktop-ini
		File.SetAttributes(dir, File.GetAttributes(dir) | FileAttributes.System);
	}
}
