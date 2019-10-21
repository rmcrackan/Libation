using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AaxDecrypter;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileManager;

namespace ScrapingDomainServices
{
    /// <summary>
    /// Download DRM book and decrypt audiobook files.
    /// 
    /// Processes:
    /// Download: download aax file: the DRM encrypted audiobook
    /// Decrypt: remove DRM encryption from audiobook. Store final book
    /// Backup: perform all steps (downloaded, decrypt) still needed to get final book
    /// </summary>
    public class DecryptBook : IDecryptable
    {
        public event EventHandler<string> Begin;
        public event EventHandler<string> StatusUpdate;
        public event EventHandler<string> DecryptBegin;

        public event EventHandler<string> TitleDiscovered;
        public event EventHandler<string> AuthorsDiscovered;
        public event EventHandler<string> NarratorsDiscovered;
        public event EventHandler<byte[]> CoverImageFilepathDiscovered;
        public event EventHandler<int> UpdateProgress;

        public event EventHandler<string> DecryptCompleted;
        public event EventHandler<string> Completed;

        // ValidateAsync() doesn't need UI context
        public async Task<bool> ValidateAsync(LibraryBook libraryBook)
            => await validateAsync_ConfigureAwaitFalse(libraryBook.Book.AudibleProductId).ConfigureAwait(false);
        private async Task<bool> validateAsync_ConfigureAwaitFalse(string productId)
            => await AudibleFileStorage.AAX.ExistsAsync(productId)
            && !(await AudibleFileStorage.Audio.ExistsAsync(productId));

        // do NOT use ConfigureAwait(false) on ProcessUnregistered()
        // often does a lot with forms in the UI context
        public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            var displayMessage = $"[{libraryBook.Book.AudibleProductId}] {libraryBook.Book.Title}";

            Begin?.Invoke(this, displayMessage);

            try
            {
                var aaxFilename = await AudibleFileStorage.AAX.GetAsync(libraryBook.Book.AudibleProductId);

                if (aaxFilename == null)
                    return new StatusHandler { "aaxFilename parameter is null" };
                if (!FileUtility.FileExists(aaxFilename))
                    return new StatusHandler { $"Cannot find AAX file: {aaxFilename}" };
                if (await AudibleFileStorage.Audio.ExistsAsync(libraryBook.Book.AudibleProductId))
                    return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

                string proposedOutputFile = Path.Combine(AudibleFileStorage.DecryptInProgress, $"[{libraryBook.Book.AudibleProductId}].m4b");

                string outputAudioFilename;
                //outputAudioFilename = await inAudibleDecrypt(proposedOutputFile, aaxFilename);
                outputAudioFilename = await aaxToM4bConverterDecrypt(proposedOutputFile, aaxFilename);

                // decrypt failed
                if (outputAudioFilename == null)
                    return new StatusHandler { "Decrypt failed" };

                moveFilesToBooksDir(libraryBook.Book, outputAudioFilename);

                Dinah.Core.IO.FileExt.SafeDelete(aaxFilename);

                var statusHandler = new StatusHandler();
                var finalAudioExists = await AudibleFileStorage.Audio.ExistsAsync(libraryBook.Book.AudibleProductId);
                if (!finalAudioExists)
                    statusHandler.AddError("Cannot find final audio file after decryption");
                return statusHandler;
            }
            finally
            {
                Completed?.Invoke(this, displayMessage);
            }
        }

