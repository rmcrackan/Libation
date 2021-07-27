using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileManager
{
    class BackgroundFileSystem
    {
        public string RootDirectory { get; private set; }
        public string SearchPattern { get; private set; }
        public SearchOption SearchOption { get; private set; }

        private FileSystemWatcher fileSystemWatcher { get; set; }
        private BlockingCollection<FileSystemEventArgs> directoryChangesEvents { get; set; }
        private Task backgroundScanner { get; set; }
        private List<string> fsCache { get; set; }

        public string FindFile(string regexPattern, RegexOptions options)
        {
            lock (fsCache)
            {
                return fsCache.FirstOrDefault(s => Regex.IsMatch(s, regexPattern, options));
            }
        }

        public void Init(string rootDirectory, string searchPattern, SearchOption searchOptions)
        {
            RootDirectory = rootDirectory;
            SearchPattern = searchPattern;
            SearchOption = searchOptions;

            //Calling CompleteAdding() will cause background scanner to terminate.
            directoryChangesEvents?.CompleteAdding();
            fsCache?.Clear();
            directoryChangesEvents?.Dispose();
            fileSystemWatcher?.Dispose();

            fsCache = Directory.EnumerateFiles(RootDirectory, SearchPattern, SearchOption).ToList();

            directoryChangesEvents = new BlockingCollection<FileSystemEventArgs>();
            fileSystemWatcher = new FileSystemWatcher(RootDirectory);
            fileSystemWatcher.Created += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Changed;
            fileSystemWatcher.Error += FileSystemWatcher_Error;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.EnableRaisingEvents = true;

            //Wait for background scanner to terminate before reinitializing.
            backgroundScanner?.Wait();
            backgroundScanner = new Task(BackgroundScanner);
            backgroundScanner.Start();
        }

        private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            Init(RootDirectory, SearchPattern, SearchOption);
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            directoryChangesEvents.Add(e);
        }

        #region Background Thread
        private void BackgroundScanner()
        {
            while (directoryChangesEvents.TryTake(out FileSystemEventArgs change, -1))
                UpdateLocalCache(change);
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
            else if (change.ChangeType == WatcherChangeTypes.Renamed)
            {
                var renameChange = change as RenamedEventArgs;

                RemovePath(renameChange.OldFullPath);
                AddPath(renameChange.FullPath);
            }
        }

        private void RemovePath(string path)
        {
            var pathsToRemove = fsCache.Where(p => p.StartsWith(path)).ToArray();

            foreach (var p in pathsToRemove)
                fsCache.Remove(p);
        }

        private void AddPath(string path)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                fsCache.AddRange(Directory.EnumerateFiles(path, SearchPattern, SearchOption));
            else
                fsCache.Add(path);
        }

        #endregion
    }
}
