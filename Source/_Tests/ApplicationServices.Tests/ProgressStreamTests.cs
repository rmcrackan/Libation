using ApplicationServices;
using AssertionHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ApplicationServices.Tests;

[TestClass]
public class ProgressStreamTests
{
	[TestMethod]
	public void Read_reports_bytes_read_to_callback()
	{
		var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
		using var inner = new MemoryStream(data);
		long reportedBytes = 0;
		using var sut = new ProgressStream(inner, count => reportedBytes += count);

		var buffer = new byte[4];
		var read = sut.Read(buffer, 0, 4);

		read.Should().Be(4);
		reportedBytes.Should().Be(4);

		read = sut.Read(buffer, 0, 4);
		read.Should().Be(4);
		reportedBytes.Should().Be(8);
	}

	[TestMethod]
	public async Task ReadAsync_reports_bytes_read_to_callback()
	{
		var data = new byte[] { 10, 20, 30, 40, 50 };
		using var inner = new MemoryStream(data);
		long reportedBytes = 0;
		using var sut = new ProgressStream(inner, count => reportedBytes += count);

		var buffer = new byte[3];
		var read = await sut.ReadAsync(buffer, 0, 3);

		read.Should().Be(3);
		reportedBytes.Should().Be(3);

		read = await sut.ReadAsync(buffer, 0, 3);
		read.Should().Be(2);
		reportedBytes.Should().Be(5);
	}

	[TestMethod]
	public async Task CopyToAsync_reports_total_bytes_copied()
	{
		var data = new byte[1024];
		Random.Shared.NextBytes(data);
		using var inner = new MemoryStream(data);
		long reportedBytes = 0;
		using var sut = new ProgressStream(inner, count => reportedBytes += count);

		using var destination = new MemoryStream();
		await sut.CopyToAsync(destination);

		destination.ToArray().Should().BeEquivalentTo(data);
		reportedBytes.Should().Be(1024);
	}

	[TestMethod]
	public void Properties_delegate_to_inner_stream()
	{
		var data = new byte[100];
		using var inner = new MemoryStream(data);
		using var sut = new ProgressStream(inner, _ => { });

		sut.CanRead.Should().BeTrue();
		sut.CanSeek.Should().BeTrue();
		sut.CanWrite.Should().BeTrue();
		sut.Length.Should().Be(100);
		sut.Position.Should().Be(0);

		sut.Position = 50;
		inner.Position.Should().Be(50);
	}

	[TestMethod]
	public void Dispose_disposes_inner_stream()
	{
		var inner = new MemoryStream(new byte[10]);
		var sut = new ProgressStream(inner, _ => { });

		sut.Dispose();

		Assert.ThrowsExactly<ObjectDisposedException>(() => inner.ReadByte());
	}
}
