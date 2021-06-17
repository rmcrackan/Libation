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
			var checksum = await RCrack.GetChecksum(inputdisplay.Text);

			statuslbl.Text = "Cracking Activation Bytes...";
			var activation_bytes = await RCrack.GetActivationBytes(checksum);

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
			Resources.Extract("ffmpeg.exe");

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
				FileName = Path.Combine(Resources.resdir, "ffmpeg.exe"),
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

		private void decryptConvertRb_CheckedChanged(object sender, EventArgs e) => convertGb.Enabled = convertRb.Checked;
	}
}
