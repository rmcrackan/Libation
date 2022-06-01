using ApplicationServices;
using AppScaffolding;
using Microsoft.EntityFrameworkCore;

namespace Hangover
{
    public partial class Form1
    {
        private string dbFile;

        private void Load_databaseTab()
        {
            dbFile = UNSAFE_MigrationHelper.DatabaseFile;
            if (dbFile is null)
            {
                databaseFileLbl.Text = $"Database file not found";
                return;
            }

            databaseFileLbl.Text = $"Database file: {UNSAFE_MigrationHelper.DatabaseFile ?? "not found"}";
        }

        private void databaseTab_VisibleChanged(object sender, EventArgs e)
        {
            if (!databaseTab.Visible)
                return;
        }

        private void sqlExecuteBtn_Click(object sender, EventArgs e)
        {
            ensureBackup();

            sqlResultsTb.Clear();

            try
            {
                var sql = sqlTb.Text.Trim();

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
                    nonQuery(sql);
                else
                    query(sql);
            }
            catch (Exception ex)
            {
                sqlResultsTb.Text = $"{ex.Message}\r\n{ex.StackTrace}";
            }
            finally
            {
                deleteUnneededBackups();
            }
        }

        private string dbBackup;
        private DateTime dbFileLastModified;
        private void ensureBackup()
        {
            if (dbBackup is not null)
                return;

            dbFileLastModified = File.GetLastWriteTimeUtc(dbFile);

            dbBackup
                = Path.ChangeExtension(dbFile, "").TrimEnd('.')
                + $"_backup_{DateTime.UtcNow:O}".Replace(':', '-').Replace('.', '-')
                + Path.GetExtension(dbFile);
            File.Copy(dbFile, dbBackup);
        }

        private void deleteUnneededBackups()
        {
            var newLastModified = File.GetLastWriteTimeUtc(dbFile);
            if (dbFileLastModified == newLastModified)
            {
                File.Delete(dbBackup);
                dbBackup = null;
            }
        }

        void query(string sql)
        {
            // ef doesn't support truly generic queries. have to drop down to ado.net
            using var context = DbContexts.GetContext();
            using var conn = context.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var reader = cmd.ExecuteReader();
            var results = 0;
            var builder = new System.Text.StringBuilder();
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
                    sqlResultsTb.AppendText(builder.ToString());
                    builder.Clear();
                }
            }

            sqlResultsTb.AppendText(builder.ToString());
            builder.Clear();

            if (results == 0)
                sqlResultsTb.Text = "[no results]";
            else
            {
                sqlResultsTb.AppendText($"\r\n{results} result");
                if (results != 1) sqlResultsTb.AppendText("s");
            }
        }

        void nonQuery(string sql)
        {
            using var context = DbContexts.GetContext();
            var results = context.Database.ExecuteSqlRaw(sql);

            sqlResultsTb.AppendText($"{results} record");
            if (results != 1) sqlResultsTb.AppendText("s");
            sqlResultsTb.AppendText(" affected");
        }
    }
}
