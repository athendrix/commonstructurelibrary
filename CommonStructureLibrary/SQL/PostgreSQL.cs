using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using static CSL.DependencyInjection;
using static CSL.Helpers.Generics;

namespace CSL.SQL
{
    public class PostgreSQL : SQLDB
    {
        public static bool TrustAllServerCertificates = false;
        /// <summary>
        /// Connects to a Postgres Database using the parameters given.
        /// </summary>
        /// <param name="Server">The Server to connect to.</param>
        /// <param name="Database">The Database to use on the Server.</param>
        /// <param name="username">The username to use to connect to the server.</param>
        /// <param name="password">The password to use to connect to the server.</param>
        /// <param name="Schema">The default Schema to use in the Database. `null` reverts to checking for a schema matching the username first, and then falls back to the public schema.</param>
        /// <param name="SslMode">The SSLMode for connecting to the server. This defaults to `Prefer` for compatibility, but if your Postgres server is on another host and you have SSL configured, you might consider using `Require` instead.</param>
        /// <returns>A connection to a PostgreSQL server.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<PostgreSQL> Connect(string Server, string Database, string username, string password, string? Schema = null, SslMode SslMode = SslMode.Prefer)
        {
            INpgsqlConnectionStringBuilder csb = CreateINpgsqlConnectionStringBuilder();
            if (Server.Contains(":"))
            {
                string[] serversplit = Server.Split(':');
                string dumbtest = serversplit[0].ToLower();
                if (dumbtest is "http" or "https")
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
            csb.Username = username;
            csb.Password = password;
            csb.SslMode = SslMode;
            csb.Pooling = false;
            csb.TrustServerCertificate = TrustAllServerCertificates;

            PostgreSQL toReturn = new PostgreSQL(CreateNpgsqlConnection(csb.ConnectionString));
            await toReturn.SetSchema(Schema);
            return toReturn;
        }
        /// <summary>
        /// Connects to a Postgres Database using Environment Variables
        /// POSTGRES_HOST sets the Server to connect to with a default of `localhost`
        /// POSTGRES_DB sets the Database to use on the Server with a default of matching the POSTGRES_USER
        /// POSTGRES_USER sets the username to use to connect to the server with a default of `postgres`
        /// POSTGRES_PASSWORD sets the password to use to connect to the server with no default. This must be set at a minimum.
        /// POSTGRES_SCHEMA sets the default Schema to use in the Database. The default checks for a schema matching the username first, and then falls back to the public schema.
        /// POSTGRES_SSLMODE sets the SSLMode for connecting to the server. This defaults to `Prefer` for compatibility, but if your Postgres server is on another host and you have SSL configured, you might consider using `Require` instead.
        /// </summary>
        /// <returns>A connection to a PostgreSQL server.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Task<PostgreSQL> Connect() =>
            Connect(
                Data.Env.Vars["POSTGRES_HOST"] ?? "localhost",
                Data.Env.Vars["POSTGRES_DB"] ?? Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                Data.Env.Vars["POSTGRES_USER"] ?? "postgres",
                Data.Env.Vars["POSTGRES_PASSWORD"] ?? throw new ArgumentException("POSTGRES_PASSWORD must be specified as an Environment Variable!)"),
                Data.Env.Vars["POSTGRES_SCHEMA"],
                ParseSSLMode(Data.Env.Vars["POSTGRES_SSLMODE"])
                );
        private static SslMode ParseSSLMode(string? SSLMODE) => SSLMODE == null ? SslMode.Prefer : (SslMode)Enum.Parse(typeof(SslMode), char.ToUpperInvariant(SSLMODE[0]) + SSLMODE.ToLowerInvariant().Substring(1));
        public PostgreSQL(DbConnection connection) : base(connection)
        {
            currentTransaction = null;
            InternalConnection.Open();
        }
        public Task SetSchema(string? Schema = null)
        {
            if (Schema != null)
            {
                return ExecuteNonQuery("CREATE SCHEMA IF NOT EXISTS \"" + Common.NameParser(Schema) + "\"; SET search_path to \"" + Common.NameParser(Schema) + "\";");
            }
            else
            {
                return ExecuteNonQuery("SET search_path to \"$user\", public;");
            }
        }
        #region Abstract Implementations
        public override object? ConvertToFriendlyParameter(object? parameter)
        {
            object? toReturn = parameter;
            Type? ParameterType = toReturn?.GetType();
            if (ParameterType == null) { return toReturn; }
            ParameterType = Nullable.GetUnderlyingType(ParameterType) ?? ParameterType;
            if (ParameterType.IsEnum)
            {
                ParameterType = Enum.GetUnderlyingType(ParameterType);
                toReturn = Convert.ChangeType(toReturn, ParameterType);
            }
            if (ParameterType == typeof(char))
            {
                char? val = (char?)toReturn;
                return val != null ? new string(val.Value, 1) : null;
            }
            if (ParameterType == typeof(ushort)) { return (short?)(ushort?)toReturn; }
            if (ParameterType == typeof(uint)) { return (int?)(uint?)toReturn; }
            if (ParameterType == typeof(ulong)) { return (long?)(ulong?)toReturn; }
            return toReturn;
        }

        public override object? ConvertFromFriendlyParameter(object? parameter, ParameterInfo pi)
        {
            if (parameter is null or DBNull)
            {
                if (!IsNullable(pi)) { throw new Exception("ParameterInfo does not match parameter!"); }
                return null;
            }
            Type ParameterType = pi.ParameterType;
            ParameterType = Nullable.GetUnderlyingType(ParameterType) ?? ParameterType;
            if (ParameterType == typeof(byte)) { return (byte)(short)parameter; }
            if (ParameterType == typeof(sbyte)) { return (sbyte)(short)parameter; }
            if (ParameterType == typeof(char)) { return ((string)parameter)[0]; }
            if (ParameterType == typeof(ushort)) { return (ushort)(short)parameter; }
            if (ParameterType == typeof(uint)) { return (uint)(int)parameter; }
            if (ParameterType == typeof(ulong)) { return (ulong)(long)parameter; }
            if (ParameterType.IsEnum) { return Enum.ToObject(ParameterType, Convert.ChangeType(parameter, Enum.GetUnderlyingType(ParameterType))); }
            return parameter;
        }
        private static readonly Dictionary<Type, string> SQLTypes = new Dictionary<Type, string>()
    {
        {typeof(bool),      "BOOLEAN"},
        {typeof(bool?),     "BOOLEAN"},
        {typeof(sbyte),     "SMALLINT"},
        {typeof(sbyte?),    "SMALLINT"},
        {typeof(byte),      "SMALLINT"},
        {typeof(byte?),     "SMALLINT"},
        {typeof(char),      "CHAR(1)"}, //Small bit of C# conversion required
        {typeof(char?),     "CHAR(1)"}, //Small bit of C# conversion required
        {typeof(short),     "SMALLINT"},
        {typeof(short?),    "SMALLINT"},
        {typeof(ushort),    "SMALLINT"},
        {typeof(ushort?),   "SMALLINT"},
        {typeof(int),       "INTEGER"},
        {typeof(int?),      "INTEGER"},
        {typeof(uint),      "INTEGER"},
        {typeof(uint?),     "INTEGER"},
        {typeof(long),      "BIGINT"},
        {typeof(long?),     "BIGINT"},
        {typeof(ulong),     "BIGINT"},
        {typeof(ulong?),    "BIGINT"},
        {typeof(float),     "FLOAT4"},
        {typeof(float?),    "FLOAT4"},
        {typeof(double),    "FLOAT8"},
        {typeof(double?),   "FLOAT8"},
        {typeof(decimal),   "NUMERIC"},
        {typeof(decimal?),  "NUMERIC"},
        {typeof(Guid),      "UUID"},
        {typeof(Guid?),     "UUID"},
        {typeof(DateTime),  "TIMESTAMP"},
        {typeof(DateTime?), "TIMESTAMP"},
        {typeof(string),    "TEXT"},
        {typeof(byte[]),    "BYTEA" }
    };
        public override string? GetSQLType(Type t) => SQLTypes.ContainsKey(t) ? SQLTypes[t] : null;
        public override Task Truncate(string TableName, bool cascade) => ExecuteNonQuery($"TRUNCATE {TableName}{(cascade ? " CASCADE" : "")};");
        #endregion
    }
}
