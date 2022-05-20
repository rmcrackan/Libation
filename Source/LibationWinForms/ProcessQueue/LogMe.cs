using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.ProcessQueue
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

		public static LogMe RegisterForm<T>(T form) where T : ILogForm
		{
			var logMe = new LogMe();

			if (form is null)
				return logMe;

			logMe.LogInfo += (_, text) => form?.WriteLine(text);

			logMe.LogErrorString += (_, text) => form?.WriteLine(text);

			logMe.LogError += (_, tuple) =>
			{
				form?.WriteLine(tuple.Item2 ?? "Automated backup: error");
				form?.WriteLine("ERROR: " + tuple.Item1.Message);
			};

			return logMe;
		}

		public void Info(string text) => LogInfo?.Invoke(this, text);
		public void Error(string text) => LogErrorString?.Invoke(this, text);
		public void Error(Exception ex, string text = null) => LogError?.Invoke(this, (ex, text));
	}
}
