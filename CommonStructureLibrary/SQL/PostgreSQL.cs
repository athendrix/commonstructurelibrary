//using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static CSL.DependencyInjection;

namespace CSL.SQL
{
    public class PostgreSQL : SQL
    {
        public static bool TrustAllServerCertificates = false;
        public static async Task<PostgreSQL> Connect(string Server, string Database, string username, string password, string Schema = null, SslMode SslMode = SslMode.Prefer)
        {
            PostgreSQL toReturn = new PostgreSQL(Server, Database, username, password, SslMode, TrustAllServerCertificates);
            if (Schema != null)
            {
                await toReturn.ExecuteNonQuery("CREATE SCHEMA IF NOT EXISTS \"" + Common.NameParser(Schema) + "\"; SET search_path to \"" + Common.NameParser(Schema) + "\";");
            }
            return toReturn;
        }
        private PostgreSQL(string Server, string Database, string username, string password, SslMode SslMode, bool TrustAllServerCertificates = false)
        {
            INpgsqlConnectionStringBuilder csb = CreateINpgsqlConnectionStringBuilder();
            if(Server.Contains(":"))
            {
                string[] serversplit = Server.Split(':');
                string dumbtest = serversplit[0].ToLower();
                if(dumbtest == "http" || dumbtest == "https")
                {
                    throw new ArgumentException("Postgres connections are not http connections.");
                }
                csb.Host = serversplit[0];
                if (int.TryParse(serversplit[1], out int port))
                {
                    csb.Port = port;
                }
            }
            else
            {
                csb.Host = Server;
            }
            csb.Database = Database;
            //csb.SearchPath = Schema;
            csb.Username = username;
            csb.Password = password;
            csb.SslMode = SslMode;
            csb.TrustServerCertificate = TrustAllServerCertificates;
            
            currentTransaction = null;
            InternalConnection = CreateNpgsqlConnection(csb.ConnectionString);
            InternalConnection.Open();
        }
    }
}
