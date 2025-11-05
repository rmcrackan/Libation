using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace FileManager
{
	/// <summary>
	/// Tracks actual locations of files.
	/// </summary>
	public class BackgroundFileSystem : IDisposable
    {
        public LongPath RootDirectory { get; private set; }
        public string SearchPattern { get; private set; }
        public SearchOption SearchOption { get; private set; }

        private FileSystemWatcher? fileSystemWatcher { get; set; }
        private BlockingCollection<FileSystemEventArgs>? directoryChangesEvents { get; set; }
        private Task? backgroundScanner { get; set; }

        private Lock fsCacheLocker { get; } = new();
        private List<LongPath> fsCache { get; } = new();

        public BackgroundFileSystem(LongPath rootDirectory, string searchPattern, SearchOption searchOptions)
        {
            RootDirectory = rootDirectory;
            SearchPattern = searchPattern;
            SearchOption = searchOptions;

            Init();
        }

        public LongPath? FindFile(System.Text.RegularExpressions.Regex regex)
        {
            lock (fsCacheLocker)
                return fsCache.FirstOrDefault(s => regex.IsMatch(s));
        }

        public List<LongPath> FindFiles(System.Text.RegularExpressions.Regex regex)
        {
            lock (fsCacheLocker)
                return fsCache.Where(s => regex.IsMatch(s)).ToList();
        }

        public void RefreshFiles()
        {
            lock (fsCacheLocker)
            {
                fsCache.Clear();
                fsCache.AddRange(SafestEnumerateFiles(RootDirectory));
            }
        }

        private void Init()
        {
            Stop();

            lock (fsCacheLocker)
                fsCache.AddRange(SafestEnumerateFiles(RootDirectory));

            directoryChangesEvents = new BlockingCollection<FileSystemEventArgs>();
			fileSystemWatcher = new FileSystemWatcher(RootDirectory)
			{
				IncludeSubdirectories = true,
				EnableRaisingEvents = true
			};
			fileSystemWatcher.Created += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Changed;
            fileSystemWatcher.Error += FileSystemWatcher_Error;

            backgroundScanner = new Task(BackgroundScanner);
            backgroundScanner.Start();
        }
        private void Stop()
        {
            //Stop raising events
            fileSystemWatcher?.Dispose();

            try
            {
                //Calling CompleteAdding() will cause background scanner to terminate.
                directoryChangesEvents?.CompleteAdding();
            }
            // if directoryChangesEvents is non-null and isDisposed, this exception is thrown. there's no other way to check >:(
            catch (ObjectDisposedException) { }

            //Wait for background scanner to terminate before reinitializing.
            backgroundScanner?.Wait();

            //Dispose of directoryChangesEvents after backgroundScanner exists.
            directoryChangesEvents?.Dispose();

            lock (fsCacheLocker)
                fsCache.Clear();
        }

        private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            Init();
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            directoryChangesEvents?.Add(e);
        }

        #region Background Thread
        private void BackgroundScanner()
        {
            while (directoryChangesEvents?.TryTake(out var change, -1) is true)
            {
                lock (fsCacheLocker)
                    UpdateLocalCache(change);
            }
        }

        private void UpdateLocalCache(FileSystemEventArgs change)
        {
            if (change.ChangeType == WatcherChangeTypes.Deleted)
            {
                RemovePath(change.FullPath);
            }
            else if (change.ChangeType == WatcherChangeTypes.Created)
            {
                AddPath(change.FullPath);
            }
            else if (change.ChangeType == WatcherChangeTypes.Renamed && change is RenamedEventArgs renameChange)
            {
                RemovePath(renameChange.OldFullPath);
                AddPath(renameChange.FullPath);
            }
        }

        private void RemovePath(LongPath path)
        {
            path = path.LongPathName;
            var pathsToRemove = fsCache.Where(p => ((string)p).StartsWith(path)).ToArray();

            foreach (var p in pathsToRemove)
                fsCache.Remove(p);
        }

        private void AddPath(LongPath path)
        {
            path = path.LongPathName;
            //Temporary files created when updating the db will disappear before their attributes can be read.
            if (Path.GetFileName(path).Contains("LibationContext.db") || !File.Exists(path) && !Directory.Exists(path))
                return;
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                AddUniqueFiles(SafestEnumerateFiles(path));
            else
                AddUniqueFile(path);
        }

        private IEnumerable<LongPath> SafestEnumerateFiles(string path)
        {
            try
            {
                return FileUtility.SaferEnumerateFiles(path, SearchPattern, SearchOption);
            }
            catch
            {
                return [];
            }
        }

        private void AddUniqueFiles(IEnumerable<LongPath> newFiles)
        {
            foreach (var file in newFiles)
                AddUniqueFile(file);
        }

        private void AddUniqueFile(LongPath newFile)
        {
            if (!fsCache.Contains(newFile))
                fsCache.Add(newFile);
        }

		#endregion

		public void Dispose()
		{
			Stop();
			GC.SuppressFinalize(this);
		}
	}
}
