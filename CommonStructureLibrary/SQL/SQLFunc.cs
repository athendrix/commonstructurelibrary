using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public static class SQLFunc
    {
        public static Func<SQLDB, Input, Task<AutoClosingEnumerable<Output>>> GetFunc<Input, Output>(string SQLCode)
            where Input : CSLRecord<Input> where Output : CSLRecord<Output> =>
            async (SQLDB sql, Input input) => (await sql.ExecuteReader(SQLCode, sql.ConvertToFriendlyParameters(input.ToArray()))).ReadRecords<Output>(sql);

        public static Func<SQLDB, Task<AutoClosingEnumerable<Output>>> GetFunc<Output>(string SQLCode)
            where Output : CSLRecord<Output> =>
            async (SQLDB sql) => (await sql.ExecuteReader(SQLCode)).ReadRecords<Output>(sql);

        public static Func<SQLDB, Input, Task<int>> GetAction<Input>(string SQLCode)
            where Input : CSLRecord<Input> =>
            (SQLDB sql, Input input) => sql.ExecuteNonQuery(SQLCode, sql.ConvertToFriendlyParameters(input.ToArray()));
        public static Func<SQLDB, Task<int>> GetAction(string SQLCode) => (SQLDB sql) => sql.ExecuteNonQuery(SQLCode);
    }
}
