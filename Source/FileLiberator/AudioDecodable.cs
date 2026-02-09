using System;
using System.Threading.Tasks;

namespace FileLiberator;

public abstract class AudioDecodable : Processable
{
	public delegate byte[]? RequestCoverArtHandler(object sender, EventArgs eventArgs);
	public event RequestCoverArtHandler? RequestCoverArt;
	public event EventHandler<string>? TitleDiscovered;
	public event EventHandler<string>? AuthorsDiscovered;
	public event EventHandler<string>? NarratorsDiscovered;
	public event EventHandler<byte[]>? CoverImageDiscovered;
	public abstract Task CancelAsync();

	protected void OnTitleDiscovered(string title) => OnTitleDiscovered(null, title);
	protected void OnTitleDiscovered(object? _, string? title)
	{
		Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(TitleDiscovered), Title = title });
		if (title != null)
			TitleDiscovered?.Invoke(this, title);
	}

	protected void OnAuthorsDiscovered(string authors) => OnAuthorsDiscovered(null, authors);
	protected void OnAuthorsDiscovered(object? _, string? authors)
	{
		Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(AuthorsDiscovered), Authors = authors });
		if (authors != null)
			AuthorsDiscovered?.Invoke(this, authors);
	}

	protected void OnNarratorsDiscovered(string narrators) => OnNarratorsDiscovered(null, narrators);
	protected void OnNarratorsDiscovered(object? _, string? narrators)
	{
		Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(NarratorsDiscovered), Narrators = narrators });
		if (narrators != null)
			NarratorsDiscovered?.Invoke(this, narrators);
	}

	protected byte[]? OnRequestCoverArt()
	{
		Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(RequestCoverArt) });
		return RequestCoverArt?.Invoke(this, EventArgs.Empty);
	}

	protected void OnCoverImageDiscovered(byte[] coverImage)
	{
		Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(CoverImageDiscovered), CoverImageBytes = coverImage.Length });
		CoverImageDiscovered?.Invoke(this, coverImage);
	}
}
