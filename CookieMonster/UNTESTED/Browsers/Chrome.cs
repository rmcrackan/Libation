using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileManager;

namespace CookieMonster
{
    internal class Chrome : IBrowser
    {
		public async Task<IEnumerable<CookieValue>> GetAllCookiesAsync()
		{
			var col = new List<CookieValue>();

			var strPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Cookies");
			if (!FileUtility.FileExists(strPath))
				return col;

			//
			// IF WE GET AN ERROR HERE
			//   then add a reference to sqlite core in the project which is ultimately calling this.
			// a project which directly references CookieMonster doesn't need to also ref sqlite.
			// however, for any further number of abstractions, the project needs to directly ref sqlite.
			// eg: this will not work unless the winforms proj adds sqlite to ref.s:
			//     LibationWinForm > AudibleDotComAutomation > CookieMonster
			//
			using var conn = new SQLiteConnection("Data Source=" + strPath + ";pooling=false");
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT host_key, name, value, encrypted_value, last_access_utc, expires_utc FROM cookies;";

			conn.Open();
			using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
			while (reader.Read())
			{
				var host_key = reader.GetString(0);
				var name = reader.GetString(1);
				var value = reader.GetString(2);
				var last_access_utc = reader.GetInt64(4);
				var expires_utc = reader.GetInt64(5);

				// https://stackoverflow.com/a/25874366
				if (string.IsNullOrWhiteSpace(value))
				{
					var encrypted_value = (byte[])reader[3];
					var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encrypted_value, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
					value = Encoding.ASCII.GetString(decodedData);
				}

				try
				{
					// if something goes wrong in this step (eg: a cookie has an invalid filetime), then just skip this cookie
					col.Add(new CookieValue { Browser = "chrome", Domain = host_key, Name = name, Value = value, LastAccess = chromeTimeToDateTimeUtc(last_access_utc), Expires = chromeTimeToDateTimeUtc(expires_utc) });
				}
				catch { }
			}

			return col;
		}

		// Chrome uses 1601-01-01 00:00:00 UTC as the epoch (ie the starting point for the millisecond time counter).
		// this is the same as "FILETIME" in Win32 except FILETIME uses 100ns ticks instead of ms.
		private static DateTime chromeTimeToDateTimeUtc(long time) => DateTime.SpecifyKind(DateTime.FromFileTime(time * 10), DateTimeKind.Utc);
	}
}
