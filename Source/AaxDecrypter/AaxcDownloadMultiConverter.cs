using AAXClean;
using AAXClean.Codecs;
using FileManager;
using Mpeg4Lib;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AaxDecrypter;

public class AaxcDownloadMultiConverter : AaxcDownloadConvertBase
{
	private static readonly TimeSpan minChapterLength = TimeSpan.FromSeconds(3);
	private FileStream? workingFileStream;

	public AaxcDownloadMultiConverter(string outDirectory, string cacheDirectory, IDownloadOptions dlOptions)
		: base(outDirectory, cacheDirectory, dlOptions)
	{
		AsyncSteps.Name = $"Download, Convert Aaxc To {DownloadOptions.OutputFormat}, and Split";
		AsyncSteps["Step 1: Get Aaxc Metadata"] = () => Task.Run(Step_GetMetadata);
		AsyncSteps["Step 2: Download Decrypted Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
	}

	protected override void OnInitialized()
	{
		//Finishing configuring lame encoder.
		if (DownloadOptions.OutputFormat == OutputFormat.Mp3)
		{
			if (AaxFile is null)
				throw new InvalidOperationException($"AaxFile is null during {nameof(OnInitialized)} in {nameof(AaxcDownloadConvertBase)}.");
			if (DownloadOptions.LameConfig is null)
				throw new InvalidOperationException($"LameConfig is null during {nameof(OnInitialized)} in {nameof(DownloadOptions)}.");

			MpegUtil.ConfigureLameOptions(
				AaxFile,
				DownloadOptions.LameConfig,
				DownloadOptions.Downsample,
				DownloadOptions.MatchSourceBitrate,
				chapters: null);
		}
	}


	protected async override Task<bool> Step_DownloadAndDecryptAudiobookAsync()
	{
		if (AaxFile is null) return false;
	
		try
		{
			await (AaxConversion = decryptMultiAsync(AaxFile, DownloadOptions.ChapterInfo));

			if (AaxConversion.IsCompletedSuccessfully)
				await moveMoovToBeginning(AaxFile, workingFileStream?.Name);

			return AaxConversion.IsCompletedSuccessfully;
		}
		finally
		{
			workingFileStream?.Dispose();
			FinalizeDownload();
		}
	}

	private Mp4Operation decryptMultiAsync(Mp4File aaxFile, ChapterInfo splitChapters)
	{
		var chapterCount = 0;
		return
			DownloadOptions.OutputFormat == OutputFormat.M4b
			? aaxFile.ConvertToMultiMp4aAsync
			(
				splitChapters,
				newSplitCallback => newSplit(++chapterCount, splitChapters, newSplitCallback)
			)
			: aaxFile.ConvertToMultiMp3Async
			(
				splitChapters,
				newSplitCallback => newSplit(++chapterCount, splitChapters, newSplitCallback),
				DownloadOptions.LameConfig
			);

		void newSplit(int currentChapter, ChapterInfo splitChapters, INewSplitCallback newSplitCallback)
		{
			moveMoovToBeginning(aaxFile, workingFileStream?.Name).GetAwaiter().GetResult();
			var newTempFile = GetNewTempFilePath(DownloadOptions.OutputFormat.ToString());
			MultiConvertFileProperties props = new()
			{
				OutputFileName = newTempFile.FilePath,
				PartsPosition = currentChapter,
				PartsTotal = splitChapters.Count,
				Title = newSplitCallback.Chapter?.Title,
			};

			newSplitCallback.OutputFile = workingFileStream = createOutputFileStream(props);
			newSplitCallback.TrackTitle = DownloadOptions.GetMultipartTitle(props);
			newSplitCallback.TrackNumber = currentChapter;
			newSplitCallback.TrackCount = splitChapters.Count;

			OnTempFileCreated(newTempFile with { PartProperties = props });
		}

		FileStream createOutputFileStream(MultiConvertFileProperties multiConvertFileProperties)
		{
			FileUtility.SaferDelete(multiConvertFileProperties.OutputFileName);
			return File.Open(multiConvertFileProperties.OutputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		}
	}

	private Mp4Operation moveMoovToBeginning(Mp4File aaxFile, string? filename)
	{
		if (DownloadOptions.OutputFormat is OutputFormat.M4b
			&& DownloadOptions.MoveMoovToBeginning
			&& filename is not null
			&& File.Exists(filename))
		{
			return Mp4File.RelocateMoovAsync(filename);
		}
		else return Mp4Operation.FromCompleted(aaxFile);
	}
}
