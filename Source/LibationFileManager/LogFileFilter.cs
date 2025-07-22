using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

#nullable enable
namespace LibationFileManager;

/// <summary>
/// Hooks the file sink to set the log file path for the LogFileFilter.
/// </summary>
public class FileSinkHook : FileLifecycleHooks
{
	public override Stream OnFileOpened(string path, Stream underlyingStream, Encoding encoding)
	{
		LogFileFilter.SetLogFilePath(path);
		return base.OnFileOpened(path, underlyingStream, encoding);
	}
}


/// <summary>
/// Identify log entries which are to be written to files, and save them to a zip file.
/// 
/// Files are detected by pattern matching. If the logged type has properties named 'filename' and 'filedata' (case insensitive)
/// with types string and byte[] respectively, the type is destructured and written to the log zip file.
/// 
/// The zip file's name will be derived from the active log file's name, with "_AdditionalFiles.zip" appended.
/// </summary>
public class LogFileFilter : IDestructuringPolicy
{
	private static readonly object lockObj = new();
	public static string? ZipFilePath { get; private set; }
	public static string? LogFilePath { get; private set; }
	public static void SetLogFilePath(string? logFilePath)
	{
		lock(lockObj)
		{
			(LogFilePath, ZipFilePath)
				= File.Exists(logFilePath) && Path.GetDirectoryName(logFilePath) is string logDir
				? (logFilePath, Path.Combine(logDir, $"{Path.GetFileNameWithoutExtension(logFilePath)}_AdditionalFiles.zip"))
				: (null, null);
		}
	}

	private static bool TrySaveLogFile(ref string filename, byte[] fileData, CompressionLevel compression)
	{
		try
		{
			lock (lockObj)
			{
				if (string.IsNullOrEmpty(ZipFilePath))
					return false;

				using var archive = new ZipArchive(File.Open(ZipFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8);
				filename = GetUniqueEntryName(archive, filename);

				var entry = archive.CreateEntry(filename, compression);
				using var entryStream = entry.Open();
				entryStream.Write(fileData);
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string GetUniqueEntryName(ZipArchive archive, string filename)
	{
		var entryFileName = filename;
		for (int i = 1; archive.Entries.Any(e => e.Name == entryFileName); i++)
		{
			entryFileName = $"{Path.GetFileNameWithoutExtension(filename)}_({i++}){Path.GetExtension(filename)}";
		}
		return entryFileName;
	}

	public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
	{
		if (value?.GetType().GetProperties() is PropertyInfo[] properties && properties.Length >= 2
			&& properties.FirstOrDefault(p => p.Name.Equals("filename", StringComparison.InvariantCultureIgnoreCase)) is PropertyInfo filenameProperty && filenameProperty.PropertyType == typeof(string)
			&& properties.FirstOrDefault(p => p.Name.Equals("fileData", StringComparison.InvariantCultureIgnoreCase)) is PropertyInfo fileDataProperty && fileDataProperty.PropertyType == typeof(byte[]))
		{
			var filename = filenameProperty.GetValue(value) as string;
			var fileData = fileDataProperty.GetValue(value) as byte[];

			if (filename != null && fileData != null && fileData.Length > 0)
			{
				var compressionProperty = properties.FirstOrDefault(f => f.PropertyType == typeof(CompressionLevel));
				var compression = compressionProperty?.GetValue(value) is CompressionLevel c ? c : CompressionLevel.Fastest;

				result
				= TrySaveLogFile(ref filename, fileData, compression)
				? propertyValueFactory.CreatePropertyValue($"Log file '{filename}' saved in {ZipFilePath}")
				: propertyValueFactory.CreatePropertyValue($"Log file '{filename}' could not be saved in {ZipFilePath ?? "<null_path>"}. File Contents = {Convert.ToBase64String(fileData)}");

				return true;
			}
		}

		result = null;
		return false;
	}
}
