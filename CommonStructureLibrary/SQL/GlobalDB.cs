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
        public async Task<GlobalDB> InitDB()
        {
            if (!createtables)
            {
                using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
                {
                    await sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS \"Settings\" ( \"Key\" VARCHAR(255) NOT NULL UNIQUE, \"Value\" TEXT, PRIMARY KEY(\"Key\") ); " +
                    "CREATE TABLE IF NOT EXISTS \"KVStore\" ( \"Key\" UUID NOT NULL, \"Value\" BYTEA, PRIMARY KEY(\"Key\") ); " +
                    "CREATE TABLE IF NOT EXISTS \"Log\" ( \"Timestamp\" TIMESTAMP NOT NULL UNIQUE, \"Message\" TEXT, \"EntryType\" TEXT, \"EventID\" INT, \"CategoryID\" INT, \"RawData\" BYTEA, PRIMARY KEY(\"Timestamp\"));");
                    createtables = true;
                }
            }
            return this;
        }
        #region Settings
        public async Task<string> Get(string key)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                return await sql.ExecuteScalar<string>(
                    //"INSERT INTO \"Settings\" (\"Key\",\"Value\") VALUES (@key,null) ON CONFLICT DO NOTHING; " +
                    "SELECT \"Value\" FROM \"Settings\" WHERE \"Key\" = @key;", new Dictionary<string, object> { { "@key", key } });
            }
        }
        public async Task Set(string key, string value)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                if (value == null)
                {
                    await sql.ExecuteNonQuery("DELETE FROM \"Settings\" WHERE \"Key\" =  @key;", new Dictionary<string, object> { { "@key", key } });
                }
                else
                {
                    await sql.ExecuteNonQuery("INSERT INTO \"Settings\" (\"Key\",\"Value\") VALUES (@key,@value) ON CONFLICT(\"Key\") DO UPDATE SET \"Value\" = EXCLUDED.\"Value\";", new Dictionary<string, object> { { "@key", key }, { "@value", value } });
                }
            }
        }
        public async Task ClearSettings()
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                await sql.ExecuteNonQuery("TRUNCATE \"Settings\";");
            }
        }
        #endregion
        #region KVStore
        public Task<T?> GetStruct<T>(Guid key) where T : struct
        {
            return Get<T?>(key, (x) =>
            {
                if (x == null) { return null; }
                using (MemoryStream ms = new MemoryStream(x))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    return br.ReadStruct<T>();
                }
            });
        }
        public async Task<T> Get<T>(Guid key, Func<byte[],T> Create)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                byte[] data = await sql.ExecuteScalar<byte[]>(
                    //"INSERT INTO \"KVStore\" (\"Key\",\"Value\") VALUES (@key,null) ON CONFLICT DO NOTHING; " +
                    "SELECT \"Value\" FROM \"KVStore\" WHERE \"Key\" = @key;", new Dictionary<string, object> { { "@key", key } });
                return Create(data);
            }
        }
        public Task Set<T>(Guid key, T value) where T : IBinaryWritable => Set(key, value, (x) => (value?.ToByteArray()));
        public Task Set(Guid key, byte[] value) => Set(key, value, (x) => x);
        public Task SetStruct<T>(Guid key, T? value) where T : struct
        {
            return Set(key, value, (x) =>
            {
                if (x == null) { return null; }
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.WriteStruct(value.Value);
                    return ms.ToArray();
                }
            });
        }
        public async Task Set<T>(Guid key, T value, Func<T,byte[]> ToByteArray )
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                byte[] data = ToByteArray(value);
                if (data == null)
                {
                    await sql.ExecuteNonQuery("DELETE FROM \"KVStore\" WHERE \"Key\" =  @key;", new Dictionary<string, object> { { "@key", key } });
                }
                else
                {
                    await sql.ExecuteNonQuery("INSERT INTO \"KVStore\" (\"Key\",\"Value\") VALUES (@key,@value) ON CONFLICT(\"Key\") DO UPDATE SET \"Value\" = EXCLUDED.\"Value\";", new Dictionary<string, object> { { "@key", key }, { "@value", data } });
                }
            }
        }
        public async Task ClearKVStore()
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                await sql.ExecuteNonQuery("TRUNCATE \"KVStore\";");
            }
        }
        #endregion
        public async Task Log(string Message, string LogEntryType = "Error", int eventID = 0, int categoryID = 0, byte[] rawData = null)
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                await sql.ExecuteNonQuery("INSERT INTO \"Log\" (\"Timestamp\",\"Message\",\"EntryType\",\"EventID\",\"CategoryID\",\"RawData\") VALUES (@timestamp,@message,@entrytype,@eventid,@categoryid,@rawdata);",
                new Dictionary<string, object> { { "@timestamp", DateTime.Now }, { "@message", Message }, { "@entrytype", LogEntryType }, { "@eventid", eventID }, { "@categoryid", categoryID }, { "@rawdata", rawData } });
            }

        }
        public async Task ClearLogs()
        {
            using (SQL sql = await PostgreSQL.Connect(Server, Database, username, password, Schema, sslMode))
            {
                await sql.ExecuteNonQuery("TRUNCATE \"Log\";");
            }
        }
    }
}
