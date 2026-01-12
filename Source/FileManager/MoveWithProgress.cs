﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Serilog;

#nullable enable
namespace FileManager;

public class MoveFileProgressEventArgs : EventArgs
{
	public long TotalFileSize { get; }
	public long TotalBytesTransferred { get; }
	public long BytesMoved { get; }
	public bool Continue { get; set; } = true;

	internal MoveFileProgressEventArgs(long bytesMoved, long totalBytesTransferred, long totalFileSize)
	{
		BytesMoved = bytesMoved;
		TotalBytesTransferred = totalBytesTransferred;
		TotalFileSize = totalFileSize;
	}
}

public class MoveWithProgress
{
	public event EventHandler<MoveFileProgressEventArgs>? MoveProgress;

	public async Task<bool> MoveAsync(LongPath source, LongPath destination, bool overwrite = false, CancellationToken cancellation = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));
		ArgumentException.ThrowIfNullOrEmpty(destination, nameof(destination));
		var sourceFileInfo = new FileInfo(source);

		if (!sourceFileInfo.Exists)
			throw new FileNotFoundException($"Source file '{source}' does not exist.", source);

		var destinationFile = new FileInfo(destination);
		var sourceDevice = GetDeviceId(sourceFileInfo);
		var destinationDevice = GetDeviceId(destinationFile.Directory);

		if (sourceDevice == destinationDevice)
		{
			File.Move(sourceFileInfo.FullName, destinationFile.FullName, overwrite);
			MoveProgress?.Invoke(this, new MoveFileProgressEventArgs(destinationFile.Length, destinationFile.Length, sourceFileInfo.Length));
			return true;
		}

		if (destinationFile.Exists && !overwrite)
			throw new IOException("The file exists.");

		bool success = false;
		try
		{
			success = await CopyWithProgressAsync(sourceFileInfo, destinationFile, cancellation);
		}
		finally
		{
			if (success)
				FileUtility.SaferDelete(sourceFileInfo.FullName);
			else
				FileUtility.SaferDelete(destinationFile.FullName);
		}
		return success;
	}

	private static string? GetDeviceId(FileSystemInfo? fsEntry)
		=> fsEntry?.FullName is not string path ? null
		: LongPath.IsWindows ? GetDriveSerialNumber(path)
		: LongPath.IsOSX ? RunShellCommand("stat -L -f %d \"" + path + "\"")
		: RunShellCommand("stat -L -f -c %d \"" + path + "\"");

	private async Task<bool> CopyWithProgressAsync(FileInfo sourceFileInfo, FileInfo destinationFile, CancellationToken cancellation)
	{
		const int BlockSizeMb = 8;
		const int BlockSizeBytes = BlockSizeMb * (1 << 20);
		using FileStream sourceStream = sourceFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		using FileStream destinationStream = destinationFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

		byte[] readBuff = ArrayPool<byte>.Shared.Rent(BlockSizeBytes);
		byte[] writeBuff = ArrayPool<byte>.Shared.Rent(BlockSizeBytes);

		long totalCopied = 0, bytesMovedSinceLastReport = 0;
		DateTime nextReport = default;
		try
		{
			int bytesRead = await sourceStream.ReadAsync(writeBuff, 0, BlockSizeBytes, cancellation);
			while (bytesRead > 0)
			{
				totalCopied += bytesRead;
				bytesMovedSinceLastReport += bytesRead;

				var readTask = sourceStream.ReadAsync(readBuff, 0, BlockSizeBytes, cancellation);
				await destinationStream.WriteAsync(writeBuff, 0, bytesRead, cancellation);

				if (DateTime.UtcNow >= nextReport)
				{
					var args = new MoveFileProgressEventArgs(bytesMovedSinceLastReport, totalCopied, sourceFileInfo.Length);
					bytesMovedSinceLastReport = 0;
					MoveProgress?.Invoke(this, args);
					if (!args.Continue)
						break;
					nextReport = DateTime.UtcNow.AddMilliseconds(200.0);
				}
				bytesRead = await readTask;
				(readBuff, writeBuff) = (writeBuff, readBuff);
			}

			destinationStream.SetLength(totalCopied);
			MoveProgress?.Invoke(this, new MoveFileProgressEventArgs(bytesMovedSinceLastReport, totalCopied, sourceFileInfo.Length));
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(readBuff);
			ArrayPool<byte>.Shared.Return(writeBuff);
		}
		return totalCopied == sourceFileInfo.Length;
	}

	private static string? RunShellCommand(string command)
	{
		var psi = new ProcessStartInfo
		{
			FileName = "/bin/sh",
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true,
			ArgumentList = { "-c", command }
		};
		try
		{
			var proc = Process.Start(psi);
			proc?.WaitForExit();
			return proc?.StandardOutput?.ReadToEnd()?.Trim();
		}
		catch (Exception e)
		{
			Log.Logger.Error(e, "Failed to run shell command. {@Arguments}", psi.ArgumentList);
			return null;
		}
	}

	private static string? GetDriveSerialNumber(string path)
	{
		const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
		const uint OPEN_EXISTING = 3;
		var handle = CreateFile(path, FileAccess.Read, FileShare.Read, 0, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
		if (handle.IsInvalid)
			return null;

		try
		{
			BY_HANDLE_FILE_INFORMATION info = default;
			if (!GetFileInformationByHandle(handle, ref info))
			{
				return null;
			}
			return info.dwVolumeSerialNumber.ToString("x8");
		}
		finally
		{
			handle.Close();
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, ref BY_HANDLE_FILE_INFORMATION lpFileInformation);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateFile(string fileName, FileAccess fileAccess, FileShare fileShare, nint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, nint hTemplateFile);

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct BY_HANDLE_FILE_INFORMATION
	{
		private uint dwFileAttributes;
		private long ftCreationTime;
		private long ftLastAccessTime;
		private long ftLastWriteTime;
		public uint dwVolumeSerialNumber;
		private uint nFileSizeHigh;
		private uint nFileSizeLow;
		private uint nNumberOfLinks;
		private uint nFileIndexHigh;
		private uint nFileIndexLow;
	}
}