using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public record Example3(Guid ID, string Data1, string? Data2, int Data25) : IDBSet
    {
        #region Static Functions
        public static Task<int> CreateDB(SQLDB sql) => sql.ExecuteNonQuery(
            "CREATE TABLE IF NOT EXISTS \"Example3\" (" +
            "\"ID\" UUID NOT NULL, " +
            "\"Data1\" TEXT NOT NULL, " +
            "\"Data2\" TEXT, " +
            "\"Data25\" INTEGER NOT NULL, " +
            "PRIMARY KEY(\"ID\"), " +
            "UNIQUE(\"Data25\"), " +
            "UNIQUE(\"Data1\", \"Data2\") " +
            ");");
        public static IEnumerable<Example3> GetRecords(IDataReader dr)
        {
            while(dr.Read())
            {
                Guid ID =  (Guid)dr[0];
                string Data1 =  (string)dr[1];
                string? Data2 = dr.IsDBNull(2) ? null : (string)dr[2];
                int Data25 =  (int)dr[3];
                yield return new Example3(ID, Data1, Data2, Data25);
            }
            yield break;
        }
        #region Select
        public static async Task<AutoClosingEnumerable<Example3>> Select(SQLDB sql)
        {
            AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\";");
            return new AutoClosingEnumerable<Example3>(GetRecords(dr),dr);
        }
        public static async Task<AutoClosingEnumerable<Example3>> Select(SQLDB sql, string query, params object[] parameters)
        {
            AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\" WHERE " + query + " ;", parameters);
            return new AutoClosingEnumerable<Example3>(GetRecords(dr),dr);
        }
        public static async Task<Example3?> SelectBy_ID(SQLDB sql, Guid ID)
        {
            using(AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\" WHERE \"ID\" = @0;", ID))
            {
                return GetRecords(dr).FirstOrDefault();
            }
        }
        public static async Task<Example3?> SelectBy_Data25(SQLDB sql, int Data25)
        {
            using(AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\" WHERE \"Data25\" = @0;", Data25))
            {
                return GetRecords(dr).FirstOrDefault();
            }
        }
        public static async Task<Example3?> SelectBy_Data1_Data2(SQLDB sql, string Data1, string? Data2)
        {
            using(AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\" WHERE \"Data1\" = @0 AND \"Data2\" = @1;", Data1, Data2 == null ? default : Data2))
            {
                return GetRecords(dr).FirstOrDefault();
            }
        }
        public static async Task<AutoClosingEnumerable<Example3>> SelectBy_Data1(SQLDB sql, string Data1)
        {
            AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\" WHERE \"Data1\" = @0;", Data1);
            {
                return new AutoClosingEnumerable<Example3>(GetRecords(dr),dr);
            }
        }
        public static async Task<AutoClosingEnumerable<Example3>> SelectBy_Data2(SQLDB sql, string? Data2)
        {
            AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example3\" WHERE \"Data2\" = @0;", Data2 == null ? default : Data2);
            {
                return new AutoClosingEnumerable<Example3>(GetRecords(dr),dr);
            }
        }
        #endregion
        #region Delete
        public static Task<int> DeleteBy_ID(SQLDB sql, Guid ID) => sql.ExecuteNonQuery("DELETE FROM \"Example3\" WHERE \"ID\" = @0;", ID);
        public static Task<int> DeleteBy_Data25(SQLDB sql, int Data25) => sql.ExecuteNonQuery("DELETE FROM \"Example3\" WHERE \"Data25\" = @0;", Data25);
        public static Task<int> DeleteBy_Data1_Data2(SQLDB sql, string Data1, string? Data2) => sql.ExecuteNonQuery("DELETE FROM \"Example3\" WHERE \"Data1\" = @0 AND \"Data2\" = @1;", Data1, Data2 == null ? default : Data2);
        public static Task<int> DeleteBy_Data1(SQLDB sql, string Data1) => sql.ExecuteNonQuery("DELETE FROM \"Example3\" WHERE \"Data1\" = @0;", Data1);
        public static Task<int> DeleteBy_Data2(SQLDB sql, string? Data2) => sql.ExecuteNonQuery("DELETE FROM \"Example3\" WHERE \"Data2\" = @0;", Data2 == null ? default : Data2);
        #endregion
        #region Table Management
        public static Task Truncate(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"TRUNCATE \"Example3\"{(cascade?" CASCADE":"")};");
        public static Task Drop(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"DROP TABLE IF EXISTS \"Example3\"{(cascade?" CASCADE":"")};");
        #endregion
        #endregion
        #region Instance Functions
        public Task<int> Insert(SQLDB sql) =>
            sql.ExecuteNonQuery("INSERT INTO \"Example3\" (\"ID\", \"Data1\", \"Data2\", \"Data25\") " +
            "VALUES(@0, @1, @2, @3);", ToArray());
        public Task<int> Update(SQLDB sql) =>
            sql.ExecuteNonQuery("UPDATE \"Example3\" " +
            "SET \"Data1\" = @1, \"Data2\" = @2, \"Data25\" = @3 " +
            "WHERE \"ID\" = @0;", ToArray());
        public Task<int> Upsert(SQLDB sql) =>
            sql.ExecuteNonQuery("INSERT INTO \"Example3\" (\"ID\", \"Data1\", \"Data2\", \"Data25\") " +
            "VALUES(@0, @1, @2, @3) " +
            "ON CONFLICT (\"ID\") DO UPDATE " +
            "SET \"Data1\" = @1, \"Data2\" = @2, \"Data25\" = @3;", ToArray());
        public object?[] ToArray()
        {
            Guid _ID = ID;
            string _Data1 = Data1;
            string? _Data2 = Data2 == null ? default : Data2;
            int _Data25 = Data25;
            return new object?[] { _ID, _Data1, _Data2, _Data25 };
        }
        #endregion
    }
}