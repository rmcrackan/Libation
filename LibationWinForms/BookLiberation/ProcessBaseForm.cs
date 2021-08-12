using DataLayer;
using Dinah.Core.Net.Http;
using FileLiberator;
using System;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
	public class ProcessBaseForm : StreamBaseForm
	{
		protected Action<string> InfoLogAction { get; private set; }
		public virtual void SetProcessable(IStreamable streamable, Action<string> infoLog)
		{
			InfoLogAction = infoLog;
			SetStreamable(streamable);

			if (Streamable is not null && Streamable is IProcessable processable)
			{
				OnUnsubscribeAll(this, EventArgs.Empty);

				processable.Begin += OnBegin;
				processable.Completed += OnCompleted;
				processable.StatusUpdate += OnStatusUpdate;
				Disposed += OnUnsubscribeAll;
			}
		}

		private void OnUnsubscribeAll(object sender, EventArgs e)
		{
			Disposed -= OnUnsubscribeAll;
			if (Streamable is not null && Streamable is IProcessable processable)
			{
				processable.Begin -= OnBegin;
				processable.Completed -= OnCompleted;
				processable.StatusUpdate -= OnStatusUpdate;
			}
		}

		#region IProcessable event handlers
		public virtual void OnBegin(object sender, LibraryBook libraryBook) => InfoLogAction($"Begin: {libraryBook.Book}");
		public virtual void OnStatusUpdate(object sender, string statusUpdate) => InfoLogAction("- " + statusUpdate);
		public virtual void OnCompleted(object sender, LibraryBook libraryBook) => InfoLogAction($"Completed: {libraryBook.Book}{Environment.NewLine}");
		#endregion
	}
}
