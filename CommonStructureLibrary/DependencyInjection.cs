using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

namespace CSL
{
    public static class DependencyInjection
    {
        #region AesGcm
        private static DependencyMissingException AesDependencyException => new DependencyMissingException(
            "AesGcm dependency missing! Inject the dependency into the CSL.\n" +
            "This should look like:\n\n" +
            "--------------------------------------------------------------------------------\n" +
            "CSL.DependencyInjection.AesGcmConstructor = (x) => new System.Security.Cryptography.AesGcm(x);\n" +
            "--------------------------------------------------------------------------------\n\n" +
            "Put this line at the start of your program to use any methods that depend on AesGcm.");
        internal static IAesGcm CreateIAesGcm(byte[] key) => AesGcmConstructor == null ? throw AesDependencyException : new AesGcmWrapper(AesGcmConstructor(key));
        public static Func<byte[], dynamic>? AesGcmConstructor { private get; set; } = null;

        internal class AesGcmWrapper : IAesGcm
        {
            private readonly dynamic AesGcmInstance;
            public AesGcmWrapper(dynamic AesGcmInstance) => this.AesGcmInstance = AesGcmInstance;
            public void Decrypt(byte[] nonce, byte[] ciphertext, byte[] tag, byte[] plaintext, byte[]? associatedData = null) => AesGcmInstance.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
            public void Dispose() => AesGcmInstance.Dispose();
            public void Encrypt(byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag, byte[]? associatedData = null) => AesGcmInstance.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
        }
        internal interface IAesGcm : IDisposable
        {
            void Encrypt(byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag, byte[]? associatedData = default);
            void Decrypt(byte[] nonce, byte[] ciphertext, byte[] tag, byte[] plaintext, byte[]? associatedData = default);
        }
        #endregion
        #region Npgsql
        private static DependencyMissingException NpgsqlDependencyException => new DependencyMissingException(
            "NpgsqlConnection dependency missing! Inject the dependency into the CSL.\n" +
            "This should look like:\n\n" +
            "--------------------------------------------------------------------------------\n" +
            "CSL.DependencyInjection.NpgsqlConnectionConstructor = (x) => new Npgsql.NpgsqlConnection(x);\n" +
            "CSL.DependencyInjection.NpgsqlConnectionStringConstructor = () => new Npgsql.NpgsqlConnectionStringBuilder();\n" +
            "CSL.DependencyInjection.SslModeConverter = (x) => (Npgsql.SslMode)x;\n" +
            "--------------------------------------------------------------------------------\n\n" +
            "Put this line at the start of your program to use any methods that depend on Npgsql.");
        #region NpgsqlConnection
        internal static DbConnection CreateNpgsqlConnection(string ConnectionString) => NpgsqlConnectionConstructor == null ? throw NpgsqlDependencyException : NpgsqlConnectionConstructor(ConnectionString);
        public static Func<string, DbConnection>? NpgsqlConnectionConstructor { private get; set; } = null;
        #endregion
        #region NpgsqlConnectionStringBuilder
        internal static INpgsqlConnectionStringBuilder CreateINpgsqlConnectionStringBuilder() => NpgsqlConnectionStringConstructor == null ? throw NpgsqlDependencyException : new NpgsqlConnectionStringWrapper(NpgsqlConnectionStringConstructor());
        
        public static Func<SslMode, dynamic>? SslModeConverter { private get; set; } = null;
        public static Func<dynamic>? NpgsqlConnectionStringConstructor { private get; set; } = null;
        internal class NpgsqlConnectionStringWrapper : INpgsqlConnectionStringBuilder
        {
            private readonly dynamic NpgsqlConnectionStringInstance;
            public NpgsqlConnectionStringWrapper(dynamic NpgsqlConnectionStringInstance) => this.NpgsqlConnectionStringInstance = NpgsqlConnectionStringInstance;

            public string Host { set => NpgsqlConnectionStringInstance.Host = value; }
            public int Port { set => NpgsqlConnectionStringInstance.Port = value; }
            public string Database { set => NpgsqlConnectionStringInstance.Database = value; }
            public string Username { set => NpgsqlConnectionStringInstance.Username = value; }
            public string Password { set => NpgsqlConnectionStringInstance.Password = value; }
            public SslMode SslMode { set => NpgsqlConnectionStringInstance.SslMode = SslModeConverter == null ? throw NpgsqlDependencyException : SslModeConverter(value); }
            public bool TrustServerCertificate { set => NpgsqlConnectionStringInstance.TrustServerCertificate = value; }
            public bool Pooling { set => NpgsqlConnectionStringInstance.Pooling = value; }

