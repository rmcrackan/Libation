using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieMonster
{
    internal class InternetExplorer : IBrowser
    {
        public async Task<IEnumerable<CookieValue>> GetAllCookiesAsync()
        {
            // real locations of Windows Cookies folders
            //
            // Windows 7:
            // C:\Users\username\AppData\Roaming\Microsoft\Windows\Cookies
            // C:\Users\username\AppData\Roaming\Microsoft\Windows\Cookies\Low
            //
            // Windows 8, Windows 8.1, Windows 10:
            // C:\Users\username\AppData\Local\Microsoft\Windows\INetCookies
            // C:\Users\username\AppData\Local\Microsoft\Windows\INetCookies\Low

            var strPath = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);

            var col = (await getIECookiesAsync(strPath).ConfigureAwait(false)).ToList();
            col = col.Concat(await getIECookiesAsync(Path.Combine(strPath, "Low"))).ToList();

            return col;
        }

        private static async Task<IEnumerable<CookieValue>> getIECookiesAsync(string strPath)
        {
            var cookies = new List<CookieValue>();

            var files = await Task.Run(() => Directory.EnumerateFiles(strPath, "*.txt"));
            foreach (string path in files)
            {
                var cookiesInFile = new List<CookieValue>();

                var cookieLines = File.ReadAllLines(path);
                CookieValue currCookieVal = null;
                for (var i = 0; i < cookieLines.Length; i++)
                {
                    var line = cookieLines[i];

                    // IE cookie format
                    // 0  Cookie name
                    // 1  Cookie value
                    // 2  Host / path for the web server setting the cookie
                    // 3  Flags
                    // 4  Expiration time (low int)
                    // 5  Expiration time (high int)
                    // 6  Creation time (low int)
                    // 7  Creation time (high int)
                    // 8  Record delimiter == "*"
                    var pos = i % 9;
                    long expLoTemp = 0;
                    long creatLoTemp = 0;
                    if (pos == 0)
                    {
                        currCookieVal = new CookieValue { Browser = "ie", Name = line };
                        cookiesInFile.Add(currCookieVal);
                    }
                    else if (pos == 1)
                        currCookieVal.Value = line;
                    else if (pos == 2)
                        currCookieVal.Domain = line;
                    else if (pos == 4)
                        expLoTemp = Int64.Parse(line);
                    else if (pos == 5)
                        currCookieVal.Expires = LoHiToDateTime(expLoTemp, Int64.Parse(line));
                    else if (pos == 6)
                        creatLoTemp = Int64.Parse(line);
                    else if (pos == 7)
                        currCookieVal.LastAccess = LoHiToDateTime(creatLoTemp, Int64.Parse(line));
                }

                cookies.AddRange(cookiesInFile);
            }

            return cookies;
        }

        private static DateTime LoHiToDateTime(long lo, long hi) => DateTime.FromFileTimeUtc(((hi << 32) + lo));
    }
}
