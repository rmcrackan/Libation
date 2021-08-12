using DataLayer;
using FileLiberator;
using System;

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
				OnUnsubscribeAll(this, null);

				processable.Begin += OnBegin;
				processable.Completed += OnCompleted;
				processable.StatusUpdate += OnStatusUpdate;

				//Don't unsubscribe from Dispose because it fires on
				//IStreamable.StreamingCompleted, and the IProcessable
				//events need to live past that event.
				processable.Completed += OnUnsubscribeAll;
			}
		}

		private void OnUnsubscribeAll(object sender, LibraryBook e)
		{
			if (Streamable is IProcessable processable)
			{
				processable.Completed -= OnUnsubscribeAll;
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
