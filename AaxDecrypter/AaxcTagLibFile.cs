using System;
using System.Linq;
using Dinah.Core;
using TagLib;
using TagLib.Mpeg4;

namespace AaxDecrypter
{
    public class AaxcTagLibFile : TagLib.Mpeg4.File
    {
        // ©
        private const byte COPYRIGHT = 0xa9;

        private static ReadOnlyByteVector narratorType { get; } = new ReadOnlyByteVector(COPYRIGHT, (byte)'n', (byte)'r', (byte)'t');
        private static ReadOnlyByteVector descriptionType { get; } = new ReadOnlyByteVector(COPYRIGHT, (byte)'d', (byte)'e', (byte)'s');
        private static ReadOnlyByteVector publisherType { get; } = new ReadOnlyByteVector(COPYRIGHT, (byte)'p', (byte)'u', (byte)'b');

        public string AsciiTitleSansUnabridged => TitleSansUnabridged?.UnicodeToAscii();
        public string AsciiFirstAuthor => FirstAuthor?.UnicodeToAscii();
        public string AsciiNarrator => Narrator?.UnicodeToAscii();
        public string AsciiComment => Comment?.UnicodeToAscii();
        public string AsciiLongDescription => LongDescription?.UnicodeToAscii();

        public AppleTag AppleTags => GetTag(TagTypes.Apple) as AppleTag;

        public string Comment => AppleTags.Comment;
        public string[] Authors => AppleTags.Performers;
        public string FirstAuthor => Authors?.Length > 0 ? Authors[0] : default;
        public string TitleSansUnabridged => AppleTags.Title?.Replace(" (Unabridged)", "");

        public string BookCopyright => _copyright is not null && _copyright.Length > 0 ? _copyright[0] : default;
        public string RecordingCopyright => _copyright is not null && _copyright.Length > 1 ? _copyright[1] : default;
        private string[] _copyright => AppleTags.Copyright?.Replace("&#169;", string.Empty)?.Replace("(P)", string.Empty)?.Split(';');

        public string Narrator => getAppleTagsText(narratorType);
        public string LongDescription => getAppleTagsText(descriptionType);
        public string ReleaseDate => getAppleTagsText("rldt");
        public string Publisher => getAppleTagsText(publisherType);
        private string getAppleTagsText(ByteVector byteVector)
        {
            string[] text = AppleTags.GetText(byteVector);
            return text.Length == 0 ? default : text[0];
        }

        public AaxcTagLibFile(IFileAbstraction abstraction)
            : base(abstraction, ReadStyle.Average)
        {          
        }

        public AaxcTagLibFile(string path) 
            : this(new LocalFileAbstraction(path))
        {
        }
        /// <summary>
        /// Copy all metadata fields in the source file, even those that TagLib doesn't
        /// recognize, to the output file.
        /// NOTE: Chapters aren't stored in MPEG-4 metadata. They are encoded as a Timed
        /// Text Stream (MPEG-4 Part 17), so taglib doesn't read or write them.
        /// </summary>
        /// <param name="sourceFile">File from which tags will be coppied.</param>
        public void CopyTagsFrom(AaxcTagLibFile sourceFile)
        {
            AppleTags.Clear();

            foreach (var stag in sourceFile.AppleTags)
            {
                AppleTags.SetData(stag.BoxType, stag.Children.Cast<AppleDataBox>().ToArray());
            }
        }
        public void AddPicture(byte[] coverArt)
        {
            AppleTags.SetData("covr", coverArt, 0);
        }
    }
}
