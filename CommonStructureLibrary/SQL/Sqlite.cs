using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using static CSL.DependencyInjection;

namespace CSL.SQL
{
    public class Sqlite : SQL
    {
        public Sqlite(string Filename, SqliteOpenMode Mode = SqliteOpenMode.ReadWriteCreate, SqliteCacheMode Cache = SqliteCacheMode.Default)
        {
            ISqliteConnectionStringBuilder csb = CreateISqliteConnectionStringBuilder();
            csb.DataSource = Filename;
            csb.Mode = Mode;
            csb.Cache = Cache;
            currentTransaction = null;
            InternalConnection = CreateSqliteConnection(csb.ConnectionString);
            InternalConnection.Open();
        }
    }
}
