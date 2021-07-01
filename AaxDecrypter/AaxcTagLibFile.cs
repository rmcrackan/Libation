using System.Linq;
using System.Text.RegularExpressions;
using TagLib;
using TagLib.Mpeg4;

namespace AaxDecrypter
{
    public class AaxcTagLibFile : TagLib.Mpeg4.File
    {
        public AppleTag AppleTags => GetTag(TagTypes.Apple) as AppleTag;

        private static ReadOnlyByteVector narratorType = new ReadOnlyByteVector(0xa9, (byte)'n', (byte)'r', (byte)'t');
        private static ReadOnlyByteVector descriptionType = new ReadOnlyByteVector(0xa9, (byte)'d', (byte)'e', (byte)'s');
        private static ReadOnlyByteVector publisherType = new ReadOnlyByteVector(0xa9, (byte)'p', (byte)'u', (byte)'b');

        public string AsciiTitleSansUnabridged => TitleSansUnabridged is not null? unicodeToAscii(TitleSansUnabridged) : default;
        public string AsciiFirstAuthor => FirstAuthor is not null? unicodeToAscii(FirstAuthor) : default;
        public string AsciiNarrator => Narrator is not null ? unicodeToAscii(Narrator) : default;
        public string AsciiComment => Comment is not null ? unicodeToAscii(Comment) : default;
        public string AsciiLongDescription => LongDescription is not null ? unicodeToAscii(LongDescription) : default;

        public string Comment => AppleTags.Comment;
        public string[] Authors => AppleTags.Performers;
        public string FirstAuthor => Authors?.Length > 0 ? Authors[0] : default;
        public string TitleSansUnabridged => AppleTags.Title?.Replace(" (Unabridged)", "");
        public string BookCopyright => _copyright is not null && _copyright.Length > 0 ? _copyright[0] : default;
        public string RecordingCopyright => _copyright is not null && _copyright.Length > 1 ? _copyright[1] : default;
        public string Narrator 
        { 
            get
            {
                string[] text = AppleTags.GetText(narratorType);
                return text.Length == 0 ? default : text[0];
            }
        }
        public string LongDescription 
        { 
            get
            {
                string[] text = AppleTags.GetText(descriptionType);
                return text.Length == 0 ? default : text[0];
            }
        }
        public string ReleaseDate
        {
            get
            {
                string[] text = AppleTags.GetText("rldt");
                return text.Length == 0 ? default : text[0];
            }
        }
        public string Publisher
        {
            get
            {
                string[] text = AppleTags.GetText(publisherType);
                return text.Length == 0 ? default : text[0];
            }
        }

        private string[] _copyright => AppleTags.Copyright?.Replace("&#169;", string.Empty)?.Replace("(P)", string.Empty)?.Split(';');
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

