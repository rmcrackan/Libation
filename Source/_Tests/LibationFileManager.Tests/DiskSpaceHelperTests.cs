using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationFileManager.Tests;

[TestClass]
public class DiskSpaceHelperTests
{
	[TestMethod]
	public void IsDiskFullException_detects_message()
	{
		var ex = new IOException("There is not enough space on the disk. : 'C:\\temp\\x.aaxc'.");
		Assert.IsTrue(DiskSpaceHelper.IsDiskFullException(ex));
	}

	[TestMethod]
	public void IsDiskFullException_detects_message_in_aggregate()
	{
		var inner = new IOException("Failed to create file because the disk was full.");
		var ex = new AggregateException(inner);
		Assert.IsTrue(DiskSpaceHelper.IsDiskFullException(ex));
	}

	[TestMethod]
	public void ErrorMessageIndicatesDiskFull_matches_common_phrases()
	{
		Assert.IsTrue(DiskSpaceHelper.ErrorMessageIndicatesDiskFull("There is not enough space on the disk. : 'C:\\temp\\x.aaxc'."));
		Assert.IsFalse(DiskSpaceHelper.ErrorMessageIndicatesDiskFull("Unable to read beyond the end of the stream."));
	}
}
