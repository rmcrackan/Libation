using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AaxDecrypter;
using Dinah.Core.IO;
using Dinah.Core.Logging;
using Dinah.Core.Windows.Forms;
using Serilog;

namespace inAudibleLite
{
	public partial class Form1 : Form
	{
		public const string APP_NAME = "inAudibleLite 1.0";

		private ISimpleAaxToM4bConverter converter;

		private Action<string> _conversionCompleteAction { get; }
		public Form1(Action<string> conversionCompleteAction) : this() => _conversionCompleteAction = conversionCompleteAction;

		public Form1()
		{
			InitializeComponent();
			this.btnConvert.Enabled = false;

			initSerilog();
			redirectWriteLine();
		}

		private static void initSerilog()
		{
			// default. for reference. output example:
			// 2019-11-26 08:48:40.224 -05:00 [DBG] Begin Libation
			var default_outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
			// with class and method info. output example:
			// 2019-11-26 08:48:40.224 -05:00 [DBG] (at LibationWinForm.Program.init()) Begin Libation
			var code_outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (at {Caller}) {Message:lj}{NewLine}{Exception}";


			var logPath = Path.Combine(Path.GetTempPath(), "Log.log");

			Log.Logger = new LoggerConfiguration()
				.Enrich.WithCaller()
				.MinimumLevel.Debug()
				.WriteTo.File(logPath,
					rollingInterval: RollingInterval.Month,
					outputTemplate: code_outputTemplate)
				.CreateLogger();

			Log.Logger.Debug("Begin Libation");

			// .Here() captures debug info via System.Runtime.CompilerServices attributes. Warning: expensive
			//var withLineNumbers_outputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}{Exception}{NewLine}";
			//Log.Logger.Here().Debug("Begin Libation. Debug with line numbers");
		}

		private void redirectWriteLine()
		{
			// redirect Console.WriteLine to console, log file, textbox
			var multiLogger = new MultiTextWriter(
				Console.Out,
				new RichTextBoxTextWriter(this.rtbLog),
				new SerilogTextWriter());
			Console.SetOut(multiLogger);
		}

		private async void btnSelectFile_Click(object sender, EventArgs e)
		{
			var openFileDialog = new OpenFileDialog { Filter = "Audible files (*.aax)|*.aax" };
			if (openFileDialog.ShowDialog() != DialogResult.OK)
				return;

			this.rtbLog.Clear();

			this.txtInputFile.Text = openFileDialog.FileName;
			converter = await AaxToM4bConverter.CreateAsync(this.txtInputFile.Text, this.decryptKeyTb.Text);

			pictureBox1.Image = Dinah.Core.Drawing.ImageReader.ToImage(converter.coverBytes);

			this.txtOutputFile.Text = converter.outputFileName;

			this.btnConvert.Enabled = true;
		}

		private async void btnConvert_Click(object sender, EventArgs e)
		{
			this.btnConvert.Enabled = false;

			// only re-process prelim stats if input filename was changed
			//also pick up new decrypt key, etc//if (this.txtInputFile.Text != converter?.inputFileName)
			converter = await AaxToM4bConverter.CreateAsync(this.txtInputFile.Text, this.decryptKeyTb.Text);

			converter.AppName = APP_NAME;
			converter.SetOutputFilename(this.txtOutputFile.Text);
			converter.DecryptProgressUpdate += (s, progress) => this.progressBar1.UpdateValue(progress);

			// REAL WORK DONE HERE
			await Task.Run(() => converter.Run());

			Console.WriteLine("Output directory: " + converter.outDir);

			// open output dir
			Process.Start("file://" + converter.outDir);

			this.btnConvert.Enabled = true;

			_conversionCompleteAction?.Invoke(converter.outputFileName);
		}
	}
}
