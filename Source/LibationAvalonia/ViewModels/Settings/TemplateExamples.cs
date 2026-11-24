using FileManager;
using LibationFileManager.Templates;
using System;

namespace LibationAvalonia.ViewModels.Settings;

/// <summary>Renders a template against the same sample book the edit dialog uses.</summary>
internal static class TemplateExamples
{
	private const string Invalid = "(invalid template)";

	public static string Folder(LongPath? books, string template)
		=> Render(() => Edit<Templates.FolderTemplate>(books, template).GetFolderName(), System.IO.Path.DirectorySeparatorChar.ToString());

	public static string File(LongPath? books, string template, string extension)
		=> Render(() => Edit<Templates.FileTemplate>(books, template).GetFileName(), extension);

	public static string ChapterFile(LongPath? books, string template, string extension)
		=> Render(() => Edit<Templates.ChapterFileTemplate>(books, template).GetFileName(), extension);

	public static string ChapterTitle(string template)
		=> Render(() =>
		{
			var editor = TemplateEditor<Templates.ChapterTitleTemplate>.CreateNameEditor(template);
			editor.SetTemplateText(template);
			return editor.GetName();
		});

	// The editor only renders after SetTemplateText, as the edit dialog does.
	private static ITemplateEditor Edit<T>(LongPath? books, string template) where T : Templates, ITemplate, new()
	{
		var editor = TemplateEditor<T>.CreateFilenameEditor(books ?? System.IO.Path.GetFullPath("Books"), template);
		editor.SetTemplateText(template);
		return editor;
	}

	private static string Render(Func<string?> render, string suffix = "")
	{
		try
		{
			return render() is { Length: > 0 } text
				? text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? text : text + suffix
				: Invalid;
		}
		catch (Exception)
		{
			return Invalid;
		}
	}
}
