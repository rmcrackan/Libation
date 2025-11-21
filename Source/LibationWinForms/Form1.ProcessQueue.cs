using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using LibationUiBase.GridView;
using LibationWinForms.ProcessQueue;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms
{
	public partial class Form1
	{
		int WidthChange = 0;
		private void Configure_ProcessQueue()
		{
			processBookQueue1.PopoutButton.Click += ProcessBookQueue1_PopOut;
			
			WidthChange = splitContainer1.Panel2.Width + splitContainer1.SplitterWidth;
			int width = this.Width;
			var coppalseState = Configuration.Instance.GetNonString(defaultValue: false, nameof(splitContainer1.Panel2Collapsed));
			SetQueueCollapseState(coppalseState);
			this.Width = width;
		}

		private void ProductsDisplay_LiberateClicked(object sender, LibraryBook[] libraryBooks)
		{
			try
			{
				if (processBookQueue1.ViewModel.QueueDownloadDecrypt(libraryBooks))
					SetQueueCollapseState(false);
				else if (libraryBooks.Length == 1 && libraryBooks[0].Book.AudioExists)
				{
					// liberated: open explorer to file
					var filePath = AudibleFileStorage.Audio.GetPath(libraryBooks[0].Book.AudibleProductId);
					if (!Go.To.File(filePath?.ShortPathName))
					{
						var suffix = string.IsNullOrWhiteSpace(filePath) ? "" : $":\r\n{filePath}";
						MessageBox.Show($"File not found" + suffix);
					}
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBooks);
			}
		}

		private void ProductsDisplay_LiberateSeriesClicked(object sender, SeriesEntry series)
		{
			try
			{
				Serilog.Log.Logger.Information("Begin backing up all {series} episodes", series.LibraryBook);

				if (processBookQueue1.ViewModel.QueueDownloadDecrypt(series.Children.Select(c => c.LibraryBook).UnLiberated().ToArray()))
					SetQueueCollapseState(false);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up {series} episodes", series.LibraryBook);
			}
		}

		private void ProductsDisplay_ConvertToMp3Clicked(object sender, LibraryBook[] libraryBooks)
		{
			try
			{
				if (processBookQueue1.ViewModel.QueueConvertToMp3(libraryBooks))
					SetQueueCollapseState(false);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while handling the stop light button click for {libraryBook}", libraryBooks);
			}
		}

		private void SetQueueCollapseState(bool collapsed)
		{
			if (collapsed && !splitContainer1.Panel2Collapsed)
			{
				WidthChange = splitContainer1.Panel2.Width + splitContainer1.SplitterWidth;
				splitContainer1.Panel2.Controls.Remove(processBookQueue1);
				splitContainer1.Panel2Collapsed = true;
				Width -= WidthChange;
			}
			else if (!collapsed && splitContainer1.Panel2Collapsed)
			{
				if (!processBookQueue1.PopoutButton.Visible)
					//Queue is in popout mode. Do nothing.
					return;

				Width += WidthChange;
				splitContainer1.Panel2.Controls.Add(processBookQueue1);
				splitContainer1.Panel2Collapsed = false;
				processBookQueue1.PopoutButton.Visible = true;
			}

			Configuration.Instance.SetNonString(splitContainer1.Panel2Collapsed, nameof(splitContainer1.Panel2Collapsed));
			toggleQueueHideBtn.Text = splitContainer1.Panel2Collapsed ? "❰❰❰" : "❱❱❱";
		}

		private void ToggleQueueHideBtn_Click(object sender, EventArgs e)
		{
			SetQueueCollapseState(!splitContainer1.Panel2Collapsed);
		}

		private void ProcessBookQueue1_PopOut(object sender, EventArgs e)
		{
			ProcessBookForm dockForm = new();
			dockForm.WidthChange = splitContainer1.Panel2.Width + splitContainer1.SplitterWidth;
			dockForm.RestoreSizeAndLocation(Configuration.Instance);
			dockForm.FormClosing += DockForm_FormClosing;
			splitContainer1.Panel2.Controls.Remove(processBookQueue1);
			splitContainer1.Panel2Collapsed = true;
			processBookQueue1.PopoutButton.Visible = false;
			dockForm.PassControl(processBookQueue1);
			dockForm.Show();
			this.Width -= dockForm.WidthChange;
			toggleQueueHideBtn.Visible = false;
			int deltax = filterBtn.Margin.Right + toggleQueueHideBtn.Width + toggleQueueHideBtn.Margin.Left;
			filterBtn.Location = new System.Drawing.Point(filterBtn.Location.X + deltax, filterBtn.Location.Y);
			filterSearchTb.Location = new System.Drawing.Point(filterSearchTb.Location.X + deltax, filterSearchTb.Location.Y);
		}

		private void DockForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (sender is ProcessBookForm dockForm)
			{
				this.Width += dockForm.WidthChange;
				splitContainer1.Panel2.Controls.Add(dockForm.RegainControl());
				splitContainer1.Panel2Collapsed = false;
				processBookQueue1.PopoutButton.Visible = true;
				dockForm.SaveSizeAndLocation(Configuration.Instance);
				this.Focus();
				toggleQueueHideBtn.Visible = true;
				int deltax = filterBtn.Margin.Right + toggleQueueHideBtn.Width + toggleQueueHideBtn.Margin.Left;
				filterBtn.Location = new System.Drawing.Point(filterBtn.Location.X - deltax, filterBtn.Location.Y);
				filterSearchTb.Location = new System.Drawing.Point(filterSearchTb.Location.X - deltax, filterSearchTb.Location.Y);
			}
		}
	}
}
