using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.Sqlite;

namespace CSL.SQL
{
    public class Sqlite : SQL
    {
        public Sqlite(string Filename, SqliteOpenMode Mode = SqliteOpenMode.ReadWriteCreate, SqliteCacheMode Cache = SqliteCacheMode.Default)
        {
            SqliteConnectionStringBuilder csb = new SqliteConnectionStringBuilder { DataSource = Filename, Mode = Mode, Cache = Cache };
            currentTransaction = null;
            InternalConnection = new SqliteConnection(csb.ConnectionString);
            InternalConnection.Open();
        }
    }
}
