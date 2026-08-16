using System;
using System.IO;
using LibationSearchEngine;
using Lucene.Net.Index;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchEngineTests;

[TestClass]
public class RecoverableCorruptIndexExceptionTests
{
	[TestMethod]
	public void CorruptIndexException_is_recoverable()
	{
		var ex = new CorruptIndexException("checksum mismatch in segments file");
		Assert.IsTrue(SearchEngine.IsRecoverableCorruptIndexException(ex));
	}

	[TestMethod]
	public void ReadPastEof_IOException_is_recoverable()
	{
		var ex = new IOException("read past EOF");
		Assert.IsTrue(SearchEngine.IsRecoverableCorruptIndexException(ex));
	}

	[TestMethod]
	public void ReadPastEof_IOException_is_recoverable_case_insensitive()
	{
		var ex = new IOException("Read Past Eof while filling buffer");
		Assert.IsTrue(SearchEngine.IsRecoverableCorruptIndexException(ex));
	}

	[TestMethod]
	public void CharacterInNumber_ArgumentException_is_recoverable()
	{
		var ex = new ArgumentException("Invalid or unsupported character in number");
		Assert.IsTrue(SearchEngine.IsRecoverableCorruptIndexException(ex));
	}

	[TestMethod]
	public void Unrelated_IOException_is_not_recoverable()
	{
		var ex = new IOException("The process cannot access the file because it is being used by another process.");
		Assert.IsFalse(SearchEngine.IsRecoverableCorruptIndexException(ex));
	}

	[TestMethod]
	public void Unrelated_Exception_is_not_recoverable()
	{
		var ex = new InvalidOperationException("something else");
		Assert.IsFalse(SearchEngine.IsRecoverableCorruptIndexException(ex));
	}
}
