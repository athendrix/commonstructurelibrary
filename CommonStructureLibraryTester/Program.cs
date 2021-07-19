using CSL.Encryption;
using CSL.SQL;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CommonStructureLibraryTester
{
    class Program
    {
        public static void Main(string[] args)
        {
            //Dependency Injection
            CSL.DependencyInjection.NpgsqlConnectionConstructor = (x) => new Npgsql.NpgsqlConnection(x);
            CSL.DependencyInjection.NpgsqlConnectionStringConstructor = () => new Npgsql.NpgsqlConnectionStringBuilder();
            CSL.DependencyInjection.SslModeConverter = (x) => (Npgsql.SslMode)x;

            CSL.DependencyInjection.SqliteConnectionConstructor = (x) => new Microsoft.Data.Sqlite.SqliteConnection(x);
            CSL.DependencyInjection.SqliteConnectionStringConstructor = () => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
            CSL.DependencyInjection.SqliteOpenModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteOpenMode)x;
            CSL.DependencyInjection.SqliteCacheModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteCacheMode)x;

            CSL.DependencyInjection.AesGcmConstructor = (x) => new System.Security.Cryptography.AesGcm(x);

            Tester.GetTestDB = () => PostgreSQL.Connect("localhost", "testdb", "testuser", "testpassword", "testschema");
            Tester.GetTestDB2 = () => PostgreSQL.Connect("localhost:5432", "testdb", "testuser", "testpassword", "testschema");
            Tester.RunTests();
            //AsyncMain(args).GetAwaiter().GetResult();
        }
        public static async Task AsyncMain(string[] args)
        {
            Console.WriteLine("Enter Decryption Key (Enter no key to Encrypt with new key.)");
            string keybase = Console.ReadLine();
            if(string.IsNullOrWhiteSpace(keybase))
            {
                using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector())
                {
                    Console.WriteLine("Key:" + Convert.ToBase64String(protector.GetKey()));
                    Console.WriteLine("Enter Plain Text");
                    string PlaintextString = Console.ReadLine();
                    Console.WriteLine("Protected Text:");
                    Console.WriteLine(await protector.Protect(PlaintextString));
                }
            }
            else
            {
                byte[] key = Convert.FromBase64String(keybase);
                Console.WriteLine("Enter Protected Text");
                string ProtectedString = Console.ReadLine();
                using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector(key))
                {
                    Console.WriteLine("Decrypted Text:");
                    Console.WriteLine(await protector.Unprotect(ProtectedString));
                }
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
