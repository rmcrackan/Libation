using System;
using System.Threading.Tasks;

namespace LibationUiBase
{
	// decouple serilog and form. include convenience factory method
	public class LogMe
	{
		public event EventHandler<string> LogInfo;
		public event EventHandler<string> LogErrorString;
		public event EventHandler<(Exception, string)> LogError;

		private LogMe()
		{
			LogInfo += (_, text) => Serilog.Log.Logger.Information($"Automated backup: {text}");
			LogErrorString += (_, text) => Serilog.Log.Logger.Error(text);
			LogError += (_, tuple) => Serilog.Log.Logger.Error(tuple.Item1, tuple.Item2 ?? "Automated backup: error");
		}
		private static ILogForm LogForm;
		public static LogMe RegisterForm<T>(T form) where T : ILogForm
		{
			var logMe = new LogMe();

			if (form is null)
				return logMe;

			LogForm = form;

			logMe.LogInfo += LogMe_LogInfo;
			logMe.LogErrorString += LogMe_LogErrorString;
			logMe.LogError += LogMe_LogError;

			return logMe;
		}

		private static async void LogMe_LogError(object sender, (Exception, string) tuple)
		{
			await Task.Run(() => LogForm?.WriteLine(tuple.Item2 ?? "Automated backup: error"));
			await Task.Run(() => LogForm?.WriteLine("ERROR: " + tuple.Item1.Message));
		}

		private static async void LogMe_LogErrorString(object sender, string text)
		{
			await Task.Run(() => LogForm?.WriteLine(text));
		}

		private static async void LogMe_LogInfo(object sender, string text)
		{
			await Task.Run(() => LogForm?.WriteLine(text));
		}

		public void Info(string text) => LogInfo?.Invoke(this, text);
		public void Error(string text) => LogErrorString?.Invoke(this, text);
		public void Error(Exception ex, string text = null) => LogError?.Invoke(this, (ex, text));
	}
}
