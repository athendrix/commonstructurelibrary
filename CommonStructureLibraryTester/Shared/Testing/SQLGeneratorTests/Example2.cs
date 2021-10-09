using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public class Example2Factory : IDBSetFactory<Guid, Guid, Guid, Guid>
    {
        public Task<int> CreateDB(SQLDB sql)
        {
            return sql.ExecuteNonQuery(
            "CREATE TABLE IF NOT EXISTS \"Example2\" (" +
            "\"ID\" UUID NOT NULL, " +
            "\"ID2\" UUID NOT NULL, " +
            "\"ID3\" UUID NOT NULL, " +
            "\"ID4\" UUID NOT NULL, " +
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
            "\"Data30\" BIGINT " +
            ");");
        }
        IEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.GetEnumerator(IDataReader dr) => GetEnumerator(dr);
        public IEnumerable<Example2Record> GetEnumerator(IDataReader dr)
        {
            while (dr.Read())
            {
                yield return Example2Record.FromDataReader(dr);
            }
            yield break;
        }
        #region Select
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.Select(SQLDB sql) => Select(sql);
        public async IAsyncEnumerable<Example2Record> Select(SQLDB sql)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\";"))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.Select(SQLDB sql, string query, params object[] parameters) => Select(sql, query, parameters);
        public async IAsyncEnumerable<Example2Record> Select(SQLDB sql, string query, params object[] parameters)
        {
            using (IDataReader dr = await sql.ExecuteReader(query, parameters))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK1(SQLDB sql, Guid ID) => SelectByPK1(sql, ID);
        public async IAsyncEnumerable<Example2Record> SelectByPK1(SQLDB sql, Guid ID)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0;", ID))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK2(SQLDB sql, Guid ID2) => SelectByPK2(sql, ID2);
        public async IAsyncEnumerable<Example2Record> SelectByPK2(SQLDB sql, Guid ID2)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID2\" = @0;", ID2))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK3(SQLDB sql, Guid ID3) => SelectByPK3(sql, ID3);
        public async IAsyncEnumerable<Example2Record> SelectByPK3(SQLDB sql, Guid ID3)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID3\" = @0;", ID3))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK4(SQLDB sql, Guid ID4) => SelectByPK4(sql, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK4(SQLDB sql, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID4\" = @0;", ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK12(SQLDB sql, Guid ID, Guid ID2) => SelectByPK12(sql, ID, ID2);
        public async IAsyncEnumerable<Example2Record> SelectByPK12(SQLDB sql, Guid ID, Guid ID2)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1;", ID, ID2))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK13(SQLDB sql, Guid ID, Guid ID3) => SelectByPK13(sql, ID, ID3);
        public async IAsyncEnumerable<Example2Record> SelectByPK13(SQLDB sql, Guid ID, Guid ID3)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID3\" = @1;", ID, ID3))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK14(SQLDB sql, Guid ID, Guid ID4) => SelectByPK14(sql, ID, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK14(SQLDB sql, Guid ID, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID4\" = @1;", ID, ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK23(SQLDB sql, Guid ID2, Guid ID3) => SelectByPK23(sql, ID2, ID3);
        public async IAsyncEnumerable<Example2Record> SelectByPK23(SQLDB sql, Guid ID2, Guid ID3)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID2\" = @0 AND \"ID3\" = @1;", ID2, ID3))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK24(SQLDB sql, Guid ID2, Guid ID4) => SelectByPK24(sql, ID2, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK24(SQLDB sql, Guid ID2, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID2\" = @0 AND \"ID4\" = @1;", ID2, ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK34(SQLDB sql, Guid ID3, Guid ID4) => SelectByPK34(sql, ID3, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK34(SQLDB sql, Guid ID3, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID3\" = @0 AND \"ID4\" = @1;", ID3, ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK123(SQLDB sql, Guid ID, Guid ID2, Guid ID3) => SelectByPK123(sql, ID, ID2, ID3);
        public async IAsyncEnumerable<Example2Record> SelectByPK123(SQLDB sql, Guid ID, Guid ID2, Guid ID3)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID3\" = @2;", ID, ID2, ID3))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK124(SQLDB sql, Guid ID, Guid ID2, Guid ID4) => SelectByPK124(sql, ID, ID2, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK124(SQLDB sql, Guid ID, Guid ID2, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID4\" = @2;", ID, ID2, ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK134(SQLDB sql, Guid ID, Guid ID3, Guid ID4) => SelectByPK134(sql, ID, ID3, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK134(SQLDB sql, Guid ID, Guid ID3, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID3\" = @1 AND \"ID4\" = @2;", ID, ID3, ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        IAsyncEnumerable<IDBSet<Guid, Guid, Guid, Guid>> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK234(SQLDB sql, Guid ID2, Guid ID3, Guid ID4) => SelectByPK234(sql, ID2, ID3, ID4);
        public async IAsyncEnumerable<Example2Record> SelectByPK234(SQLDB sql, Guid ID2, Guid ID3, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID2\" = @0 AND \"ID3\" = @1 AND \"ID4\" = @2;", ID2, ID3, ID4))
            {
                foreach (Example2Record item in GetEnumerator(dr))
                {
                    yield return item;
                }
            }
        }
        async Task<IDBSet<Guid, Guid, Guid, Guid>?> IDBSetFactory<Guid, Guid, Guid, Guid>.SelectByPK(SQLDB sql, Guid ID, Guid ID2, Guid ID3, Guid ID4) => await SelectByPK(sql, ID, ID2, ID3, ID4);
        public async Task<Example2Record?> SelectByPK(SQLDB sql, Guid ID, Guid ID2, Guid ID3, Guid ID4)
        {
            using (IDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID3\" = @2 AND \"ID4\" = @3;", ID, ID2, ID3, ID4))
            {
                return GetEnumerator(dr).FirstOrDefault();
            }
        }
        #endregion
        #region Delete
        public Task<int> DeleteByPK1(SQLDB sql, Guid ID)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0;", ID);
        }
        public Task<int> DeleteByPK2(SQLDB sql, Guid ID2)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID2\" = @0;", ID2);
        }
        public Task<int> DeleteByPK3(SQLDB sql, Guid ID3)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID3\" = @0;", ID3);
        }
        public Task<int> DeleteByPK4(SQLDB sql, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID4\" = @0;", ID4);
        }
        public Task<int> DeleteByPK12(SQLDB sql, Guid ID, Guid ID2)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1;", ID, ID2);
        }
        public Task<int> DeleteByPK13(SQLDB sql, Guid ID, Guid ID3)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID3\" = @1;", ID, ID3);
        }
        public Task<int> DeleteByPK14(SQLDB sql, Guid ID, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID4\" = @1;", ID, ID4);
        }
        public Task<int> DeleteByPK23(SQLDB sql, Guid ID2, Guid ID3)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID2\" = @0 AND \"ID3\" = @1;", ID2, ID3);
        }
        public Task<int> DeleteByPK24(SQLDB sql, Guid ID2, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID2\" = @0 AND \"ID4\" = @1;", ID2, ID4);
        }
        public Task<int> DeleteByPK34(SQLDB sql, Guid ID3, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID3\" = @0 AND \"ID4\" = @1;", ID3, ID4);
        }
        public Task<int> DeleteByPK123(SQLDB sql, Guid ID, Guid ID2, Guid ID3)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID3\" = @2;", ID, ID2, ID3);
        }
        public Task<int> DeleteByPK124(SQLDB sql, Guid ID, Guid ID2, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID4\" = @2;", ID, ID2, ID4);
        }
        public Task<int> DeleteByPK134(SQLDB sql, Guid ID, Guid ID3, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID3\" = @1 AND \"ID4\" = @2;", ID, ID3, ID4);
        }
        public Task<int> DeleteByPK234(SQLDB sql, Guid ID2, Guid ID3, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID2\" = @0 AND \"ID3\" = @1 AND \"ID4\" = @2;", ID2, ID3, ID4);
        }
        public Task<int> DeleteByPK(SQLDB sql, Guid ID, Guid ID2, Guid ID3, Guid ID4)
        {
            return sql.ExecuteNonQuery("DELETE FROM \"Example2\" WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID3\" = @2 AND \"ID4\" = @3;", ID, ID2, ID3, ID4);
        }
        #endregion
    }
    public record Example2Record(Guid ID, Guid ID2, Guid ID3, Guid ID4, bool Data1, bool? Data2, byte Data3, byte? Data4, char Data5, char? Data6, short Data7, short? Data8, int Data9, int? Data10, long Data11, long? Data12, ulong Data13, ulong? Data14, float Data15, float? Data16, double Data17, double? Data18, decimal Data19, decimal? Data20, string Data21, string? Data22, byte[] Data23, byte[]? Data24, Guid Data25, Guid? Data26, DateTime Data27, DateTime? Data28, Data29 Data29, Data30? Data30) : IDBSet<Guid, Guid, Guid, Guid>
    {
        #region Primary Keys
        public Guid PK1 => ID;

        public Guid PK2 => ID2;

        public Guid PK3 => ID3;

        public Guid PK4 => ID4;

        #endregion
        #region SQLConverters
        public static Example2Record FromDataReader(IDataReader dr)
        {
            Guid ID = (Guid)dr[0];
            Guid ID2 = (Guid)dr[1];
            Guid ID3 = (Guid)dr[2];
            Guid ID4 = (Guid)dr[3];
            bool Data1 = (bool)dr[4];
            bool? Data2 = dr.IsDBNull(5) ? null : (bool)dr[5];
            byte[] _Data3 = (byte[])dr[6];
            byte Data3 = _Data3[0];
            byte[]? _Data4 = dr.IsDBNull(7) ? null : (byte[])dr[7];
            byte? Data4 = _Data4 == null ? null : _Data4[0];
            string _Data5 = (string)dr[8];
            char Data5 = _Data5[0];
            string? _Data6 = dr.IsDBNull(9) ? null : (string)dr[9];
            char? Data6 = _Data6 == null ? null : _Data6[0];
            short Data7 = (short)dr[10];
            short? Data8 = dr.IsDBNull(11) ? null : (short)dr[11];
            int Data9 = (int)dr[12];
            int? Data10 = dr.IsDBNull(13) ? null : (int)dr[13];
            long Data11 = (long)dr[14];
            long? Data12 = dr.IsDBNull(15) ? null : (long)dr[15];
            long _Data13 = (long)dr[16];
            ulong Data13 = (ulong)_Data13;
            long? _Data14 = dr.IsDBNull(17) ? null : (long)dr[17];
            ulong? Data14 = _Data14 == null ? null : (ulong?)_Data14;
            float Data15 = (float)dr[18];
            float? Data16 = dr.IsDBNull(19) ? null : (float)dr[19];
            double Data17 = (double)dr[20];
            double? Data18 = dr.IsDBNull(21) ? null : (double)dr[21];
            decimal Data19 = (decimal)dr[22];
            decimal? Data20 = dr.IsDBNull(23) ? null : (decimal)dr[23];
            string Data21 = (string)dr[24];
            string? Data22 = dr.IsDBNull(25) ? null : (string)dr[25];
            byte[] Data23 = (byte[])dr[26];
            byte[]? Data24 = dr.IsDBNull(27) ? null : (byte[])dr[27];
            Guid Data25 = (Guid)dr[28];
            Guid? Data26 = dr.IsDBNull(29) ? null : (Guid)dr[29];
            DateTime Data27 = (DateTime)dr[30];
            DateTime? Data28 = dr.IsDBNull(31) ? null : (DateTime)dr[31];
            long _Data29 = (long)dr[32];
            Data29 Data29 = (Data29)_Data29;
            long? _Data30 = dr.IsDBNull(33) ? null : (long)dr[33];
            Data30? Data30 = _Data30 == null ? null : (Data30?)_Data30;
            return new Example2Record(ID, ID2, ID3, ID4, Data1, Data2, Data3, Data4, Data5, Data6, Data7, Data8, Data9, Data10, Data11, Data12, Data13, Data14, Data15, Data16, Data17, Data18, Data19, Data20, Data21, Data22, Data23, Data24, Data25, Data26, Data27, Data28, Data29, Data30);
        }
        public object[] ToArray()
        {
            Guid _ID = ID;
            Guid _ID2 = ID2;
            Guid _ID3 = ID3;
            Guid _ID4 = ID4;
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
            return new object[] { _ID, _ID2, _ID3, _ID4, _Data1, _Data2, _Data3, _Data4, _Data5, _Data6, _Data7, _Data8, _Data9, _Data10, _Data11, _Data12, _Data13, _Data14, _Data15, _Data16, _Data17, _Data18, _Data19, _Data20, _Data21, _Data22, _Data23, _Data24, _Data25, _Data26, _Data27, _Data28, _Data29, _Data30 };
        }
        #endregion
        #region IDBSetFunctions
        public Task<int> Insert(SQLDB sql)
        {
            return sql.ExecuteNonQuery("INSERT INTO \"Example2\" (\"ID\", \"ID2\", \"ID3\", \"ID4\", \"Data1\", \"Data2\", \"Data3\", \"Data4\", \"Data5\", \"Data6\", \"Data7\", \"Data8\", \"Data9\", \"Data10\", \"Data11\", \"Data12\", \"Data13\", \"Data14\", \"Data15\", \"Data16\", \"Data17\", \"Data18\", \"Data19\", \"Data20\", \"Data21\", \"Data22\", \"Data23\", \"Data24\", \"Data25\", \"Data26\", \"Data27\", \"Data28\", \"Data29\", \"Data30\") " +
            "VALUES(@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30, @31, @32, @33);", ToArray());
        }
        public Task<int> Update(SQLDB sql)
        {
            return sql.ExecuteNonQuery("UPDATE \"Example2\" " +
            "SET \"Data1\" = @4, \"Data2\" = @5, \"Data3\" = @6, \"Data4\" = @7, \"Data5\" = @8, \"Data6\" = @9, \"Data7\" = @10, \"Data8\" = @11, \"Data9\" = @12, \"Data10\" = @13, \"Data11\" = @14, \"Data12\" = @15, \"Data13\" = @16, \"Data14\" = @17, \"Data15\" = @18, \"Data16\" = @19, \"Data17\" = @20, \"Data18\" = @21, \"Data19\" = @22, \"Data20\" = @23, \"Data21\" = @24, \"Data22\" = @25, \"Data23\" = @26, \"Data24\" = @27, \"Data25\" = @28, \"Data26\" = @29, \"Data27\" = @30, \"Data28\" = @31, \"Data29\" = @32, \"Data30\" = @33 " +
            "WHERE \"ID\" = @0 AND \"ID2\" = @1 AND \"ID3\" = @2 AND \"ID4\" = @3;", ToArray());
        }
        public Task<int> Upsert(SQLDB sql)
        {
            return sql.ExecuteNonQuery("INSERT INTO \"Example2\" (\"ID\", \"ID2\", \"ID3\", \"ID4\", \"Data1\", \"Data2\", \"Data3\", \"Data4\", \"Data5\", \"Data6\", \"Data7\", \"Data8\", \"Data9\", \"Data10\", \"Data11\", \"Data12\", \"Data13\", \"Data14\", \"Data15\", \"Data16\", \"Data17\", \"Data18\", \"Data19\", \"Data20\", \"Data21\", \"Data22\", \"Data23\", \"Data24\", \"Data25\", \"Data26\", \"Data27\", \"Data28\", \"Data29\", \"Data30\") " +
            "VALUES(@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30, @31, @32, @33) " +
            "ON CONFLICT (\"ID\", \"ID2\", \"ID3\", \"ID4\") DO UPDATE " +
            "SET \"Data1\" = @4, \"Data2\" = @5, \"Data3\" = @6, \"Data4\" = @7, \"Data5\" = @8, \"Data6\" = @9, \"Data7\" = @10, \"Data8\" = @11, \"Data9\" = @12, \"Data10\" = @13, \"Data11\" = @14, \"Data12\" = @15, \"Data13\" = @16, \"Data14\" = @17, \"Data15\" = @18, \"Data16\" = @19, \"Data17\" = @20, \"Data18\" = @21, \"Data19\" = @22, \"Data20\" = @23, \"Data21\" = @24, \"Data22\" = @25, \"Data23\" = @26, \"Data24\" = @27, \"Data25\" = @28, \"Data26\" = @29, \"Data27\" = @30, \"Data28\" = @31, \"Data29\" = @32, \"Data30\" = @33;", ToArray());
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