using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using CSL.Encryption;
using CSL.SQL;
using CSL.Testing;

namespace CommonStructureLibraryTester.Testing
{
    public class SQLTests : Tests
    {
        public static readonly List<Func<Task<PostgreSQL>>> GetTestDB = new List<Func<Task<PostgreSQL>>>();
        public static readonly List<Func<Sqlite>> GetSqliteDB = new List<Func<Sqlite>>();
        public static Task ClearData(PostgreSQL sql) => Task.CompletedTask;//sql.ExecuteNonQuery("DROP SCHEMA IF EXISTS testschema CASCADE; CREATE SCHEMA testschema; SET search_path to testschema;");
        public static Task ClearData(Sqlite sql) => sql.ExecuteNonQuery("PRAGMA writable_schema = 1; DELETE FROM sqlite_master; PRAGMA writable_schema = 0; PRAGMA integrity_check;");
        #region SQL
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> LocalDBTest()
        {
            using (LocalDB localDB = new LocalDB(":memory:"))
            {
                await localDB.Set("TestKey", "TestValue");
                if((await localDB.Get("TestKey")) == "TestValue")
                {
                    return PASS();
                }
                return FAIL("TestValue did not read correctly from the database!");
            }
        }
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> ProtectedDBTest()
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            AES256KeyBasedProtector protector = new AES256KeyBasedProtector(key);
            using (LocalDB localDB = new LocalDB(":memory:"))
            using (ProtectedDB protectedDB = new ProtectedDB(localDB, protector))
            {
                await protectedDB.Set("TestKey", "TestValue");
                if( (await protectedDB.Get("TestKey")) == "TestValue")
                {
                    return PASS();
                }
                return FAIL("TestValue did not read correctly from the database!");
            }
        }
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> PostgresTest()
        {
            if(GetTestDB.Count == 0) { return FAIL("No Test Databases loaded!"); }
            for (int i = 0; i < GetTestDB.Count; i++)
            {
                using (PostgreSQL sql = await GetTestDB[i]())
                {
                    await ClearData(sql);
                    string? version = await sql.ExecuteScalar<string>("SELECT version();");
                    await sql.ExecuteNonQuery("CREATE TABLE Settings ( Key VARCHAR(255) NOT NULL UNIQUE, Value TEXT, PRIMARY KEY(Key) ); ");
                    await sql.ExecuteNonQuery("INSERT INTO Settings VALUES(@0,@1);", "SomeKey", "SomeValue");
                    if(await sql.ExecuteScalar<string>("SELECT Value from Settings WHERE Key = @0", "SomeKey") != "SomeValue")
                    {
                        FAIL("Failed to store and retrieve a simple value!");
                    }
                    await ClearData(sql);
                }
            }
            return PASS();

        }
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> GlobalDBStructTest()
        {
            if (GetTestDB.Count == 0) { return FAIL("No Test Databases loaded!"); }
            for (int i = 0; i < GetTestDB.Count; i++)
            {
                using (PostgreSQL sql = await GetTestDB[i]())
                {
                    await ClearData(sql);
                    using (GlobalDB db = new GlobalDB(sql))
                    {
                        Guid key = Guid.NewGuid();
                        int testint = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
                        await db.ClearKVStore();
                        if (await db.GetStruct<int>(key) != null)
                        {
                            return FAIL("Key was not null after clearing!");
                        }
                        await db.SetStruct<int>(key, testint);
                        int? test2int = await db.GetStruct<int>(key);
                        if (!test2int.HasValue || testint != test2int.Value)
                        {
                            return FAIL("Value did not roundtrip correctly!");
                        }
                    }
                    //await ClearData(sql);
                }
            }
            return PASS();
        }
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> GlobalDBValTest1()
        {
            if (GetTestDB.Count == 0) { return FAIL("No Test Databases loaded!"); }
            for (int i = 0; i < GetTestDB.Count; i++)
            {
                using (PostgreSQL sql = await GetTestDB[i]())
                {
                    await ClearData(sql);
                    using (GlobalDB db = new GlobalDB(sql))
                    {
                        Func<byte[]?, Guid> converterA = (br) => br == null ? Guid.Empty : new Guid(br);
                        Func<Guid, byte[]?> converterB = (gd) => gd == Guid.Empty ? null : gd.ToByteArray();
                        Guid key = Guid.NewGuid();
                        Guid value = Guid.NewGuid();
                        await db.ClearKVStore();
                        if (await db.Get(key, converterA) != Guid.Empty)
                        {
                            return FAIL("Key was not null after clearing!");
                        }
                        await db.Set(key, value, converterB);
                        Guid TestVal = await db.Get(key, converterA);
                        if (TestVal != value)
                        {
                            return FAIL("Value did not roundtrip correctly!");
                        }
                    }
                    //await ClearData(sql);
                }
            }
            return PASS();
        }
        [Priority(1)]
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> SqliteTest()
        {
            using (SQLDB sql = new Sqlite(":memory:"))
            {
                await sql.ExecuteNonQuery("CREATE TABLE TESTTABLE(Words);");
                return PASS();
            }
        }
        #endregion
    }
}
