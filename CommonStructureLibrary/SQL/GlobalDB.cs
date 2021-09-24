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
    public class GlobalDB : IDisposable
    {
        private readonly SQLDB sql;
        /// <summary>
        /// The most recent GlobalDB instance that was created. 
        /// </summary>
        //public static GlobalDB LatestDB = null;
        private bool createtables;

        public GlobalDB(PostgreSQL innerSQL)
        {
            sql = innerSQL;
            createtables = false;
            //LatestDB = this;
        }
        public async Task InitDB()
        {
            if (!createtables)
            {
                await sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS \"Settings\" ( \"Key\" VARCHAR(255) NOT NULL UNIQUE, \"Value\" TEXT, PRIMARY KEY(\"Key\") ); " +
                "CREATE TABLE IF NOT EXISTS \"KVStore\" ( \"Key\" UUID NOT NULL, \"Value\" BYTEA, PRIMARY KEY(\"Key\") ); " +
                "CREATE TABLE IF NOT EXISTS \"Log\" ( \"Timestamp\" TIMESTAMP NOT NULL UNIQUE, \"Message\" TEXT, \"EntryType\" TEXT, \"EventID\" INT, \"CategoryID\" INT, \"RawData\" BYTEA, PRIMARY KEY(\"Timestamp\"));");
                createtables = true;
            }
        }
        #region Settings
        public async Task<string?> Get(string key)
        {
            await InitDB();
            return await sql.ExecuteScalar<string>("SELECT \"Value\" FROM \"Settings\" WHERE \"Key\" = @0;", key);
        }
        public async Task Set(string key, string? value)
        {
            await InitDB();
            if (value == null)
            {
                await sql.ExecuteNonQuery("DELETE FROM \"Settings\" WHERE \"Key\" =  @0;", key);
            }
            else
            {
                await sql.ExecuteNonQuery("INSERT INTO \"Settings\" (\"Key\",\"Value\") VALUES (@0,@1) ON CONFLICT(\"Key\") DO UPDATE SET \"Value\" = EXCLUDED.\"Value\";", key, value);
            }
        }
        public async Task ClearSettings()
        {
            await InitDB();
            await sql.ExecuteNonQuery("TRUNCATE \"Settings\";");
        }
        #endregion
        #region KVStore
        public Task<T?> GetStruct<T>(Guid key) where T : struct => Get<T?>(key, (x) =>
        {
            if (x == null) { return null; }
            using (MemoryStream ms = new MemoryStream(x))
            using (BinaryReader br = new BinaryReader(ms))
            {
                return br.ReadStruct<T>();
            }
        });
        public async Task<T?> Get<T>(Guid key, Func<byte[]?, T?> Create)
        {
            await InitDB();
            byte[]? data = await sql.ExecuteScalar<byte[]>("SELECT \"Value\" FROM \"KVStore\" WHERE \"Key\" = @0;", key);
            return Create(data);
        }
        public Task Set<T>(Guid key, T? value) where T : IBinaryWritable => Set(key, value, (x) => (value?.ToByteArray()));
        public Task Set(Guid key, byte[]? value) => Set(key, value, (x) => x);
        public Task SetStruct<T>(Guid key, T? value) where T : struct => Set(key, value, (x) =>
        {
            if (x == null) { return null; }
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.WriteStruct(x.Value);
                return ms.ToArray();
            }
        });
        public async Task Set<T>(Guid key, T? value, Func<T?, byte[]?> ToByteArray)
        {
            await InitDB();
            byte[]? data = ToByteArray(value);
            if (data == null)
            {
                await sql.ExecuteNonQuery("DELETE FROM \"KVStore\" WHERE \"Key\" =  @0;", key);
            }
            else
            {
                await sql.ExecuteNonQuery("INSERT INTO \"KVStore\" (\"Key\",\"Value\") VALUES (@0,@1) ON CONFLICT(\"Key\") DO UPDATE SET \"Value\" = EXCLUDED.\"Value\";", key, data);
            }
        }
        public async Task ClearKVStore()
        {
            await InitDB();
            await sql.ExecuteNonQuery("TRUNCATE \"KVStore\";");
        }

        #endregion
        public async Task Log(string Message, string LogEntryType = "Error", int eventID = 0, int categoryID = 0, byte[]? rawData = null)
        {
            await InitDB();
            await sql.ExecuteNonQuery("INSERT INTO \"Log\" (\"Timestamp\",\"Message\",\"EntryType\",\"EventID\",\"CategoryID\",\"RawData\") VALUES (@0,@1,@2,@3,@4,@5);",
                                                            DateTime.Now, Message, LogEntryType, eventID, categoryID, rawData);
        }
        public async Task ClearLogs()
        {
            await InitDB();
            await sql.ExecuteNonQuery("TRUNCATE \"Log\";");
        }

        public void Dispose() => ((IDisposable)sql).Dispose();
    }
}
