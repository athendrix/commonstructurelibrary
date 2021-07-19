using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using CSL.Data;
using CSL.Encryption;
using CSL.Helpers;
using CSL.SQL;
using CSL.Webserver;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CommonStructureLibraryTester
{
    public static class Tester
    {
        #region Init
        public static Func<Task<PostgreSQL>> GetTestDB = null;
        public static Func<Task<PostgreSQL>> GetTestDB2 = null;
        public static void RunTests()
        {
            int passcount = 0;
            int failedcount = 0;
            int testcount = Tests.Count;
            for(int i = 0; i < testcount; i++)
            {
                Tuple<string, Func<bool>> Test = Tests[i];
                Console.Write("Test " + Test.Item1 + ":");
                if(Test.Item2())
                {
                    passcount++;
                }
                else
                {
                    failedcount++;
                }
            }
            Console.WriteLine("------------------------------");
            Console.WriteLine("PASSED:" + passcount);
            Console.WriteLine("FAILED:" + failedcount);
            Console.WriteLine("TOTAL:" + testcount);
        }
        public static List<Tuple<string,Func<bool>>> Tests = new List<Tuple<string, Func<bool>>>();
        static Tester()
        {
            Type myType = typeof(Tester);
            MemberFilter filter = (MemberInfo m, object o) =>
            {
                if(m is MethodInfo mi)
                {
                    return mi.ReturnType == typeof(bool) && mi.GetParameters().Length == 0;
                }
                return false;
            };
            MemberInfo[] list = myType.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public, filter, null);
            for(int i = 0; i < list.Length; i++)
            {
                if(list[i] is MethodInfo m)
                {
                    Tests.Add(new Tuple<string, Func<bool>>(m.Name, () =>(bool)m.Invoke(null, null)));
                }
            }
        }
        #endregion
        #region Testing Helpers
        public static bool SyncTest(Func<bool> test)
        {
            try
            {
                if (test())
                {
                    Console.WriteLine("PASSED");
                    return true;
                }
                else
                {
                    Console.WriteLine("FAILED");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("EXCEPTION");
                PrintException(e);
            }
            return false;
        }
        public static bool AsyncTest(Func<Task<bool>> test)
        {
            try
            {
                if (Task.Run(async () => await test()).Result)
                {
                    Console.WriteLine("PASSED");
                    return true;
                }
                else
                {
                    Console.WriteLine("FAILED");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION");
                PrintException(e);
            }
            return false;
        }
        public static void PrintException(Exception e, int tabcount = 0)
        {
            if(e == null)
            {
                return;
            }
            Console.WriteLine(new string('\t',tabcount) + e.Message);
            Console.WriteLine(new string('\t', tabcount) + e.StackTrace);
            if (e is AggregateException ae)
            {
                foreach(Exception aesub in ae.InnerExceptions)
                {
                    Console.WriteLine(new string('\t', tabcount) + "InnerException:");
                    PrintException(aesub, tabcount + 1);
                }
                return;
            }
            if (e.InnerException != null)
            {
                Console.WriteLine(new string('\t', tabcount) + "InnerException:");
                PrintException(e.InnerException,tabcount + 1);
            }
        }
        #endregion
        public static bool TestTest() => SyncTest(()=>true);
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
            using (ProtectedDB protectedDB = new ProtectedDB(localDB,protector))
            {
                await protectedDB.Set("TestKey", "TestValue");
                return (await protectedDB.Get("TestKey")) == "TestValue";
            }
        });

        public static bool PostgresTest() => AsyncTest(async () =>
        {
            using (PostgreSQL sql = await GetTestDB())
            {
                string version = await sql.ExecuteScalar<string>("SELECT version();");
                await sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Settings ( Key VARCHAR(255) NOT NULL UNIQUE, Value TEXT, PRIMARY KEY(Key) ); ");

                return true;
            }
        });
        public static bool PostgresTest2() => AsyncTest(async () =>
        {
            using (PostgreSQL sql = await GetTestDB2())
            {
                string version = await sql.ExecuteScalar<string>("SELECT version();");
                await sql.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Settings ( Key VARCHAR(255) NOT NULL UNIQUE, Value TEXT, PRIMARY KEY(Key) ); ");

                return true;
            }
        });
        public static bool GlobalDBStructTest1() => AsyncTest(async () =>
        {
            using (GlobalDB db = new GlobalDB(await GetTestDB()))
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
                return test2int.HasValue && testint == test2int.Value;
            }
        });
        public static bool GlobalDBStructTest2() => AsyncTest(async () =>
        {
            using (GlobalDB db = new GlobalDB(await GetTestDB()))
            {
                Guid key = Guid.NewGuid();
                DateTime Now = DateTime.Now;
                await db.ClearKVStore();
                if (await db.GetStruct<DateTime>(key) != null)
                {
                    return false;
                }
                await db.SetStruct<DateTime>(key, Now);
                DateTime? TestNow = await db.GetStruct<DateTime>(key);
                return TestNow.HasValue && Now == TestNow.Value;
            }
        });
        public static bool GlobalDBValTest1() => AsyncTest(async () =>
        {
            using (GlobalDB db = new GlobalDB(await GetTestDB()))
            {
                Func<byte[], Guid> converterA = (br) => br == null ? Guid.Empty : new Guid(br);
                Func<Guid, byte[]> converterB = (gd) => gd == Guid.Empty ? null : gd.ToByteArray();
                Guid key = Guid.NewGuid();
                Guid value = Guid.NewGuid();
                await db.ClearKVStore();
                if (await db.Get(key, converterA) != Guid.Empty)
                {
                    return false;
                }
                await db.Set(key, value, converterB);
                Guid TestVal = await db.Get(key, converterA);
                return TestVal == value;
            }
        });
        #endregion
        #region Encryption
        public static bool FriendlyPasswordTest() => SyncTest(() =>
        {
            HashSet<string> testAdj = new HashSet<string>();
            testAdj.UnionWith(Passwords.Adjectives);
            if(testAdj.Count != 256)//256 Unique Values
            {
                return false;
            }
            HashSet<string> testPoke = new HashSet<string>();
            testPoke.UnionWith(Passwords.Pokemon);
            if(testPoke.Count != 256)//256 Unique Values
            {
                return false;
            }
            HashSet<string> testVerb = new HashSet<string>();
            testVerb.UnionWith(Passwords.Verbs);
            if (testVerb.Count != 256)//256 Unique Values
            {
                return false;
            }
            Regex r = new Regex("^[A-Z][a-z]+$",RegexOptions.Compiled);
            for(int i = 0; i < 256; i++)
            {
                if (!r.IsMatch(Passwords.Adjectives[i]))
                {
                    Console.WriteLine("\"" + Passwords.Adjectives[i] + "\" is not a proper password term.");
                    return false;
                }
                if (!r.IsMatch(Passwords.Pokemon[i]))
                {
                    Console.WriteLine("\"" + Passwords.Pokemon[i] + "\" is not a proper password term.");
                    return false;
                }
                if (!r.IsMatch(Passwords.Verbs[i]))
                {
                    Console.WriteLine("\"" + Passwords.Verbs[i] + "\" is not a proper password term.");
                    return false;
                }
            }
            return Passwords.FriendlyPassGen() != null &&
            Passwords.FriendlyPassPhrase40Bit() != null &&
            Passwords.FriendlyPassPhrase56Bit() != null &&
            Passwords.ThreeLetterWordPassword(5) != null;
        });
        #endregion
        #region Other Helpers
        public static bool GenericsTest1()
        {
            return SyncTest(() =>
            {
                bool toReturn = true;
                int testint = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
                toReturn &= Generics.TryParse(Generics.ToString(testint), out int outtest) && testint == outtest;
                toReturn &= Generics.TryParse(Generics.ToString(int.MinValue), out int outmin) && outmin == int.MinValue;
                toReturn &= Generics.TryParse(Generics.ToString(int.MaxValue), out int outmax) && outmax == int.MaxValue;
                return toReturn;
            });
        }
        #endregion
    }
}
