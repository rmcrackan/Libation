using DataLayer;
using Dinah.Core.Net.Http;
using FileLiberator;
using System;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
	public class ProcessBaseForm : StreamBaseForm
	{
		protected LogMe LogMe { get; private set; }
		public virtual void SetProcessable(IStreamable streamable, LogMe logMe)
		{
			LogMe = logMe;
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
			if (Streamable is IProcessable processable)
			{
				processable.Begin -= OnBegin;
				processable.Completed -= OnCompleted;
				processable.StatusUpdate -= OnStatusUpdate;
			}
		}

		#region IProcessable event handlers
		public virtual void OnBegin(object sender, LibraryBook libraryBook) => LogMe.Info($"Begin: {libraryBook.Book}");
		public virtual void OnStatusUpdate(object sender, string statusUpdate) => LogMe.Info("- " + statusUpdate);
		public virtual void OnCompleted(object sender, LibraryBook libraryBook) => LogMe.Info($"Completed: {libraryBook.Book}{Environment.NewLine}");
		#endregion
	}
}
