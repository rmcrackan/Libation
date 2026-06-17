namespace LibationFileManager;

/// <summary>User-facing title and body for startup failure, recovery, and crash dialogs.</summary>
public sealed record FatalStartupMessage(string Title, string Body);
