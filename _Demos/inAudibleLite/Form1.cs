using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AaxDecrypter;
using Dinah.Core.IO;
using Dinah.Core.Windows.Forms;

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

			initLogging();
		}

		private void initLogging()
		{
			// redirect Console.WriteLine to console, log file, textbox
			var origOut = Console.Out;
			var controlWriter = new RichTextBoxTextWriter(this.rtbLog);
			var tempPath = Path.GetTempPath();
			var logger1 = new FileLogger(Path.Combine(tempPath, APP_NAME));
			var logger2 = new FileLoggerTextWriter(logger1);
			var multiLogger = new MultiTextWriter(origOut, controlWriter, logger2);
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
