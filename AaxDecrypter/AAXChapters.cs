using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Dinah.Core.Diagnostics;

namespace AaxDecrypter
{
    public class AAXChapters : Chapters
    {
        public AAXChapters(string file)
        {
            var info = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.ffprobePath,
                Arguments = "-loglevel panic -show_chapters -print_format xml \"" + file + "\""
            };
            var xml = info.RunHidden().Output;

            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.LoadXml(xml);
            var chaptersXml = xmlDocument.SelectNodes("/ffprobe/chapters/chapter")
                .Cast<System.Xml.XmlNode>()
                .Where(n => n.Name == "chapter");

            foreach (var cnode in chaptersXml)
            {
                double startTime = double.Parse(cnode.Attributes["start_time"].Value.Replace(",", "."), CultureInfo.InvariantCulture);
                double endTime = double.Parse(cnode.Attributes["end_time"].Value.Replace(",", "."), CultureInfo.InvariantCulture);

                string chapterTitle = cnode.ChildNodes
                    .Cast<System.Xml.XmlNode>()
                    .Where(childnode => childnode.Attributes["key"].Value == "title")
                    .Select(childnode => childnode.Attributes["value"].Value)
                    .FirstOrDefault();

                AddChapter(new Chapter(startTime, endTime, chapterTitle));
            }
        }
    }
}
