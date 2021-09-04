using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DtoImporterService
{
	public record timeLogEntry(string msg, long totalElapsed, long delta);
	public static class PerfLogger
	{
		private static Stopwatch sw = new Stopwatch();
		private static List<timeLogEntry> __log { get; } = new List<timeLogEntry> { new("begin", 0, 0) };

		public static void logTime(string s)
		{
			var totalElapsed = sw.ElapsedMilliseconds;

			var prev = __log.Last().totalElapsed;
			var delta = totalElapsed - prev;

			__log.Add(new(s, totalElapsed, delta));
		}
		public static void logRestart()
		{
			__log.Clear();
			__log.Add(new("begin", 0, 0));
			sw.Restart();
		}
		public static void stop() => sw.Stop();
		public static string logOutput =>
			$"{nameof(timeLogEntry.msg)}\t{nameof(timeLogEntry.totalElapsed)}\t{nameof(timeLogEntry.delta)}\r\n"
			+ __log.Select(t => $"{t.msg}\t{t.totalElapsed}\t{t.delta}").Aggregate((a, b) => $"{a}\r\n{b}");
	}
}
