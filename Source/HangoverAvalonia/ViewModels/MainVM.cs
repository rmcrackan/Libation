using System;
using System.Threading.Tasks;

namespace HangoverAvalonia.ViewModels;

public partial class MainVM : ViewModelBase
{
	public Func<string, Task<bool>>? ConfirmDbMutationAsync { get; set; }

	public MainVM()
	{
		Load_databaseVM();
		Load_deletedVM();
	}
}
