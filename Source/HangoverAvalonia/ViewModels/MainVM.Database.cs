using HangoverBase;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace HangoverAvalonia.ViewModels;

public partial class MainVM
{
	private DatabaseTab _tab;

	private string _databaseFileText;
	private bool _databaseFound;
	private string _sqlResults;
	private string _duplicateResults;
	private string _duplicateAsinStatusText;
	private bool _canRemoveDuplicateAsins;
	private bool _confirmRemoveDuplicateAsins;

	public string DatabaseFileText { get => _databaseFileText; set => this.RaiseAndSetIfChanged(ref _databaseFileText, value); }
	public string SqlQuery { get; set; }
	public bool DatabaseFound { get => _databaseFound; set => this.RaiseAndSetIfChanged(ref _databaseFound, value); }
	public string SqlResults { get => _sqlResults; set => this.RaiseAndSetIfChanged(ref _sqlResults, value); }
	public string DuplicateResults { get => _duplicateResults; set => this.RaiseAndSetIfChanged(ref _duplicateResults, value); }
	public string DuplicateAsinStatusText { get => _duplicateAsinStatusText; set => this.RaiseAndSetIfChanged(ref _duplicateAsinStatusText, value); }
	public bool CanRemoveDuplicateAsins { get => _canRemoveDuplicateAsins; set => this.RaiseAndSetIfChanged(ref _canRemoveDuplicateAsins, value); }
	public bool ConfirmRemoveDuplicateAsins
	{
		get => _confirmRemoveDuplicateAsins;
		set => this.RaiseAndSetIfChanged(ref _confirmRemoveDuplicateAsins, value);
	}

	private void Load_databaseVM()
	{
		_tab = new(new DatabaseTabCommands(() => SqlQuery, s => SqlResults += s, s => SqlResults = s, s => DuplicateResults = s));

		_tab.LoadDatabaseFile();
		if (_tab.DbFile is null)
		{
			DatabaseFileText = $"Database file not found";
			DuplicateAsinStatusText = "Duplicate ASIN cleanup unavailable (database not found).";
			DatabaseFound = false;
			CanRemoveDuplicateAsins = false;
			return;
		}

		DatabaseFileText = $"Database file: {_tab.DbFile}";
		DatabaseFound = true;
		RefreshDuplicateAsinStatus();
	}

	public async Task ExecuteQueryAsync()
	{
		if (HangoverDbMutation.IsMutatingSql(SqlQuery)
			&& ConfirmDbMutationAsync is not null
			&& !await ConfirmDbMutationAsync(HangoverDbMutation.SqlMutatingDescription))
			return;

		_tab.ExecuteQuery();
	}

	public void RefreshDuplicateAsinStatus()
	{
		DuplicateAsinStatusText = _tab.GetDuplicateAsinStatusText();
		CanRemoveDuplicateAsins = _tab.CanRemoveDuplicateAsins();
		if (!CanRemoveDuplicateAsins)
			ConfirmRemoveDuplicateAsins = false;
	}

	public void ScanDuplicateAsins()
	{
		_tab.ScanDuplicateAsins();
		RefreshDuplicateAsinStatus();
	}

	public async Task RemoveDuplicateAsinsAsync()
	{
		if (!ConfirmRemoveDuplicateAsins)
			return;

		if (ConfirmDbMutationAsync is not null
			&& !await ConfirmDbMutationAsync(HangoverDbMutation.RemoveDuplicateAsinsDescription))
			return;

		_tab.RemoveDuplicateAsins();
		ConfirmRemoveDuplicateAsins = false;
		RefreshDuplicateAsinStatus();
	}
}
