using System;
using System.IO;

namespace LibationCli;

internal class ConsoleProgressBar
{
	public TextWriter Output { get; }
	public int MaxWidth { get; }
	public char ProgressChar { get; }
	public char NoProgressChar { get; }

	public double? Progress
	{
		get => field;
		set
		{
			field = value ?? 0;
			WriteProgress();
		}
	}

	public TimeSpan RemainingTime
	{
		get => m_RemainingTime;
		set
		{
			m_RemainingTime = value;
			WriteProgress();
		}
	}

	private TimeSpan m_RemainingTime;
	private int m_LastWriteLength = 0;
	private const int MAX_ETA_LEN = 10;
	private readonly int m_NumProgressPieces;

	public ConsoleProgressBar(
		TextWriter output,
		int maxWidth = 80,
		char progressCharr = '#',
		char noProgressChar = '.')
	{
		Output = output;
		MaxWidth = maxWidth;
		ProgressChar = progressCharr;
		NoProgressChar = noProgressChar;
		m_NumProgressPieces = MaxWidth - MAX_ETA_LEN - 4;
	}

	private void WriteProgress()
	{
		var numCompleted = (int)Math.Round(double.Min(100, Progress ?? 0) * m_NumProgressPieces / 100);
		var numRemaining = m_NumProgressPieces - numCompleted;
		var progressBar = $"[{new string(ProgressChar, numCompleted)}{new string(NoProgressChar, numRemaining)}]  ";

		progressBar += RemainingTime.TotalMinutes > 1000
			? "ETA ∞"
			: $"ETA {(int)RemainingTime.TotalMinutes}:{RemainingTime.Seconds:D2}";

		Output.Write(new string('\b', m_LastWriteLength) + progressBar);
		if (progressBar.Length < m_LastWriteLength)
		{
			var extra = m_LastWriteLength - progressBar.Length;
			Output.Write(new string(' ', extra) + new string('\b', extra));
		}
		m_LastWriteLength = progressBar.Length;
	}

	public void Clear()
		=> Output.Write(
			new string('\b', m_LastWriteLength) +
			new string(' ', m_LastWriteLength) +
			new string('\b', m_LastWriteLength));
}