        private async Task<string> aaxToM4bConverterDecrypt(string proposedOutputFile, string aaxFilename)
        {
            DecryptBegin?.Invoke(this, $"Begin decrypting {aaxFilename}");

            try
            {
                var converter = await AaxToM4bConverter.CreateAsync(aaxFilename, Configuration.Instance.DecryptKey);
                converter.AppName = "Libation";

                TitleDiscovered?.Invoke(this, converter.tags.title);
                AuthorsDiscovered?.Invoke(this, converter.tags.author);
                NarratorsDiscovered?.Invoke(this, converter.tags.narrator);
                CoverImageFilepathDiscovered?.Invoke(this, converter.coverBytes);

                converter.SetOutputFilename(proposedOutputFile);
                converter.DecryptProgressUpdate += (s, progress) => UpdateProgress?.Invoke(this, progress);

                // REAL WORK DONE HERE
                var success = await Task.Run(() => converter.Run());

                if (!success)
                {
                    Console.WriteLine("decrypt failed");
                    return null;
                }

                Configuration.Instance.DecryptKey = converter.decryptKey;

                return converter.outputFileName;
            }
            finally
            {
                DecryptCompleted?.Invoke(this, $"Completed decrypting {aaxFilename}");
            }
        }

        private static void moveFilesToBooksDir(Book product, string outputAudioFilename)
        {
            // files are: temp path\author\[asin].ext
            var m4bDir = new FileInfo(outputAudioFilename).Directory;
            var files = m4bDir
                .EnumerateFiles()
                .Where(f => f.Name.ContainsInsensitive(product.AudibleProductId))
                .ToList();

            // create final directory. move each file into it. MOVE AUDIO FILE LAST
            // new dir: safetitle_limit50char + " [" + productId + "]"

            // to prevent the paths from getting too long, we don't need after the 1st ":" for the folder
            var underscoreIndex = product.Title.IndexOf(':');
            var titleDir = (underscoreIndex < 4) ? product.Title : product.Title.Substring(0, underscoreIndex);
            var finalDir = FileUtility.GetValidFilename(AudibleFileStorage.BooksDirectory, titleDir, null, product.AudibleProductId);
            Directory.CreateDirectory(finalDir);

            // move audio files to the end of the collection so these files are moved last
            var musicFiles = files.Where(f => AudibleFileStorage.Audio.IsFileTypeMatch(f));
            files = files
                .Except(musicFiles)
                .Concat(musicFiles)
                .ToList();

            var musicFileExt = musicFiles
                .Select(f => f.Extension)
                .Distinct()
                .Single()
                .Trim('.');

            foreach (var f in files)
            {
                var dest = AudibleFileStorage.Audio.IsFileTypeMatch(f)
                    // audio filename: safetitle_limit50char + " [" + productId + "]." + audio_ext
                    ? FileUtility.GetValidFilename(finalDir, product.Title, musicFileExt, product.AudibleProductId)
                    // non-audio filename: safetitle_limit50char + " [" + productId + "][" + audio_ext +"]." + non_audio_ext
                    : FileUtility.GetValidFilename(finalDir, product.Title, f.Extension, product.AudibleProductId, musicFileExt);

                File.Move(f.FullName, dest);
            }
        }

        #region legacy inAudible wire-up code
        //
        // instructions are in comments below for editing and interacting with inAudible. eg:
        //   \_NET\Visual Studio 2017\inAudible197\decompiled - in progress\inAudible.csproj
        // first, add its project and put its exe path into inAudiblePath
        //
        #region placeholder code
        // this exists so the below legacy code will compile as-is. comment out placeholder code when actually connecting to inAudible

        class Form
        {
            internal void Show() => throw new NotImplementedException();
            internal void Kill() => throw new NotImplementedException();
        }
        class TextBox
        {
            internal string Text { set => throw new NotImplementedException(); }
        }
        class Button
        {
            internal void PerformClick() => throw new NotImplementedException();
        }
        class AudibleConvertor
        {
            internal class GLOBALS
            {
                internal static string ExecutablePath { set => throw new NotImplementedException(); }
            }
            internal class Form1 : Form
            {
                internal Form1(Action<string> action) => throw new NotImplementedException();
                internal void LoadAudibleFiles(string[] arr) => throw new NotImplementedException();
                internal TextBox txtOutputFile { get => throw new NotImplementedException(); }
                internal Button btnConvert { get => throw new NotImplementedException(); }
            }
        }
        #endregion

