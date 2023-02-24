using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public abstract class SQLFunc
    {
        protected SQLFunc(string SQLCode) => this.SQLCode = SQLCode;
        protected readonly string SQLCode = "";
    }
    public class SQLFunc<Input, Output> : SQLFunc where Input : CSLRecord<Input> where Output : CSLRecord<Output>
    {
        public SQLFunc(string SQLCode) : base(SQLCode) { }
        public async Task<AutoClosingEnumerable<Output>> Run(SQLDB sql, Input input) => (await sql.ExecuteReader(SQLCode, sql.ConvertToFriendlyParameters(input.ToArray()))).ReadRecords<Output>(sql);
    }
    public class SQLFunc<Output> : SQLFunc where Output : CSLRecord<Output>
    {
        public SQLFunc(string SQLCode) : base(SQLCode) { }
        public async Task<AutoClosingEnumerable<Output>> Run(SQLDB sql) => (await sql.ExecuteReader(SQLCode)).ReadRecords<Output>(sql);
    }
    public class SQLAction<Input> : SQLFunc where Input : CSLRecord<Input>
    {
        public SQLAction(string SQLCode) : base(SQLCode) { }
        public Task<int> Run(SQLDB sql, Input input) => sql.ExecuteNonQuery(SQLCode, sql.ConvertToFriendlyParameters(input.ToArray()));
    }
    public class SQLAction : SQLFunc
    {
        public SQLAction(string SQLCode) : base(SQLCode) { }
        public Task<int> Run(SQLDB sql) => sql.ExecuteNonQuery(SQLCode);
    }
}
