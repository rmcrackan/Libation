using System.Text;
using ApplicationServices;
using AppScaffolding;
using Dinah.Core;
using Microsoft.EntityFrameworkCore;

namespace HangoverBase
{
    public class DatabaseTabCommands
    {
        public Func<string> SqlInput { get; }
        public Action<string> SqlOutputAppend { get; }
        public Action<string> SqlOutputOverwrite { get; }

        public DatabaseTabCommands() { }
        public DatabaseTabCommands(
            Func<string> sqlInput,
            Action<string> sqlDisplayAppend,
            Action<string> sqlDisplayOverwrite)
        {
            SqlInput = ArgumentValidator.EnsureNotNull(sqlInput, nameof(sqlInput));
            SqlOutputAppend = ArgumentValidator.EnsureNotNull(sqlDisplayAppend, nameof(sqlDisplayAppend));
            SqlOutputOverwrite = ArgumentValidator.EnsureNotNull(sqlDisplayOverwrite, nameof(sqlDisplayOverwrite));
        }
    }

    public class DatabaseTab
    {
        private DatabaseTabCommands _commands { get; }
        public string DbFile { get; private set; }

        public DatabaseTab(DatabaseTabCommands commands)
        {
            _commands = ArgumentValidator.EnsureNotNull(commands, nameof(commands));

            ArgumentValidator.EnsureNotNull(commands, nameof(_commands.SqlInput));
            ArgumentValidator.EnsureNotNull(commands, nameof(_commands.SqlOutputAppend));
            ArgumentValidator.EnsureNotNull(commands, nameof(_commands.SqlOutputOverwrite));
        }

        public void LoadDatabaseFile() => DbFile = UNSAFE_MigrationHelper.DatabaseFile;

        private string dbBackup;
        private DateTime dbFileLastModified;
        public void EnsureBackup()
        {
            if (dbBackup is not null)
                return;

            dbFileLastModified = File.GetLastWriteTimeUtc(DbFile);

            dbBackup
                = Path.ChangeExtension(DbFile, "").TrimEnd('.')
                + $"_backup_{DateTime.UtcNow:O}".Replace(':', '-').Replace('.', '-')
                + Path.GetExtension(DbFile);
            File.Copy(DbFile, dbBackup);
        }

        public void ExecuteQuery()
        {
            EnsureBackup();

            _commands.SqlOutputOverwrite("");

            try
            {
                var sql = _commands.SqlInput()?.Trim();
                if (sql is null) return;

                #region // explanation
                // Routing statements to non-query is a convenience.
                // I went down the rabbit hole of full parsing and it's more trouble than it's worth. The parsing is easy due to available libraries. The edge cases of what to do next got too complex for slight gains.
                // It's also not useful to take the extra effort to separate non-queries which don't return a row count. Eg: alter table, drop table
                // My half-assed solution here won't even catch simple mistakes like this -- and that's ok
                //   -- line 1 is a comment
                //   delete from foo
                #endregion
                var lower = sql.ToLower();
                if (lower.StartsWith("update") || lower.StartsWith("insert") || lower.StartsWith("delete"))
                    NonQuery(sql);
                else
                    Query(sql);
            }
            catch (Exception ex)
            {
                _commands.SqlOutputOverwrite($"{ex.Message}\r\n{ex.StackTrace}");
            }
            finally
            {
                DeleteUnneededBackups();
            }
        }

        public void DeleteUnneededBackups()
        {
            var newLastModified = File.GetLastWriteTimeUtc(DbFile);
            if (dbFileLastModified == newLastModified)
            {
                File.Delete(dbBackup);
                dbBackup = null;
            }
        }

        public void Query(string sql)
        {
            // ef doesn't support truly generic queries. have to drop down to ado.net
            using var context = DbContexts.GetContext();
            using var conn = context.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var reader = cmd.ExecuteReader();
            var results = 0;
            var builder = new StringBuilder();
            var lines = 0;
            while (reader.Read())
            {
                results++;

                for (var i = 0; i < reader.FieldCount; i++)
                    builder.Append(reader.GetValue(i) + "\t");
                builder.AppendLine();

                lines++;
                if (lines % 10 == 0)
                {
                    _commands.SqlOutputAppend(builder.ToString());
                    builder.Clear();
                }
            }

            _commands.SqlOutputAppend(builder.ToString());
            builder.Clear();

            if (results == 0)
                _commands.SqlOutputOverwrite("[no results]");
            else
            {
                _commands.SqlOutputAppend($"\r\n{results} result");
                if (results != 1) _commands.SqlOutputAppend("s");
            }
        }

        public void NonQuery(string sql)
        {
            using var context = DbContexts.GetContext();
            var results = context.Database.ExecuteSqlRaw(sql);

            _commands.SqlOutputAppend($"{results} record");
            if (results != 1) _commands.SqlOutputAppend("s");
            _commands.SqlOutputAppend(" affected");
        }
    }
}