        private static string inAudiblePath { get; }
            = @"C:\"
            + @"DEV_ROOT_EXAMPLE\"
            + @"_NET\Visual Studio 2017\"
            + @"inAudible197\decompiled - in progress\bin\Debug\inAudible.exe";
        private static async Task<string> inAudibleDecrypt(string proposedOutputFile, string aaxFilename)
        {
            #region // inAudible code to change:
            /*
             * Prevent "Path too long" error
             * =============================
             * BatchFiles.cs :: GenerateOutputFilepath()
             * Add this just before the bottom return statement
             *
                  if (oneOff && !string.IsNullOrWhiteSpace(outputPath))
                      return str + "\\" + Path.GetFileNameWithoutExtension(outputPath) + "." + fileType;
             */
            #endregion

            #region init inAudible
            #region // suppress warnings
            // inAudible. project properties > Build > Warning level=2
            #endregion
            #region // instructions to create inAudible ExecutablePath
            /*
             * STEP 1
             * ======
             * do a PROJECT level find/replace within inAudible
             * find
             *     Application.ExecutablePath
             * replace
             *     AudibleConvertor.GLOBALS.ExecutablePath
             * STEP 2
             * ======
             * new inAudible root-level file
             *     _GLOBALS.cs
             * contents:
             * namespace AudibleConvertor { public static class GLOBALS { public static string ExecutablePath { get; set; } = System.Windows.Forms.Application.ExecutablePath; } }
             */
            #endregion
            AudibleConvertor.GLOBALS.ExecutablePath = inAudiblePath;
            // before using inAudible, set ini values
            setIniValues(new Dictionary<string, string> { ["selected_codec"] = "lossless", ["embed_cover"] = "True", ["copy_cover_art"] = "False", ["create_cue"] = "True", ["nfo"] = "True", ["strip_unabridged"] = "True", });
            #endregion

            // this provides the async magic to keep all of the form calling code in one method instead of event callback pattern
            // TODO: error handling is not obvious:
            //   https://deaddesk.top/don't-fall-for-TaskCompletionSource-traps/
            var tcs = new TaskCompletionSource<string>();

            // to know when inAudible is complete. code to change:
            #region // code to preceed ctor
            /*
                  Action<string> _conversionCompleteAction;
                  public Form1(Action<string> conversionCompleteAction) : this() => _conversionCompleteAction = conversionCompleteAction;
             */
            #endregion
            #region // code for the end of bgwAAX_Completed()
            /*
                  if (this.myAdvancedOptions.beep && !this.myAdvancedOptions.cylon) this.SOXPlay(Form1.appPath + "\\beep.mp3", true);
                  else if (myAdvancedOptions.cylon) SOXPlay(appPath + "\\inAudible-end.mp3", true);
                  _conversionCompleteAction?.Invoke(outputFileName);
                }
             */
            #endregion

            #region start inAudible
            var form = new AudibleConvertor.Form1(tcs.SetResult);
            form.Show();
            form.LoadAudibleFiles(new string[] { aaxFilename }); // inAudible: make public

            // change output info to include asin. put in temp
            form.txtOutputFile.Text = proposedOutputFile; // inAudible: make public

            // submit/process/decrypt
            form.btnConvert.PerformClick(); // inAudible: make public

            // ta-da -- magic! we stop here until inAudible complete
            var outputAudioFilename = await tcs.Task;
            #endregion

            #region when complete, close inAudible
            // use this instead of Dinah.Core.Windows.Forms.UIThread()
            form.Kill();
            #endregion

            return outputAudioFilename;
        }

        private static void setIniValues(Dictionary<string, string> settings)
        {
            // C:\Users\username\Documents\inAudible\config.ini
            var iniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "inAudible", "config.ini");
            var iniContents = File.ReadAllText(iniPath);

            foreach (var kvp in settings)
                iniContents = System.Text.RegularExpressions.Regex.Replace(
                    iniContents,
                    $@"\r\n{kvp.Key} = [^\r\n]+\r\n",
                    $"\r\n{kvp.Key} = {kvp.Value}\r\n");

            File.WriteAllText(iniPath, iniContents);
        }
        #endregion
    }
}
