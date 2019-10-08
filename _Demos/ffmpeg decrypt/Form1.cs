using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ffmpeg_decrypt
{
	public partial class Form1 : Form
	{
		public static string resdir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res");

		public Form1()
		{
			InitializeComponent();

			qualityCombo.SelectedIndex = 1; // value == 80
			statuslbl.Text = "";
			decryptRb.Checked = true;
		}

		private void inpbutton_Click(object sender, EventArgs e)
		{
			using var ofd = new OpenFileDialog { Filter = "Audible Audio Files|*.aax", Title = "Select an Audible Audio File", FileName = "" };
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				inputdisplay.Text = ofd.FileName;
				outputdisplay.Text = Path.GetDirectoryName(ofd.FileName);
				convertbutton.Enabled = true;
			}
		}

		private void outpbutton_Click(object sender, EventArgs e)
		{
			using var fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				outputdisplay.Text = fbd.SelectedPath;
		}

		private async void convertbutton_Click(object sender, EventArgs e)
		{
			//var sw = Stopwatch.StartNew();

			// disable UI
			inputPnl.Enabled = false;

			statuslbl.Text = "Getting File Hash...";
			var checksum = await GetChecksum(inputdisplay.Text);

			statuslbl.Text = "Cracking Activation Bytes...";
			var activation_bytes = await GetActivationBytes(checksum);

			statuslbl.Text = "Converting File...";
			var encodeTo
				= decryptRb.Checked ? EncodeTo.DecryptOnly
				: rmp3.Checked ? EncodeTo.Mp3
				: raac.Checked ? EncodeTo.M4b
				: rflac.Checked ? EncodeTo.Flac
				: throw new NotImplementedException();
			await decryptAndSaveFile(activation_bytes, inputdisplay.Text, outputdisplay.Text, txtConsole, encodeTo, int.Parse(qualityCombo.Text));

			// re-enable UI
			inputPnl.Enabled = true;

			//sw.Stop();
			//var total = (int)sw.Elapsed.TotalSeconds;

			statuslbl.Text = "Conversion Complete!";
		}

		private static async Task<string> GetChecksum(string aaxPath)
		{
			Extract("ffprobe.exe");

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(resdir, "ffprobe.exe"),
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

			await Task.Run(() => ffp.WaitForExit());

			ffp.Close();

			// example checksum line:
			// ... [aax] file checksum == 0c527840c4f18517157eb0b4f9d6f9317ce60cd1
			var checksum = ffprobeStderr.ExtractString("file checksum == ", 40);

			return checksum;
		}

		/// <summary>use checksum to get activation bytes. activation bytes are unique per audible customer. only have to do this 1x/customer</summary>
		private static async Task<string> GetActivationBytes(string checksum)
		{
			Extract("rcrack.exe");

			Extract("alglib1.dll");
			// RainbowCrack files to recover your own Audible activation data (activation_bytes) in an offline manner
			Extract("audible_byte#4-4_0_10000x789935_0.rtc");
			Extract("audible_byte#4-4_1_10000x791425_0.rtc");
			Extract("audible_byte#4-4_2_10000x790991_0.rtc");
			Extract("audible_byte#4-4_3_10000x792120_0.rtc");
			Extract("audible_byte#4-4_4_10000x790743_0.rtc");
			Extract("audible_byte#4-4_5_10000x790568_0.rtc");
			Extract("audible_byte#4-4_6_10000x791458_0.rtc");
			Extract("audible_byte#4-4_7_10000x791707_0.rtc");
			Extract("audible_byte#4-4_8_10000x790202_0.rtc");
			Extract("audible_byte#4-4_9_10000x791022_0.rtc");

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(resdir, "rcrack.exe"),
				Arguments = @". -h " + checksum,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WorkingDirectory = Directory.GetCurrentDirectory()
			};

			using var rcr = new Process { StartInfo = startInfo };
			rcr.Start();

			var rcrackStdout = rcr.StandardOutput.ReadToEnd();

			await Task.Run(() => rcr.WaitForExit());
			rcr.Close();

			// example result
			// 0c527840c4f18517157eb0b4f9d6f9317ce60cd1  \xbd\x89X\x09  hex:bd895809
			var activation_bytes = rcrackStdout.ExtractString("hex:", 8);

			return activation_bytes;
		}

		// ProcessStartInfo.Arguments: use Escaper.EscapeArguments instead of .SurroundWithQuotes()

		// see also: https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
		// top 2 answers show: easy, sync, async

		enum EncodeTo
		{
			/// <summary>Decrypt only. Retain original encoding</summary>
			DecryptOnly,
			/// <summary>LAME MP3</summary>
			Mp3,
			/// <summary>M4B AAC</summary>
			M4b,
			/// <summary>FLAC HD</summary>
			Flac
		}

		private static async Task decryptAndSaveFile(string activation_bytes, string inputPath, string outputPath, TextBoxBase debugWindow, EncodeTo encodeTo, int encodeQuality = 80)
		{
			Extract("ffmpeg.exe");

			var fileBase = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(inputPath));

			string arguments;

			// only decrypt. no re-encoding
			if (encodeTo == EncodeTo.DecryptOnly)
			{
				var fileout = fileBase + ".m4b";
				arguments = $"-activation_bytes {activation_bytes} -i {inputPath.SurroundWithQuotes()} -vn -c:a copy {fileout.SurroundWithQuotes()}";
			}
			// re-encode. encoding will be determined by file extension
			else // if (convertRb.Checked)
			{
				var fileout = fileBase + "." + encodeTo.ToString().ToLower();
				arguments = $"-y -activation_bytes {activation_bytes} -i {inputPath.SurroundWithQuotes()} -ab {encodeQuality} -map_metadata 0 -id3v2_version 3 -vn {fileout.SurroundWithQuotes()}";
			}

			// nothing in stdout. progress/debug info is in stderr
			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(resdir, "ffmpeg.exe"),
				Arguments = arguments,
				CreateNoWindow = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				WorkingDirectory = Directory.GetCurrentDirectory()
			};

			using var ffm = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
			ffm.ErrorDataReceived += (s, ea) => debugWindow.UIThread(() => debugWindow.AppendText($"DEBUG: {ea.Data}\r\n"));

			ffm.Start();
			ffm.BeginErrorReadLine();
			await Task.Run(() => ffm.WaitForExit());
			ffm.Close();
		}

		/// <summary>extract embedded resource to file if it doesn't already exist</summary>
		private static void Extract(string resourceName)
		{
			// first determine whether files exist already in res dir
			if (File.Exists(Path.Combine(resdir, resourceName)))
				return;

			// extract embedded resource
			// this technique works but there are easier ways:
			// https://stackoverflow.com/questions/13031778/how-can-i-extract-a-file-from-an-embedded-resource-and-save-it-to-disk
			Directory.CreateDirectory(resdir);
			using var resource = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream($"{nameof(ffmpeg_decrypt)}.res." + resourceName);
			using var reader = new BinaryReader(resource);
			using var file = new FileStream(Path.Combine(resdir, resourceName), FileMode.OpenOrCreate);
			using var writer = new BinaryWriter(file);
			writer.Write(reader.ReadBytes((int)resource.Length));
		}

		private void decryptConvertRb_CheckedChanged(object sender, EventArgs e) => convertGb.Enabled = convertRb.Checked;
	}
}
