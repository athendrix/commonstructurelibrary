using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public class MockDB : SQLDB
    {
        private List<string> LogData = new List<string>();
        public void Log(string s) => LogData.Add(s);
        public IEnumerable<string> ReadLog() => LogData.AsReadOnly().AsEnumerable();
        
        
        
        public class MockDbCommand : DbCommand
        {
            private MockDB mdb;
            public MockDbCommand(MockDB mdb)
            {
                this.mdb = mdb;
            }

            public override void Cancel() => mdb.Log("Command Cancelled");
            public override int ExecuteNonQuery() => throw new NotImplementedException();
            public override object ExecuteScalar() => throw new NotImplementedException();
            public override void Prepare() => throw new NotImplementedException();
            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection { get; }
            protected override DbTransaction DbTransaction { get; set; }
            public override bool DesignTimeVisible { get; set; }
            protected override DbParameter CreateDbParameter() => throw new NotImplementedException();
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => throw new NotImplementedException();
        }
        
        
        
        
        
        
        public class MockDbTransaction : DbTransaction
        {
            private MockDB mdb;
            private Guid TransactionID = Guid.NewGuid();
            public MockDbTransaction(MockDB mdb, IsolationLevel isolationLevel)
            {
                this.mdb = mdb;
                this.IsolationLevel = isolationLevel;
                mdb.Log($"Transaction {TransactionID} Created. IsolationLevel {isolationLevel}");
            }
            public override void Commit() => mdb.Log($"Transaction {TransactionID} Committed");
            public override void Rollback() => mdb.Log($"Transaction {TransactionID} Committed");
            protected override DbConnection DbConnection => mdb.dbConnection;
            public override IsolationLevel IsolationLevel { get; }
        }
        public class MockDbConnection : DbConnection
        {
            public MockDB mdb { get; set; }
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => new MockDbTransaction(mdb, isolationLevel);
            public override void ChangeDatabase(string databaseName) => mdb.Log($"Database Changed to {_Database = databaseName}");

            private bool Connected = false;
            public override void Close()
            {
                mdb.Log("DbConnection Closed");
                Connected = false;
            }
            public override void Open()
            {
                mdb.Log("DbConnection Opened");
                Connected = true;
            }
            public override ConnectionState State => Connected ? ConnectionState.Open : ConnectionState.Closed;

            private string _DataSource = "";
            public override string ConnectionString
            {
                get => _DataSource;
                set
                {
                    mdb.Log($"ConnectionString set to {value}");
                    _DataSource = value;
                }
            }
            private string _Database = "";
            public override string Database => _Database;

            public override string DataSource => _DataSource;
            public override string ServerVersion  => "0.0";
            protected override DbCommand CreateDbCommand() => throw new NotImplementedException();
        }
        public MockDbConnection dbConnection { get; }
        public MockDB() : this(new MockDbConnection()) { }
        private MockDB(MockDbConnection mdbc) : base(mdbc)
        {
            this.dbConnection = mdbc;
            mdbc.mdb          = this;
        }
        public override object? ConvertToFriendlyParameter(object? parameter) => parameter;
        public override object? ConvertFromFriendlyParameter(object? parameter, ParameterInfo pi) => parameter;
        public override string? GetSQLType(Type t) => t.ToString();
        public override async Task Truncate(string TableName, bool cascade = false) => Log($"{TableName} Truncated with cascade {(cascade ? "ON" : "OFF")}");
    }
}