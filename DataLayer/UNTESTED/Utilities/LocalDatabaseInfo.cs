using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer.Utilities
{
	public static class LocalDatabaseInfo
	{
		public static List<string> GetLocalDBInstances()
		{
			// Start the child process.
			using var p = new System.Diagnostics.Process
			{
				StartInfo = new System.Diagnostics.ProcessStartInfo
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					FileName = "cmd.exe",
					Arguments = "/C sqllocaldb info",
					CreateNoWindow = true,
					WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
				}
			};
			p.Start();
			var output = p.StandardOutput.ReadToEnd();
			p.WaitForExit();

			// if LocalDb is not installed then it will return that 'sqllocaldb' is not recognized as an internal or external command operable program or batch file
			return string.IsNullOrWhiteSpace(output) || output.Contains("not recognized")
				? new List<string>()
				: output
					.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
					.Select(i => i.Trim())
					.Where(i => !string.IsNullOrEmpty(i))
					.ToList();
		}
	}
}
