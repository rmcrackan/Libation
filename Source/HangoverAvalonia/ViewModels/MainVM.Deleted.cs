using System.Threading.Tasks;

namespace HangoverAvalonia.ViewModels;

public partial class MainVM
{
	public TrashBinViewModel TrashBinViewModel { get; } = new();

	private void Load_deletedVM()
	{
		TrashBinViewModel.ConfirmDbMutationAsync = action
			=> ConfirmDbMutationAsync is { } confirm
				? confirm(action)
				: Task.FromResult(true);
	}
}
