using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ffmpeg_decrypt
{
	public static class RCrack
	{
		public static async Task<string> GetChecksum(string aaxPath)
		{
			Resources.Extract("ffprobe.exe");

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(Resources.resdir, "ffprobe.exe"),
				Arguments = aaxPath.SurroundWithQuotes(),
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				WorkingDirectory = Directory.GetCurrentDirectory()
			};

			using var ffp = new Process { StartInfo = startInfo };
			ffp.Start();

			// checksum is in the debug info. ffprobe's debug info is written to stderr, not stdout
			var ffprobeStderr = ffp.StandardError.ReadToEnd();

			await ffp.WaitForExitAsync();

			ffp.Close();

			// example checksum line:
			// ... [aax] file checksum == 0c527840c4f18517157eb0b4f9d6f9317ce60cd1
			var checksum = ffprobeStderr.ExtractString("file checksum == ", 40);

			return checksum;
		}

		/// <summary>use checksum to get activation bytes. activation bytes are unique per audible customer. only have to do this 1x/customer</summary>
		public static async Task<string> GetActivationBytes(string checksum)
		{
			Resources.Extract("rcrack.exe");

			Resources.Extract("alglib1.dll");
			// RainbowCrack files to recover your own Audible activation data (activation_bytes) in an offline manner
			Resources.Extract("audible_byte#4-4_0_10000x789935_0.rtc");
			Resources.Extract("audible_byte#4-4_1_10000x791425_0.rtc");
			Resources.Extract("audible_byte#4-4_2_10000x790991_0.rtc");
			Resources.Extract("audible_byte#4-4_3_10000x792120_0.rtc");
			Resources.Extract("audible_byte#4-4_4_10000x790743_0.rtc");
			Resources.Extract("audible_byte#4-4_5_10000x790568_0.rtc");
			Resources.Extract("audible_byte#4-4_6_10000x791458_0.rtc");
			Resources.Extract("audible_byte#4-4_7_10000x791707_0.rtc");
			Resources.Extract("audible_byte#4-4_8_10000x790202_0.rtc");
			Resources.Extract("audible_byte#4-4_9_10000x791022_0.rtc");

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(Resources.resdir, "rcrack.exe"),
				Arguments = @". -h " + checksum,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WorkingDirectory = Directory.GetCurrentDirectory()
			};

			using var rcr = new Process { StartInfo = startInfo };
			rcr.Start();

			var rcrackStdout = rcr.StandardOutput.ReadToEnd();

			await rcr.WaitForExitAsync();
			rcr.Close();

			// example result
			// 0c527840c4f18517157eb0b4f9d6f9317ce60cd1  \xbd\x89X\x09  hex:bd895809
			var activation_bytes = rcrackStdout.ExtractString("hex:", 8);

			return activation_bytes;
		}
	}
}
