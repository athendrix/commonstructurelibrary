using static CSL.DependencyInjection;
using System.Data.Common;

namespace CSL.SQL
{
    public class Sqlite : SQLDB
    {
        private static DbConnection CreateConnection(string Filename, SqliteOpenMode Mode, SqliteCacheMode Cache)
        {
            ISqliteConnectionStringBuilder csb = CreateISqliteConnectionStringBuilder();
            csb.DataSource = Filename;
            csb.Mode = Mode;
            csb.Cache = Cache;
            return CreateSqliteConnection(csb.ConnectionString);
        }
        public Sqlite(string Filename, SqliteOpenMode Mode = SqliteOpenMode.ReadWriteCreate, SqliteCacheMode Cache = SqliteCacheMode.Default) : base(CreateConnection(Filename,Mode,Cache))
        {
            currentTransaction = null; ;
            InternalConnection.Open();
        }
        public Sqlite(DbConnection connection) : base(connection)
        {
            currentTransaction = null;
            InternalConnection.Open();
        }
    }
}
