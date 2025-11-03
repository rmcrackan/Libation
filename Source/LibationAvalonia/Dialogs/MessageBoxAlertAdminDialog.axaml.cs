using Avalonia.Controls;
using Dinah.Core;
using FileManager;
using LibationUiBase.Forms;
using System;

namespace LibationAvalonia.Dialogs
{
	public partial class MessageBoxAlertAdminDialog : DialogWindow
	{
		public string ErrorDescription { get; set; } = "[Error message]\n[Error message]\n[Error message]";
		public string ExceptionMessage { get; set; } = "EXCEPTION MESSAGE!";

		public MessageBoxAlertAdminDialog()
		{
			InitializeComponent();
			ControlToFocusOnShow = this.FindControl<Button>(nameof(OkButton));

			if (Design.IsDesignMode)
				DataContext = this;
		}

		public MessageBoxAlertAdminDialog(string text, string caption, Exception exception) : this()
		{
			ErrorDescription = text;
			this.Title = caption;
			ExceptionMessage = $"{exception.Message}\r\n\r\n{exception.StackTrace}";
			DataContext = this;
		}

		private async void GoToGithub_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		{
			var url = "https://github.com/rmcrackan/Libation/issues";
			try
			{
				Go.To.Url(url);
			}
			catch
			{
				await MessageBox.Show(this, $"Error opening url\r\n{url}", "Error opening url", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void GoToLogs_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		{
			try
			{
				Go.To.File(LibationFileManager.LogFileFilter.LogFilePath);
			}
			catch
			{
				LongPath dir = "";
				try
				{
					dir = LibationFileManager.Configuration.Instance.LibationFiles;
					Go.To.Folder(dir.ShortPathName);
				}
				catch
				{
					await MessageBox.Show(this, $"Error opening folder\r\n{dir}", "Error opening folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}		
		}

		public void OkButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			SaveAndClose();
		}
	}
}
