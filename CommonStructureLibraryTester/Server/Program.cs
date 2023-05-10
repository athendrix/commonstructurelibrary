using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonStructureLibraryTester.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CSL.DependencyInjection.NpgsqlConnectionConstructor = (x) => new Npgsql.NpgsqlConnection(x);
            CSL.DependencyInjection.NpgsqlConnectionStringConstructor = () => new Npgsql.NpgsqlConnectionStringBuilder();
            CSL.DependencyInjection.SslModeConverter = (x) => Enum.Parse(typeof(Npgsql.SslMode), x.ToString());

            CSL.DependencyInjection.SqliteConnectionConstructor = (x) => new Microsoft.Data.Sqlite.SqliteConnection(x);
            CSL.DependencyInjection.SqliteConnectionStringConstructor = () => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
            CSL.DependencyInjection.SqliteOpenModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteOpenMode)x;
            CSL.DependencyInjection.SqliteCacheModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteCacheMode)x;

            CSL.DependencyInjection.AesGcmConstructor = (x) => new System.Security.Cryptography.AesGcm(x);

            CommonStructureLibraryTester.Testing.SQLTests.GetTestDB.Add(() => CSL.SQL.PostgreSQL.Connect("localhost", "testdb", "testuser", "testpassword", "testschema"));
            CommonStructureLibraryTester.Testing.SQLTests.GetTestDB.Add(() => CSL.SQL.PostgreSQL.Connect("localhost:5432", "testdb", "testuser", "testpassword", "testschema"));
            CommonStructureLibraryTester.Testing.SQLTests.GetTestDB.Add(async () =>
            {
                Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection("Host=localhost;Database=testdb;Username=testuser;Password=testpassword;SSL Mode=Prefer;Trust Server Certificate=False");
                CSL.SQL.PostgreSQL sql = new CSL.SQL.PostgreSQL(connection);
                await sql.SetSchema("testschema");
                return sql;
            });
            CommonStructureLibraryTester.Testing.SQLTests.GetSqliteDB.Add(() => new CSL.SQL.Sqlite(":memory:"));

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
