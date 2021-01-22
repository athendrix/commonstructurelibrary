using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    /// <summary>
    /// Uses PostgreSQL to provide access to a shared Key/Value store and logging functionality.
    /// </summary>
    public class GlobalDB
    {
        /// <summary>
        /// The most recent GlobalDB instance that was created. 
        /// </summary>
        public static GlobalDB LatestDB = null;
        private bool createtables;

        private readonly string Server;
        private readonly string Database;
        private readonly string username;
        private readonly string password;
        private readonly string Schema;
        private readonly Npgsql.SslMode sslMode;
        public GlobalDB(string Server, string Database, string username, string password, string Schema = null, Npgsql.SslMode sslMode = Npgsql.SslMode.Prefer)
        {
            this.Server = Server;
            this.Database = Database;
            this.username = username;
            this.password = password;
            this.Schema = Schema;
            this.sslMode = sslMode;
            createtables = false;
            LatestDB = this;
        }

        public async Task<string> Get(string key)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                CreateDB(sql);
                return await sql.ExecuteScalar<string>(
                    "INSERT INTO \"Settings\" (\"Key\",\"Value\") VALUES (@key,null) ON CONFLICT DO NOTHING; " +
                    "SELECT \"VALUE\" FROM \"Settings\" WHERE \"KEY\" = @key;", new Dictionary<string, object> { { "@key", key } });
            }
        }
        public async Task Set(string key, string value)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                CreateDB(sql);
                await sql.ExecuteNonQuery("INSERT INTO \"Settings\" (\"Key\",\"Value\") VALUES (@key,@value) ON CONFLICT(\"Key\") DO UPDATE SET \"Value\" = EXCLUDED.\"Value\";", new Dictionary<string, object> { { "@key", key }, { "@value", value } });
            }
        }

        public async Task Log(string Message, string LogEntryType = "Error", int eventID = 0, int categoryID = 0, byte[] rawData = null)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                CreateDB(sql);
                await sql.ExecuteNonQuery("INSERT INTO \"Log\" (\"Timestamp\",\"Message\",\"EntryType\",\"EventID\",\"CategoryID\",\"RawData\") VALUES (@timestamp,@message,@entrytype,@eventid,@categoryid,@rawdata);",
                new Dictionary<string, object> { { "@timestamp", DateTime.Now }, { "@message", Message }, { "@entrytype", LogEntryType }, { "@eventid", eventID }, { "@categoryid", categoryID }, { "@rawdata", rawData } });
            }

        }
        private void CreateDB(SQL sql)
        {
            if (!createtables)
            {
                sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS \"Settings\" ( \"Key\" VARCHAR(255) NOT NULL UNIQUE, \"Value\" TEXT, PRIMARY KEY(\"Key\") ); " +
                "CREATE TABLE IF NOT EXISTS \"Log\" ( \"Timestamp\" TIMESTAMP NOT NULL UNIQUE, \"Message\" TEXT, \"EntryType\" TEXT, \"EventID\" INT, \"CategoryID\" INT, \"RawData\" BYTEA, PRIMARY KEY(\"Timestamp\"));");
                createtables = true;
            }
        }
    }
}
