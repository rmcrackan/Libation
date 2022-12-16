namespace HangoverAvalonia.ViewModels
{
	public partial class MainVM : ViewModelBase
	{
		public MainVM()
		{
			Load_databaseVM();
			Load_deletedVM();
		}
	}
}
