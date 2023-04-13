using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class ApprovalNeededDialog : DialogWindow
	{
		public ApprovalNeededDialog() : base(saveAndRestorePosition: false)
		{
			InitializeComponent();
		}

		protected override Task SaveAndCloseAsync()
		{
			Serilog.Log.Logger.Information("Approve button clicked");

			return base.SaveAndCloseAsync();
		}

		public async void Approve_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
	}
}
