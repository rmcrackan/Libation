using DataLayer;
using Dinah.Core.ErrorHandling;
using LibationFileManager;
using System.Threading.Tasks;

namespace FileLiberator;

/// <summary>
/// Instantly fails processing so the bad-book error dialog can be tested without a real download failure.
/// </summary>
public class SimulateBadBookFailure : Processable, IProcessable<SimulateBadBookFailure>
{
	public override string Name => "Simulate Bad Book Failure";

	public override bool Validate(LibraryBook libraryBook) => true;

	public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
	{
		OnBegin(libraryBook);

		var result = new StatusHandler();
		result.AddError("Simulated processing failure for testing the bad-book error dialog.");

		OnCompleted(libraryBook);
		return Task.FromResult(result);
	}

	public static SimulateBadBookFailure Create(Configuration config)
		=> new() { Configuration = config };
}
