namespace LibationFileManager;

/// <summary>User-facing title and body for startup failure, recovery, and crash dialogs.</summary>
public readonly record struct FatalStartupMessage(string Title, string Body);
