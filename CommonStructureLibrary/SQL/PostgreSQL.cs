using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public class PostgreSQL : SQL
    {
        public static bool TrustAllServerCertificates;
        public static async Task<PostgreSQL> Connect(string Server, string Database, string username, string password, string Schema = null, SslMode SslMode = SslMode.Prefer)
        {
            PostgreSQL toReturn = new PostgreSQL(Server, Database, username, password, SslMode);
            if (Schema != null)
            {
                await toReturn.ExecuteNonQuery("CREATE SCHEMA IF NOT EXISTS \"" + Common.NameParser(Schema) + "\"; SET search_path to \"" + Common.NameParser(Schema) + "\";");
            }
            return toReturn;
        }
        private PostgreSQL(string Server, string Database, string username, string password, SslMode SslMode)
        {
            TrustAllServerCertificates = true;
            NpgsqlConnectionStringBuilder csb = new NpgsqlConnectionStringBuilder()
            {
                Host = Server,
                Database = Database,
                //SearchPath = Schema,
                Username = username,
                Password = password,
                SslMode = SslMode,
                TrustServerCertificate = TrustAllServerCertificates,
            };
            currentTransaction = null;
            InternalConnection = new NpgsqlConnection(csb.ConnectionString);
            InternalConnection.Open();
        }
    }
}
