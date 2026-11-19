using ApplicationServices;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	public void Configure_Liberate() { }

	/// <summary> This gets called by the "Begin Book and PDF Backups" menu item. </summary>
	public async Task BackupAllBooks()
	{
		var books = await Task.Run(DbContexts.GetUnliberated_Flat_NoTracking);
		await BackupAllBooksAsync(books);
	}

	/// <param name="notifyIfNothingQueued">False for the automatic post-scan download, which runs unattended.</param>
	private async Task BackupAllBooksAsync(IEnumerable<LibraryBook> books, bool notifyIfNothingQueued = true)
	{
		try
		{
			var unliberated = books.UnLiberated().ToArray();

			if (await ProcessQueue.QueueDownloadDecryptAsync(unliberated, notifyIfNothingQueued: notifyIfNothingQueued))
				setQueueCollapseState(false);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error occurred while backing up all library books");
		}
	}

	/// <summary> This gets called by the "Begin PDF Only Backups" menu item. </summary>
	public async Task BackupAllPdfs()
	{
		var books = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking());
		if (await ProcessQueue.QueueDownloadPdfAsync(books))
			setQueueCollapseState(false);
	}

	public async Task ConvertAllToMp3Async()
	{
		var result = await MessageBox.Show(MainWindow,
			Strings.MainVM_ThisConvertsAllM4bTitlesIn,
			Strings.MainVM_ConvertAllM4bMp3,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning);
		if (result == DialogResult.Yes && await ProcessQueue.QueueConvertToMp3Async(await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking())))
			setQueueCollapseState(false);
	}

	private void setQueueCollapseState(bool collapsed)
	{
		QueueOpen = !collapsed;
		Configuration.Instance.SetNonString(!collapsed, nameof(QueueOpen));
	}
}
