using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

using CSL.Encryption;
using CSL.SQL;

namespace CommonStructureLibraryTester
{
    public static partial class Tests
    {
        public static readonly List<Func<Task<PostgreSQL>>> GetTestDB = new List<Func<Task<PostgreSQL>>>();
        #region SQL
        public static bool LocalDBTest => AsyncTest(async () =>
        {
            using (LocalDB localDB = new LocalDB(":memory:"))
            {
                await localDB.Set("TestKey", "TestValue");
                return (await localDB.Get("TestKey")) == "TestValue";
            }
        });
        public static bool ProtectedDBTest => AsyncTest(async () =>
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            AES256KeyBasedProtector protector = new AES256KeyBasedProtector(key);
            using (LocalDB localDB = new LocalDB(":memory:"))
            using (ProtectedDB protectedDB = new ProtectedDB(localDB, protector))
            {
                await protectedDB.Set("TestKey", "TestValue");
                return (await protectedDB.Get("TestKey")) == "TestValue";
            }
        });

        public static bool PostgresTest() => AsyncTest(async () =>
        {
            for (int i = 0; i < GetTestDB.Count; i++)
            {
                using (PostgreSQL sql = await GetTestDB[i]())
                {
                    string? version = await sql.ExecuteScalar<string>("SELECT version();");
                    await sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Settings ( Key VARCHAR(255) NOT NULL UNIQUE, Value TEXT, PRIMARY KEY(Key) ); ");
                }
            }
            return true;

        });

        public static bool GlobalDBStructTest() => AsyncTest(async () =>
        {
            for (int i = 0; i < GetTestDB.Count; i++)
            {
                using (GlobalDB db = new GlobalDB(await GetTestDB[i]()))
                {
                    Guid key = Guid.NewGuid();
                    int testint = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
                    await db.ClearKVStore();
                    if (await db.GetStruct<int>(key) != null)
                    {
                        return false;
                    }
                    await db.SetStruct<int>(key, testint);
                    int? test2int = await db.GetStruct<int>(key);
                    if (!test2int.HasValue || testint != test2int.Value)
                    {
                        return false;
                    }
                }
            }
            return true;
        });
        public static bool GlobalDBValTest1() => AsyncTest(async () =>
        {
            for (int i = 0; i < GetTestDB.Count; i++)
            {
                using (GlobalDB db = new GlobalDB(await GetTestDB[i]()))
                {
                    Func<byte[]?, Guid> converterA = (br) => br == null ? Guid.Empty : new Guid(br);
                    Func<Guid, byte[]?> converterB = (gd) => gd == Guid.Empty ? null : gd.ToByteArray();
                    Guid key = Guid.NewGuid();
                    Guid value = Guid.NewGuid();
                    await db.ClearKVStore();
                    if (await db.Get(key, converterA) != Guid.Empty)
                    {
                        return false;
                    }
                    await db.Set(key, value, converterB);
                    Guid TestVal = await db.Get(key, converterA);
                    if (TestVal != value)
                    {
                        return false;
                    }
                }
            }
            return true;
        });
        #endregion
    }
}
