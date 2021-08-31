﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using static CSL.DependencyInjection;
using System.Data.Common;

namespace CSL.SQL
{
    public class Sqlite : SQLDB
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
        public Sqlite(DbConnection connection)
        {
            InternalConnection = connection;
            InternalConnection.Open();
        }
    }
}
