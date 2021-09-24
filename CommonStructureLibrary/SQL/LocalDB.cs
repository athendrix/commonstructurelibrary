using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    /// <summary>
    /// Uses Sqlite to provide a local Key/Value store and logging functionality.
    /// </summary>
    public class LocalDB : IDisposable
    {
        private readonly SQLDB sql;
        public enum PathBase : int
        {
            ApplicationDirectory = 0,
            WorkingDirectory = 1,
            LocalAppData = 2,
            RomingAppData = 3,
            ProgramData = 4
        }
        private readonly static string[] DirectoryPaths = new string[]
        {
        AppDomain.CurrentDomain.BaseDirectory,
        Environment.CurrentDirectory + Path.DirectorySeparatorChar,
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar,
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar,
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar
        };
        //Settings.db
        private readonly string filename;
        private bool createtables;
        /// <summary>
        /// Creates a new LocalDB at the filepath.
        /// </summary>
        /// <param name="filepath">The full filepath to create or open the LocalDB.</param>
        public LocalDB(string filepath)
        {
            filename = filepath ?? throw new ArgumentNullException();
            if (filename != ":memory:")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
            createtables = false;
            sql = new Sqlite(filename);
        }
        /// <summary>
        /// Creates a new LocalDB at a predefined Well Known application directory.
        /// </summary>
        /// <param name="pathBase">The Well Known path to start from.</param>
        /// <param name="ApplicationFolderName">The subfolder into which to place the Local.db database. (Usually the same as the Application's name)</param>
        public LocalDB(PathBase pathBase, string ApplicationFolderName) : this(DirectoryPaths[(int)pathBase] + ApplicationFolderName + Path.DirectorySeparatorChar + "Local.db")
        {
        }

        public async Task<string?> Get(string key)
        {
            await CreateDB();
            return await sql.ExecuteScalar<string>(
                "INSERT OR IGNORE INTO `SETTINGS` (`KEY`,`VALUE`) VALUES (@0,null); " +
                "SELECT `VALUE` FROM `SETTINGS` WHERE `KEY` = @0;", key);
        }
        public async Task Set(string key, string? value)
        {
            await CreateDB();
            await sql.ExecuteNonQuery("INSERT OR REPLACE INTO `SETTINGS` (`KEY`,`VALUE`) VALUES (@0,@1);", key, value);
        }


        public async Task Log(string Message, string LogEntryType = "Error", int eventID = 0, int categoryID = 0, byte[]? rawData = null)
        {

            await CreateDB();
            await sql.ExecuteNonQuery("INSERT OR REPLACE INTO `LOG` (`TIMESTAMP`,`MESSAGE`,`ENTRYTYPE`,`EVENTID`,`CATEGORYID`,`RAWDATA`) VALUES (@0,@1,@2,@3,@4,@5);",
                                                                    DateTime.Now, Message, LogEntryType, eventID, categoryID, rawData);
        }

        private async Task CreateDB()
        {
            if (!createtables)
            {
                await sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `SETTINGS` ( `KEY` TEXT NOT NULL UNIQUE, `VALUE` TEXT, PRIMARY KEY(`KEY`) ); " +
                "CREATE TABLE IF NOT EXISTS `LOG` ( `TIMESTAMP` DATETIME NOT NULL UNIQUE, `MESSAGE` TEXT, `ENTRYTYPE` TEXT, `EVENTID` INTEGER, `CATEGORYID` INTEGER, `RAWDATA` BLOB, PRIMARY KEY(`TIMESTAMP`)  );");
                createtables = true;
            }
        }

        public void Dispose() => ((IDisposable)sql).Dispose();
    }
}
