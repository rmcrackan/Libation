using LibationFileManager;

namespace LibationUiBase.ProcessQueue;

public class BadBookSessionContext
{
	public Configuration.BadBookAction? Override { get; set; }

	public void Reset() => Override = null;
}