        /// <summary>
        /// Attempts to convert unicode characters to an approximately equal ASCII character.
        /// </summary>
        private string unicodeToAscii(string unicodeStr)
        {
            //Accents
            unicodeStr = Regex.Replace(unicodeStr, "[éèëêð]", "e");
            unicodeStr = Regex.Replace(unicodeStr, "[ÉÈËÊ]", "E");
            unicodeStr = Regex.Replace(unicodeStr, "[àâä]", "a");
            unicodeStr = Regex.Replace(unicodeStr, "[ÀÁÂÃÄÅ]", "A");
            unicodeStr = Regex.Replace(unicodeStr, "[àáâãäå]", "a");
            unicodeStr = Regex.Replace(unicodeStr, "[ÙÚÛÜ]", "U");
            unicodeStr = Regex.Replace(unicodeStr, "[ùúûüµ]", "u");
            unicodeStr = Regex.Replace(unicodeStr, "[òóôõöø]", "o");
            unicodeStr = Regex.Replace(unicodeStr, "[ÒÓÔÕÖØ]", "O");
            unicodeStr = Regex.Replace(unicodeStr, "[ìíîï]", "i");
            unicodeStr = Regex.Replace(unicodeStr, "[ÌÍÎÏ]", "I");
            unicodeStr = Regex.Replace(unicodeStr, "[š]", "s");
            unicodeStr = Regex.Replace(unicodeStr, "[Š]", "S");
            unicodeStr = Regex.Replace(unicodeStr, "[ñ]", "n");
            unicodeStr = Regex.Replace(unicodeStr, "[Ñ]", "N");
            unicodeStr = Regex.Replace(unicodeStr, "[ç]", "c");
            unicodeStr = Regex.Replace(unicodeStr, "[Ç]", "C");
            unicodeStr = Regex.Replace(unicodeStr, "[ÿ]", "y");
            unicodeStr = Regex.Replace(unicodeStr, "[Ÿ]", "Y");
            unicodeStr = Regex.Replace(unicodeStr, "[ž]", "z");
            unicodeStr = Regex.Replace(unicodeStr, "[Ž]", "Z");
            unicodeStr = Regex.Replace(unicodeStr, "[Ð]", "D");

            //Ligatures
            unicodeStr = Regex.Replace(unicodeStr, "[œ]", "oe");
            unicodeStr = Regex.Replace(unicodeStr, "[Œ]", "Oe");
            unicodeStr = Regex.Replace(unicodeStr, "[ꜳ]", "aa");
            unicodeStr = Regex.Replace(unicodeStr, "[Ꜳ]", "AA");
            unicodeStr = Regex.Replace(unicodeStr, "[æ]", "ae");
            unicodeStr = Regex.Replace(unicodeStr, "[Æ]", "AE");
            unicodeStr = Regex.Replace(unicodeStr, "[ꜵ]", "ao");
            unicodeStr = Regex.Replace(unicodeStr, "[Ꜵ]", "AO");
            unicodeStr = Regex.Replace(unicodeStr, "[ꜷ]", "au");
            unicodeStr = Regex.Replace(unicodeStr, "[Ꜷ]", "AU");
            unicodeStr = Regex.Replace(unicodeStr, "[«»ꜹꜻ]", "av");
            unicodeStr = Regex.Replace(unicodeStr, "[«»ꜸꜺ]", "AV");
            unicodeStr = Regex.Replace(unicodeStr, "[🙰]", "et");
            unicodeStr = Regex.Replace(unicodeStr, "[ﬀ]", "ff");
            unicodeStr = Regex.Replace(unicodeStr, "[ﬃ]", "ffi");
            unicodeStr = Regex.Replace(unicodeStr, "[ﬄ]", "f‌f‌l");
            unicodeStr = Regex.Replace(unicodeStr, "[ﬁ]", "fi");
            unicodeStr = Regex.Replace(unicodeStr, "[ﬂ]", "fl");
            unicodeStr = Regex.Replace(unicodeStr, "[ƕ]", "hv");
            unicodeStr = Regex.Replace(unicodeStr, "[Ƕ]", "Hv");
            unicodeStr = Regex.Replace(unicodeStr, "[℔]", "lb");
            unicodeStr = Regex.Replace(unicodeStr, "[ꝏ]", "oo");
            unicodeStr = Regex.Replace(unicodeStr, "[Ꝏ]", "OO");
            unicodeStr = Regex.Replace(unicodeStr, "[ﬆ]", "st");
            unicodeStr = Regex.Replace(unicodeStr, "[ꜩ]", "tz");
            unicodeStr = Regex.Replace(unicodeStr, "[Ꜩ]", "TZ");
            unicodeStr = Regex.Replace(unicodeStr, "[ᵫ]", "ue");
            unicodeStr = Regex.Replace(unicodeStr, "[ꭣ]", "uo");

            //Punctuation
            unicodeStr = Regex.Replace(unicodeStr, "[«»\u2018\u2019\u201A\u201B\u2032\u2035]", "\'");
            unicodeStr = Regex.Replace(unicodeStr, "[«»\u201C\u201D\u201E\u201F\u2033\u2036]", "\"");
            unicodeStr = Regex.Replace(unicodeStr, "[\u2026]", "...");
            unicodeStr = Regex.Replace(unicodeStr, "[\u1680]", "-");

            //Spaces
            unicodeStr = Regex.Replace(unicodeStr, "[«»\u00A0\u2000\u2002\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u200F\u205F\u3000]", " ");
            unicodeStr = Regex.Replace(unicodeStr, "[«»\u2001\u2003]", "  ");
            unicodeStr = Regex.Replace(unicodeStr, "[«»\u180E\u200B\uFEFF]", "");

            return unicodeStr;
        }
    }
}