            public string ConnectionString => NpgsqlConnectionStringInstance.ConnectionString;
        }
        internal interface INpgsqlConnectionStringBuilder
        {
            string Host { set; }
            int Port { set; }
            string Database { set; }
            string Username { set; }
            string Password { set; }
            SslMode SslMode { set; }
            bool TrustServerCertificate { set; }
            bool Pooling { set; }
            string ConnectionString { get; }
        }
        #endregion
        #endregion
        #region Sqlite
        private static DependencyMissingException SqliteDependencyException => new DependencyMissingException(
            "SqliteConnection dependency missing! Inject the dependency into the CSL.\n" +
            "This should look like:\n\n" +
            "--------------------------------------------------------------------------------\n" +
            "CSL.DependencyInjection.SqliteConnectionConstructor = (x) => new Microsoft.Data.Sqlite.SqliteConnection(x);\n" +
            "CSL.DependencyInjection.SqliteConnectionStringConstructor = () => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();\n" +
            "CSL.DependencyInjection.SqliteOpenModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteOpenMode)x;\n" +
            "CSL.DependencyInjection.SqliteCacheModeConverter = (x) => (Microsoft.Data.Sqlite.SqliteCacheMode)x;\n" +
            "--------------------------------------------------------------------------------\n\n" +
            "Put this line at the start of your program to use any methods that depend on Sqlite.");
        #region SqliteConnection
        public static Func<string, DbConnection>? SqliteConnectionConstructor { private get; set; } = null;
        internal static DbConnection CreateSqliteConnection(string ConnectionString) => SqliteConnectionConstructor == null ? throw SqliteDependencyException : SqliteConnectionConstructor(ConnectionString);
        #endregion
        #region SqliteConnectionStringBuilder

        internal static ISqliteConnectionStringBuilder CreateISqliteConnectionStringBuilder() => SqliteConnectionStringConstructor == null ? throw SqliteDependencyException : new SqliteConnectionStringWrapper(SqliteConnectionStringConstructor!());
        public static Func<SqliteOpenMode, dynamic>? SqliteOpenModeConverter { private get; set; } = null;
        public static Func<SqliteCacheMode, dynamic>? SqliteCacheModeConverter { private get; set; } = null;
        public static Func<dynamic>? SqliteConnectionStringConstructor { private get; set; } = null;
        internal class SqliteConnectionStringWrapper : ISqliteConnectionStringBuilder
        {
            private readonly dynamic SqliteConnectionStringInstance;
            public SqliteConnectionStringWrapper(dynamic SqliteConnectionStringInstance) => this.SqliteConnectionStringInstance = SqliteConnectionStringInstance;

            public string DataSource { set => SqliteConnectionStringInstance.DataSource = value; }
            public SqliteOpenMode Mode { set => SqliteConnectionStringInstance.Mode = SqliteOpenModeConverter == null ? throw SqliteDependencyException : SqliteOpenModeConverter(value); }
            public SqliteCacheMode Cache { set => SqliteConnectionStringInstance.Cache = SqliteCacheModeConverter == null ? throw SqliteDependencyException : SqliteCacheModeConverter(value); }
            public string ConnectionString => SqliteConnectionStringInstance.ConnectionString;
        }
        internal interface ISqliteConnectionStringBuilder
        {
            string DataSource { set; }
            SqliteOpenMode Mode { set; }
            SqliteCacheMode Cache { set; }
            string ConnectionString { get; }
        }
        #endregion
        #endregion
    }
    public class DependencyMissingException : Exception
    {
        public DependencyMissingException()
        {
        }

        public DependencyMissingException(string message) : base(message)
        {
        }

        public DependencyMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DependencyMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    #region Other
    public enum SslMode
    {
        //     SSL is disabled. If the server requires SSL, the connection will fail.
        Disable = 0,
        //     Prefer SSL connections if the server allows them, but allow connections without SSL.
        Prefer = 1,
        //     Fail the connection if the server doesn't support SSL.
        Require = 2
    }
    public enum SqliteOpenMode
    {
        //     Opens the database for reading and writing, and creates it if it doesn't exist.
        ReadWriteCreate = 0,
        //     Opens the database for reading and writing.
        ReadWrite = 1,
        //     Opens the database in read-only mode.
        ReadOnly = 2,
        //     Opens an in-memory database.
        Memory = 3
    }
    public enum SqliteCacheMode
    {
        //     Default mode.
        Default = 0,
        //     Private-cache mode. Each connection uses a private cache.
        Private = 1,
        //     Shared-cache mode. Connections share a cache. This mode can change the behavior of transaction and table locking.
        Shared = 2
    }
    #endregion
}
