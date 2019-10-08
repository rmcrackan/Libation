using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using FileManager;

namespace CookieMonster
{
    internal class FireFox : IBrowser
    {
        public async Task<IEnumerable<CookieValue>> GetAllCookiesAsync()
        {
            var col = new List<CookieValue>();

            string strPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Mozilla\Firefox\Profiles");
            if (!FileUtility.FileExists(strPath))
                return col;
            var dirs = new DirectoryInfo(strPath).GetDirectories("*.default");
            if (dirs.Length != 1)
                return col;
            strPath = Path.Combine(strPath, dirs[0].Name, "cookies.sqlite");
            if (!FileUtility.FileExists(strPath))
                return col;

            // First copy the cookie jar so that we can read the cookies from unlocked copy while FireFox is running
            var strTemp = strPath + ".temp";

            File.Copy(strPath, strTemp, true);

			// Now open the temporary cookie jar and extract Value from the cookie if we find it.
			using var conn = new SQLiteConnection("Data Source=" + strTemp + ";pooling=false");
			using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT host, name, value, lastAccessed, expiry FROM moz_cookies; ";

            conn.Open();
			using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
			while (reader.Read())
			{
			    var host_key = reader.GetString(0);
			    var name = reader.GetString(1);
			    var value = reader.GetString(2);
			    var lastAccessed = reader.GetInt32(3);
			    var expiry = reader.GetInt32(4);

			    col.Add(new CookieValue { Browser = "firefox", Domain = host_key, Name = name, Value = value, LastAccess = lastAccessedToDateTime(lastAccessed), Expires = expiryToDateTime(expiry) });
			}

            if (FileUtility.FileExists(strTemp))
                File.Delete(strTemp);

            return col;
        }

        // time is in microseconds since unix epoch
        private static DateTime lastAccessedToDateTime(int time) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(time);

        // time is in normal seconds since unix epoch
        private static DateTime expiryToDateTime(int time) => new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(time);
    }
}
