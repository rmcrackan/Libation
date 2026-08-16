using System.Threading;
using LibationFileManager;

namespace LibationUiBase.ProcessQueue;

public class BadBookSessionContext
{
	public Configuration.BadBookAction? Override { get; set; }

	/// <summary>
	/// Serialises the "skip this book?" dialog. Several books can now fail at the same time, and the
	/// answer to one of them may be "apply to all remaining books" - which has to be recorded before
	/// the next book decides whether it still needs to ask.
	/// </summary>
	internal SemaphoreSlim DialogGate { get; } = new(1, 1);

	public void Reset() => Override = null;
}
