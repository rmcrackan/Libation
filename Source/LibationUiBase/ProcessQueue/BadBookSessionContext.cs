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

	/// <summary>
	/// The book the user actually answered "Abort" for, when one was asked. Null when the abort came
	/// from the Bad Book setting rather than a dialog, and null when nothing has aborted.
	/// </summary>
	/// <remarks>
	/// Recorded because tearing the queue down and reporting the abort are different jobs. The
	/// teardown is claimed by whichever book finishes first and the books are interchangeable for
	/// it; the status left on a row is read afterwards by a person who remembers which book they
	/// were asked about, and "Cancelled" on that row with "Error, Abort" on some other book's is
	/// the wrong way round.
	/// </remarks>
	public ProcessBookViewModel? AbortOriginator { get; set; }

	public void Reset()
	{
		Override = null;
		AbortOriginator = null;
	}
}
