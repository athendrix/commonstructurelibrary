using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CSL.SQL;

namespace CommonStructureLibraryTester.Example
{
    public class ExampleFactory : IDBSetFactory<Guid>
    {
        public Task<int> CreateDB(SQLDB sql)
        {
            return sql.ExecuteNonQuery(
            "CREATE TABLE IF NOT EXISTS \"Example\" (" +
            "\"ID\" UUID NOT NULL, " +
            "\"Data1\" BOOLEAN NOT NULL, " +
            "\"Data2\" BOOLEAN, " +
            "\"Data3\" BYTEA NOT NULL, " +
            "\"Data4\" BYTEA, " +
            "\"Data5\" CHAR(1) NOT NULL, " +
            "\"Data6\" CHAR(1), " +
            "\"Data7\" SMALLINT NOT NULL, " +
            "\"Data8\" SMALLINT, " +
            "\"Data9\" INTEGER NOT NULL, " +
            "\"Data10\" INTEGER, " +
            "\"Data11\" BIGINT NOT NULL, " +
            "\"Data12\" BIGINT, " +
            "\"Data13\" BIGINT NOT NULL, " +
            "\"Data14\" BIGINT, " +
            "\"Data15\" FLOAT4 NOT NULL, " +
            "\"Data16\" FLOAT4, " +
            "\"Data17\" FLOAT8 NOT NULL, " +
            "\"Data18\" FLOAT8, " +
            "\"Data19\" NUMERIC NOT NULL, " +
            "\"Data20\" NUMERIC, " +
            "\"Data21\" TEXT NOT NULL, " +
            "\"Data22\" TEXT, " +
            "\"Data23\" BYTEA NOT NULL, " +
            "\"Data24\" BYTEA, " +
            "\"Data25\" UUID NOT NULL, " +
            "\"Data26\" UUID, " +
            "\"Data27\" TIMESTAMP NOT NULL, " +
            "\"Data28\" TIMESTAMP, " +
            "\"Data29\" BIGINT NOT NULL, " +
            "\"Data30\" BIGINT, " +
            "PRIMARY KEY(\"ID\")" +
            ");");
        }
        IEnumerable<IDBSet<Guid>> IDBSetFactory<Guid>.GetEnumerator(IDataReader dr) => GetEnumerator(dr);
        public IEnumerable<ExampleRecord> GetEnumerator(IDataReader dr)
        {
            while (dr.Read())
            {
                yield return ExampleRecord.FromDataReader(dr);
            }
            yield break;
        }
        #region Select
        IAsyncEnumerable<IDBSet<Guid>> IDBSetFactory<Guid>.Select(SQLDB sql) => Select(sql);
        public async IAsyncEnumerable<ExampleRecord> Select(SQLDB sql)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example\";"))
            {
                foreach (ExampleRecord item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid>> IDBSetFactory<Guid>.Select(SQLDB sql, string query, params object[] parameters) => Select(sql, query, parameters);
        public async IAsyncEnumerable<ExampleRecord> Select(SQLDB sql, string query, params object[] parameters)
        {
            using (IDataReader dr = await sql.ExecuteReader(query, parameters))
            {
                foreach (ExampleRecord item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        async Task<IDBSet<Guid>?> IDBSetFactory<Guid>.SelectByPK(SQLDB sql, Guid ID) => await SelectByPK(sql, ID);
        public async Task<ExampleRecord?> SelectByPK(SQLDB sql, Guid ID)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example\" WHERE \"ID\" = @0;", ID))
            {
                return GetEnumerator(dr).FirstOrDefault();
            }
        }
        #endregion
        #region Delete
        public Task<int> DeleteByPK(SQLDB sql, Guid ID)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example\" WHERE \"ID\" = @0;", ID);
        }
        #endregion
    }
    public record ExampleRecord(Guid ID, bool Data1, bool? Data2, byte Data3, byte? Data4, char Data5, char? Data6, short Data7, short? Data8, int Data9, int? Data10, long Data11, long? Data12, ulong Data13, ulong? Data14, float Data15, float? Data16, double Data17, double? Data18, decimal Data19, decimal? Data20, string Data21, string? Data22, byte[] Data23, byte[]? Data24, Guid Data25, Guid? Data26, DateTime Data27, DateTime? Data28, Data29 Data29, Data30? Data30) : IDBSet<Guid>
    {
        #region Primary Keys
        public Guid PK => ID;

        #endregion
        #region SQLConverters
        public static ExampleRecord FromDataReader(IDataReader dr)
        {
            Guid ID = (Guid)dr[0];
            bool Data1 = (bool)dr[1];
            bool? Data2 = dr.IsDBNull(2) ? null : (bool)dr[2];
            byte[] _Data3 = (byte[])dr[3];
            byte Data3 = _Data3[0];
            byte[]? _Data4 = dr.IsDBNull(4) ? null : (byte[])dr[4];
            byte? Data4 = _Data4 == null ? null : _Data4[0];
            string _Data5 = (string)dr[5];
            char Data5 = _Data5[0];
            string? _Data6 = dr.IsDBNull(6) ? null : (string)dr[6];
            char? Data6 = _Data6 == null ? null : _Data6[0];
            short Data7 = (short)dr[7];
            short? Data8 = dr.IsDBNull(8) ? null : (short)dr[8];
            int Data9 = (int)dr[9];
            int? Data10 = dr.IsDBNull(10) ? null : (int)dr[10];
            long Data11 = (long)dr[11];
            long? Data12 = dr.IsDBNull(12) ? null : (long)dr[12];
            long _Data13 = (long)dr[13];
            ulong Data13 = (ulong)_Data13;
            long? _Data14 = dr.IsDBNull(14) ? null : (long)dr[14];
            ulong? Data14 = _Data14 == null ? null : (ulong?)_Data14;
            float Data15 = (float)dr[15];
            float? Data16 = dr.IsDBNull(16) ? null : (float)dr[16];
            double Data17 = (double)dr[17];
            double? Data18 = dr.IsDBNull(18) ? null : (double)dr[18];
            decimal Data19 = (decimal)dr[19];
            decimal? Data20 = dr.IsDBNull(20) ? null : (decimal)dr[20];
            string Data21 = (string)dr[21];
            string? Data22 = dr.IsDBNull(22) ? null : (string)dr[22];
            byte[] Data23 = (byte[])dr[23];
            byte[]? Data24 = dr.IsDBNull(24) ? null : (byte[])dr[24];
            Guid Data25 = (Guid)dr[25];
            Guid? Data26 = dr.IsDBNull(26) ? null : (Guid)dr[26];
            DateTime Data27 = (DateTime)dr[27];
            DateTime? Data28 = dr.IsDBNull(28) ? null : (DateTime)dr[28];
            long _Data29 = (long)dr[29];
            Data29 Data29 = (Data29)_Data29;
            long? _Data30 = dr.IsDBNull(30) ? null : (long)dr[30];
            Data30? Data30 = _Data30 == null ? null : (Data30?)_Data30;
            return new ExampleRecord(ID, Data1, Data2, Data3, Data4, Data5, Data6, Data7, Data8, Data9, Data10, Data11, Data12, Data13, Data14, Data15, Data16, Data17, Data18, Data19, Data20, Data21, Data22, Data23, Data24, Data25, Data26, Data27, Data28, Data29, Data30);
        }
        public object[] ToArray()
        {
            Guid _ID = ID;
            bool _Data1 = Data1;
            bool? _Data2 = Data2 == null ? default : Data2;
            byte[] _Data3 = new byte[] { Data3 };
            byte[]? _Data4 = Data4 == null ? default : new byte[] { Data4.Value };
            string _Data5 = Data5.ToString();
            string? _Data6 = Data6 == null ? default : Data6?.ToString();
            short _Data7 = Data7;
            short? _Data8 = Data8 == null ? default : Data8;
            int _Data9 = Data9;
            int? _Data10 = Data10 == null ? default : Data10;
            long _Data11 = Data11;
            long? _Data12 = Data12 == null ? default : Data12;
            long _Data13 = (long)Data13;
            long? _Data14 = Data14 == null ? default : (long?)Data14;
            float _Data15 = Data15;
            float? _Data16 = Data16 == null ? default : Data16;
            double _Data17 = Data17;
            double? _Data18 = Data18 == null ? default : Data18;
            decimal _Data19 = Data19;
            decimal? _Data20 = Data20 == null ? default : Data20;
            string _Data21 = Data21;
            string? _Data22 = Data22 == null ? default : Data22;
            byte[] _Data23 = Data23;
            byte[]? _Data24 = Data24 == null ? default : Data24;
            Guid _Data25 = Data25;
            Guid? _Data26 = Data26 == null ? default : Data26;
            DateTime _Data27 = Data27;
            DateTime? _Data28 = Data28 == null ? default : Data28;
            long _Data29 = (long)Data29;
            long? _Data30 = Data30 == null ? default : (long?)Data30;
            return new object[] { _ID, _Data1, _Data2, _Data3, _Data4, _Data5, _Data6, _Data7, _Data8, _Data9, _Data10, _Data11, _Data12, _Data13, _Data14, _Data15, _Data16, _Data17, _Data18, _Data19, _Data20, _Data21, _Data22, _Data23, _Data24, _Data25, _Data26, _Data27, _Data28, _Data29, _Data30 };
        }
        #endregion
        #region IDBSetFunctions
        public Task<int> Insert(SQLDB sql)
        {
            return sql.ExecuteNonQuery("INSERT INTO \"Example\" (\"ID\", \"Data1\", \"Data2\", \"Data3\", \"Data4\", \"Data5\", \"Data6\", \"Data7\", \"Data8\", \"Data9\", \"Data10\", \"Data11\", \"Data12\", \"Data13\", \"Data14\", \"Data15\", \"Data16\", \"Data17\", \"Data18\", \"Data19\", \"Data20\", \"Data21\", \"Data22\", \"Data23\", \"Data24\", \"Data25\", \"Data26\", \"Data27\", \"Data28\", \"Data29\", \"Data30\") " +
            "VALUES(@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30);", ToArray());
        }
        public Task<int> Update(SQLDB sql)
        {
            return sql.ExecuteNonQuery("UPDATE \"Example\" " +
            "SET \"Data1\" = @1, \"Data2\" = @2, \"Data3\" = @3, \"Data4\" = @4, \"Data5\" = @5, \"Data6\" = @6, \"Data7\" = @7, \"Data8\" = @8, \"Data9\" = @9, \"Data10\" = @10, \"Data11\" = @11, \"Data12\" = @12, \"Data13\" = @13, \"Data14\" = @14, \"Data15\" = @15, \"Data16\" = @16, \"Data17\" = @17, \"Data18\" = @18, \"Data19\" = @19, \"Data20\" = @20, \"Data21\" = @21, \"Data22\" = @22, \"Data23\" = @23, \"Data24\" = @24, \"Data25\" = @25, \"Data26\" = @26, \"Data27\" = @27, \"Data28\" = @28, \"Data29\" = @29, \"Data30\" = @30 " +
            "WHERE \"ID\" = @0;", ToArray());
        }
        public Task<int> Upsert(SQLDB sql)
        {
            return sql.ExecuteNonQuery("INSERT INTO \"Example\" (\"ID\", \"Data1\", \"Data2\", \"Data3\", \"Data4\", \"Data5\", \"Data6\", \"Data7\", \"Data8\", \"Data9\", \"Data10\", \"Data11\", \"Data12\", \"Data13\", \"Data14\", \"Data15\", \"Data16\", \"Data17\", \"Data18\", \"Data19\", \"Data20\", \"Data21\", \"Data22\", \"Data23\", \"Data24\", \"Data25\", \"Data26\", \"Data27\", \"Data28\", \"Data29\", \"Data30\") " +
            "VALUES(@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30) " +
            "ON CONFLICT (\"ID\") DO UPDATE " +
            "SET \"Data1\" = @1, \"Data2\" = @2, \"Data3\" = @3, \"Data4\" = @4, \"Data5\" = @5, \"Data6\" = @6, \"Data7\" = @7, \"Data8\" = @8, \"Data9\" = @9, \"Data10\" = @10, \"Data11\" = @11, \"Data12\" = @12, \"Data13\" = @13, \"Data14\" = @14, \"Data15\" = @15, \"Data16\" = @16, \"Data17\" = @17, \"Data18\" = @18, \"Data19\" = @19, \"Data20\" = @20, \"Data21\" = @21, \"Data22\" = @22, \"Data23\" = @23, \"Data24\" = @24, \"Data25\" = @25, \"Data26\" = @26, \"Data27\" = @27, \"Data28\" = @28, \"Data29\" = @29, \"Data30\" = @30;", ToArray());
        }
        #endregion
    }

    //Example Enum
    [Flags]
    //Specifying ulong allows data to be auto converted for your convenience into the database.
    public enum Data29 : ulong
    {
        NoFlags = 0,
        Flag1 = 1UL << 0,
        Flag2 = 1UL << 1,
        Flag3 = 1UL << 2,
        Flag4 = 1UL << 3,
        Flag5 = 1UL << 4,
        Flag6 = 1UL << 5,
        Flag7 = 1UL << 6,
        Flag8 = 1UL << 7,
        Flag9 = 1UL << 8,
        Flag10 = 1UL << 9,
        Flag11 = 1UL << 10,
        Flag12 = 1UL << 11,
        Flag13 = 1UL << 12,
        Flag14 = 1UL << 13,
        Flag15 = 1UL << 14,
        Flag16 = 1UL << 15,
    }

    //Example Enum
    [Flags]
    //Specifying ulong allows data to be auto converted for your convenience into the database.
    public enum Data30 : ulong
    {
        NoFlags = 0,
        Flag1 = 1UL << 0,
        Flag2 = 1UL << 1,
        Flag3 = 1UL << 2,
        Flag4 = 1UL << 3,
        Flag5 = 1UL << 4,
        Flag6 = 1UL << 5,
        Flag7 = 1UL << 6,
        Flag8 = 1UL << 7,
        Flag9 = 1UL << 8,
        Flag10 = 1UL << 9,
        Flag11 = 1UL << 10,
        Flag12 = 1UL << 11,
        Flag13 = 1UL << 12,
        Flag14 = 1UL << 13,
        Flag15 = 1UL << 14,
        Flag16 = 1UL << 15,
    }
}