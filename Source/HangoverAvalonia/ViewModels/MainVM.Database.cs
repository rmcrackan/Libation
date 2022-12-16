using HangoverBase;
using ReactiveUI;

namespace HangoverAvalonia.ViewModels
{
	public partial class MainVM
	{
		private DatabaseTab _tab;

		private string _databaseFileText;
		private bool _databaseFound;
		private string _sqlResults;
		public string DatabaseFileText { get => _databaseFileText; set => this.RaiseAndSetIfChanged(ref _databaseFileText, value); }
		public string SqlQuery { get; set; }
		public bool DatabaseFound { get => _databaseFound; set => this.RaiseAndSetIfChanged(ref _databaseFound, value); }
		public string SqlResults { get => _sqlResults; set => this.RaiseAndSetIfChanged(ref _sqlResults, value); }

		private void Load_databaseVM()
		{
			_tab = new(new(() => SqlQuery, s => SqlResults = s, s => SqlResults = s));

			_tab.LoadDatabaseFile();
			if (_tab.DbFile is null)
			{
				DatabaseFileText = $"Database file not found";
				DatabaseFound = false;
				return;
			}

			DatabaseFileText = $"Database file: {_tab.DbFile}";
			DatabaseFound = true;
		}

		public void ExecuteQuery() => _tab.ExecuteQuery();
	}
}
