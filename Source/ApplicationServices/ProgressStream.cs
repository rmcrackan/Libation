using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationServices;

/// <summary>
/// A stream wrapper that reports the number of bytes read to a callback.
/// Used to track upload streaming progress via HttpClient and MultipartFormDataContent.
/// </summary>
public sealed class ProgressStream : Stream
{
	private readonly Stream _inner;
	private readonly Action<int> _onBytesRead;

	public ProgressStream(Stream inner, Action<int> onBytesRead)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_onBytesRead = onBytesRead ?? throw new ArgumentNullException(nameof(onBytesRead));
	}

	public override bool CanRead => _inner.CanRead;
	public override bool CanSeek => _inner.CanSeek;
	public override bool CanWrite => _inner.CanWrite;
	public override long Length => _inner.Length;

	public override long Position
	{
		get => _inner.Position;
		set => _inner.Position = value;
	}

	public override void Flush() => _inner.Flush();
	public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

	public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
	public override void SetLength(long value) => _inner.SetLength(value);
	public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
	public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);
	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		=> _inner.WriteAsync(buffer, offset, count, cancellationToken);
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		=> _inner.WriteAsync(buffer, cancellationToken);

	public override int Read(byte[] buffer, int offset, int count)
	{
		var read = _inner.Read(buffer, offset, count);
		if (read > 0)
			_onBytesRead(read);
		return read;
	}

	public override int Read(Span<byte> buffer)
	{
		var read = _inner.Read(buffer);
		if (read > 0)
			_onBytesRead(read);
		return read;
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken);
		if (read > 0)
			_onBytesRead(read);
		return read;
	}

	public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
	{
		var read = await _inner.ReadAsync(buffer, cancellationToken);
		if (read > 0)
			_onBytesRead(read);
		return read;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_inner.Dispose();
		base.Dispose(disposing);
	}

	public override async ValueTask DisposeAsync()
	{
		await _inner.DisposeAsync();
		await base.DisposeAsync();
	}
}
