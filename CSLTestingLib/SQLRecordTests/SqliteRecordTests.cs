using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;
using CSL.Encryption;
using CSL.SQL;
using CSL.Testing;
using ExampleNamespace.SomeSubNamespace;
using static CSLTesting.TestingHelpers;
using System.Reflection;
using CSL;

namespace CSLTesting
{
    public class SqliteRecordTests : Tests
    {
        record GuidBuildTestRecord(string test, Guid? test2) : SQLRecord<GuidBuildTestRecord>;
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> NullableGuidBuildTest()
        {
            GuidBuildTestRecord[] gbtr = new GuidBuildTestRecord[]
            {
                new GuidBuildTestRecord("foo", Guid.Empty),
                new GuidBuildTestRecord("bar", null),
                new GuidBuildTestRecord("qux", Guid.Empty),
                new GuidBuildTestRecord("baz", null),
                new GuidBuildTestRecord("foo", Guid.Empty),
            };
            using (Sqlite sql = new Sqlite(":memory:"))
            {
                await GuidBuildTestRecord.CreateDB(sql);
                foreach (GuidBuildTestRecord rec in gbtr)
                {
                    await rec.Insert(sql);
                }
                await GuidBuildTestRecord.Select(sql, Conditional.WHERE("test2", IS.EQUAL_TO, Guid.Empty));
            }
            return PASS();
        }

        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> Example1BasicFunctionalityTest()
        {
            #region DataInit
            List<Func<Sqlite>> SQLDBs = SQLTests.GetSqliteDB;
            Data29 D29AllFlags = Data29.Flag1 | Data29.Flag2 | Data29.Flag3 | Data29.Flag4 | Data29.Flag5 |
                Data29.Flag6 | Data29.Flag7 | Data29.Flag8 | Data29.Flag9 | Data29.Flag10 | Data29.Flag11 |
                Data29.Flag12 | Data29.Flag13 | Data29.Flag14 | Data29.Flag15 | Data29.Flag16;
            Data30 D30AllFlags = Data30.Flag1 | Data30.Flag2 | Data30.Flag3 | Data30.Flag4 | Data30.Flag5 |
                Data30.Flag6 | Data30.Flag7 | Data30.Flag8 | Data30.Flag9 | Data30.Flag10 | Data30.Flag11 |
                Data30.Flag12 | Data30.Flag13 | Data30.Flag14 | Data30.Flag15 | Data30.Flag16;
            Guid Key0 = MinGuid;
            Guid Key1 = MaxGuid;
            Guid Key2 = RandomGuid;
            Guid Key3 = RandomGuid;
            Guid Key4 = RandomGuid;
            Guid Key5 = RandomGuid;
            Guid Key6 = RandomGuid;
            Example[] exampleRecords = new Example[7];
            //Min/Max vals
            exampleRecords[0] = new Example(Key0, MinBool, MaxBool, MinByte, MaxByte, MinChar, MaxChar, MinShort, MaxShort, MinInt, MaxInt, MinLong, MaxLong,
                MinULong, MaxULong, MinFloat, MaxFloat, MinDouble, MaxDouble, MinDecimal, MaxDecimal, MinString, MaxString, MinByteArray(0), MaxByteArray(255),
                MinGuid, MaxGuid, MinDateTime, MaxDateTime, Data29.NoFlags, D30AllFlags);
            //Max/Min vals
            exampleRecords[1] = new Example(Key1, MaxBool, MinBool, MaxByte, MinByte, MaxChar, MinChar, MaxShort, MinShort, MaxInt, MinInt, MaxLong, MinLong,
                MaxULong, MinULong, MaxFloat, MinFloat, MaxDouble, MinDouble, MaxDecimal, MinDecimal, MaxString, MinString, MaxByteArray(255), MinByteArray(0),
                MaxGuid, MinGuid, MaxDateTime, MinDateTime, D29AllFlags, Data30.NoFlags);
            //defaults
            exampleRecords[2] = new Example(Key2, default, default, default, default, (char)0x01, default, default, default, default, default, default, default,
                default, default, default, default, default, default, 3.141592653589793238462643383279m, default, "", default, new byte[0], default, default, default, default, default,
                default, default);
            //Random
            exampleRecords[3] = new Example(Key3,
                RandomBool, Nullable<bool?>(RandomBool),
                RandomByte, Nullable<byte?>(RandomByte),
                RandomChar, Nullable<char?>(RandomChar),
                RandomShort, Nullable<short?>(RandomShort),
                RandomInt, Nullable<int?>(RandomInt),
                RandomLong, Nullable<long?>(RandomLong),
                RandomULong, Nullable<ulong?>(RandomULong),
                RandomFloat, Nullable<float?>(RandomFloat),
                RandomDouble, Nullable<double?>(RandomDouble),
                RandomDecimal, Nullable<decimal?>(RandomDecimal),
                RandomString(RandomByte), Nullable<string?>(RandomString(RandomByte)),
                RandomByteArray(RandomByte), Nullable<byte[]?>(RandomByteArray(RandomByte)),
                RandomGuid, Nullable<Guid?>(RandomGuid),
                RandomDateTime, Nullable<DateTime?>(RandomDateTime),
                ReturnRandomValue((Data29[])Enum.GetValues(typeof(Data29))),
                Nullable<Data30?>(ReturnRandomValue((Data30[])Enum.GetValues(typeof(Data30)))));
            //Specials
            exampleRecords[4] = new Example(Key4, default, default, default, default, (char)0x01, default, default, default, default, default, default, default,
                default, default, FInf, FNegInf, DInf, DNegInf, default, default, AltMinString, AltMinString, new byte[0], new byte[0], default, default, default, default,
                default, default);
            exampleRecords[5] = new Example(Key5, default, default, default, default, (char)0x01, default, default, default, default, default, default, default,
                default, default, 9.9f, -9.9f, 18.18, -18.18, default, default, "", default, new byte[0], default, default, default, default, default,
                default, default);
            exampleRecords[6] = new Example(Key6, default, default, default, default, (char)0x01, default, default, default, default, default, default, default,
                default, default, FNegZero, FEps, DNegZero, -100.29, DecNegZero, default, "", default, new byte[0], default, default, default, default, default,
                default, default);
            #endregion
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (Sqlite sql = SQLDBs[i]())
                    {
                        await SQLTests.ClearData(sql);
                        await Example.CreateDB(sql);
                        for (int j = 0; j < exampleRecords.Length; j++)
                        {
                            await exampleRecords[j].Insert(sql);
                        }
                        for (int j = 0; j < exampleRecords.Length; j++)
                        {
                            Example? testRecord = await Example.SelectOne(sql, "\"ID\" = @0", exampleRecords[j].ID);
                            Example template = exampleRecords[j];
                            if (testRecord != template)
                            {
                                if (testRecord?.Data1 != template.Data1) return FAIL("Record not equal! Data1 did not match! " + testRecord?.Data1 + " != " + template.Data1);
                                if (testRecord.Data2 != template.Data2) return FAIL("Record not equal! Data2 did not match! " + testRecord.Data2 + " != " + template.Data2);
                                if (testRecord.Data3 != template.Data3) return FAIL("Record not equal! Data3 did not match! " + testRecord.Data3 + " != " + template.Data3);
                                if (testRecord.Data4 != template.Data4) return FAIL("Record not equal! Data4 did not match! " + testRecord.Data4 + " != " + template.Data4);
                                if (testRecord.Data5 != template.Data5) return FAIL("Record not equal! Data5 did not match! " + testRecord.Data5 + " != " + template.Data5);
                                if (testRecord.Data6 != template.Data6) return FAIL("Record not equal! Data6 did not match! " + testRecord.Data6 + " != " + template.Data6);
                                if (testRecord.Data7 != template.Data7) return FAIL("Record not equal! Data7 did not match! " + testRecord.Data7 + " != " + template.Data7);
                                if (testRecord.Data8 != template.Data8) return FAIL("Record not equal! Data8 did not match! " + testRecord.Data8 + " != " + template.Data8);
                                if (testRecord.Data9 != template.Data9) return FAIL("Record not equal! Data9 did not match! " + testRecord.Data9 + " != " + template.Data9);
                                if (testRecord.Data10 != template.Data10) return FAIL("Record not equal! Data10 did not match! " + testRecord.Data10 + " != " + template.Data10);
                                if (testRecord.Data11 != template.Data11) return FAIL("Record not equal! Data11 did not match! " + testRecord.Data11 + " != " + template.Data11);
                                if (testRecord.Data12 != template.Data12) return FAIL("Record not equal! Data12 did not match! " + testRecord.Data12 + " != " + template.Data12);
                                if (testRecord.Data13 != template.Data13) return FAIL("Record not equal! Data13 did not match! " + testRecord.Data13 + " != " + template.Data13);
                                if (testRecord.Data14 != template.Data14) return FAIL("Record not equal! Data14 did not match! " + testRecord.Data14 + " != " + template.Data14);
                                if (testRecord.Data15 != template.Data15 && (!float.IsNaN(testRecord.Data15) || !float.IsNaN(template.Data15))) return FAIL("Record not equal! Data15 did not match! " + testRecord.Data15 + " != " + template.Data15);
                                if (testRecord.Data16 != template.Data16 && (testRecord.Data16 == null || template.Data16 == null || !float.IsNaN(testRecord.Data16.Value) || !float.IsNaN(template.Data16.Value))) return FAIL("Record not equal! Data16 did not match! " + testRecord.Data16 + " != " + template.Data16);
                                if (testRecord.Data17 != template.Data17 && (!double.IsNaN(testRecord.Data17) || !double.IsNaN(template.Data17))) return FAIL("Record not equal! Data17 did not match! " + testRecord.Data17 + " != " + template.Data17);
                                if (testRecord.Data18 != template.Data18 && (testRecord.Data18 == null || template.Data18 == null || !double.IsNaN(testRecord.Data18.Value) || !double.IsNaN(template.Data18.Value))) return FAIL("Record not equal! Data18 did not match! " + testRecord.Data18 + " != " + template.Data18);
                                if (testRecord.Data19 != template.Data19) return FAIL("Record not equal! Data19 did not match! " + testRecord.Data19 + " != " + template.Data19);
                                if (testRecord.Data20 != template.Data20) return FAIL("Record not equal! Data20 did not match! " + testRecord.Data20 + " != " + template.Data20);
                                if (testRecord.Data21 != template.Data21) return FAIL("Record not equal! Data21 did not match! " + testRecord.Data21 + " != " + template.Data21);
                                if (testRecord.Data22 != template.Data22) return FAIL("Record not equal! Data22 did not match! " + testRecord.Data22 + " != " + template.Data22);
                                if (!testRecord.Data23.AsSpan().SequenceEqual(template.Data23)) return FAIL("Record not equal! Data23 did not match! " + testRecord.Data23 + " != " + template.Data23);
                                if (testRecord.Data24 != template.Data24 && testRecord.Data24?.AsSpan().SequenceEqual(template.Data24) != true) return FAIL("Record not equal! Data24 did not match! " + testRecord.Data24 + " != " + template.Data24);
                                if (testRecord.Data25 != template.Data25) return FAIL("Record not equal! Data25 did not match! " + testRecord.Data25 + " != " + template.Data25);
                                if (testRecord.Data26 != template.Data26) return FAIL("Record not equal! Data26 did not match! " + testRecord.Data26 + " != " + template.Data26);
                                if (testRecord.Data27 != template.Data27) return FAIL("Record not equal! Data27 did not match! " + testRecord.Data27 + " != " + template.Data27);
                                if (testRecord.Data28 != template.Data28) return FAIL("Record not equal! Data28 did not match! " + testRecord.Data28 + " != " + template.Data28);
                                if (testRecord.Data29 != template.Data29) return FAIL("Record not equal! Data29 did not match! " + testRecord.Data29 + " != " + template.Data29);
                                if (testRecord.Data30 != template.Data30) return FAIL("Record not equal! Data30 did not match! " + testRecord.Data30 + " != " + template.Data30);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return FAIL(e.ToString());
                }
            }
            return PASS();
        }

        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> Example3SimpleSQLTest()
        {
            Example3 ex3a = new Example3(Guid.Empty, "foo", "bar", 69);
            Example3 ex3b = new Example3(Guid.Empty, "food", "bar", 609);
            Example3 ex3c = new Example3(Guid.Empty, "french", "fries", 103);
            List<Func<Sqlite>> SQLDBs = SQLTests.GetSqliteDB;
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (Sqlite sql = SQLDBs[i]())
                    {
                        await SQLTests.ClearData(sql);
                        await Example3.CreateDB(sql);
                        await ex3a.Insert(sql);
                        await ex3b.Update(sql);
                        Example3? ex3bcomp = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                        if(ex3bcomp != ex3b) { return FAIL("Did not Update correctly!"); }
                        await ex3c.Upsert(sql);
                        Example3? ex3ccomp = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                        if (ex3ccomp != ex3c) { return FAIL("Did not Upsert correctly!"); }
                        await Example3.Truncate(sql);
                        Example3? emptycomp = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                        if (emptycomp != null) { return FAIL("Did not Truncate correctly!"); }
                        await ex3c.Upsert(sql);
                        Example3? ex3ccomp2 = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                        if (ex3ccomp2 != ex3c) { return FAIL("Did not Upsert correctly!"); }
                    }
                }
                catch (Exception e)
                {
                    return FAIL(e.ToString());
                }
            }
            return PASS();
        }

        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> ConditionGenerationTest()
        {
            var ExampleSet = new { Column1 = "SPAM", Column2 = "SPAMMER", Column3 = 22, Column4 = 2, Column5 = 6, Column6 = 6.5, Column7 = 3.0, Column8 = 5.0, Column9 = 4.5, Column10 = 22 };
            Conditional c = Conditional.WHERE("Column1", IS.EQUAL_TO, "SPAM")
                .AND("Column2", IS.NOT_EQUAL_TO, "SPAM")
                .AND("Column3", IS.GREATER_THAN, 4)
                .AND("Column4", IS.LESS_THAN, 5)
                .AND("Column5", IS.GREATER_THAN_OR_EQUAL_TO, 6)
                .AND("Column6", IS.LESS_THAN_OR_EQUAL_TO, 7)
                .AND("Column7", IS.IN, 1, 2, 3, 4, 5, 6, null)
                .AND("Column8", IS.IN, 1, 2, 3, 4, 5, 6)
                .AND("Column9", IS.NOT_IN, 1, 2, 3, 4, 5, 6, null)
                .AND("Column10", IS.NOT_IN, 1, 2, 3, 4, 5, 6);
            RecordParameter[] rps = ExampleSet.GetType().GetConstructors()[0].GetParameters().Select(x => new RecordParameter(x)).ToArray();
            List<Func<Sqlite>> SQLDBs = SQLTests.GetSqliteDB;
            using (Sqlite sql = SQLDBs[0]())
            {
                List<object> parameters = new List<object>();
                string condition = c.Build(sql,rps,ref parameters) + ";";
                if(condition != " WHERE" +
                    " \"Column1\" = @0 AND" +
                    " \"Column2\" != @0 AND" +
                    " \"Column3\" > @1 AND" +
                    " \"Column4\" < @2 AND" +
                    " \"Column5\" >= @3 AND" +
                    " \"Column6\" <= @4 AND" +
                    " (\"Column7\" IS NULL OR \"Column7\" IN (@5, @6, @7, @8, @9, @10)) AND" +
                    " \"Column8\" IN (@5, @6, @7, @8, @9, @10) AND" +
                    " (\"Column9\" IS NOT NULL AND \"Column9\" NOT IN (@5, @6, @7, @8, @9, @10)) AND" +
                    " \"Column10\" NOT IN (@11, @12, @13, @1, @2, @3);") { return FAIL("Invalid SQL\n" + condition); }
                if ((string)parameters[0] != "SPAM" ||
                    (int)parameters[1] != 4 ||
                    (int)parameters[2] != 5 ||
                    (int)parameters[3] != 6 ||
                    (double)parameters[4] != 7.0 ||
                    (double)parameters[5] != 1.0 ||
                    (double)parameters[6] != 2.0 ||
                    (double)parameters[7] != 3.0 ||
                    (double)parameters[8] != 4.0 ||
                    (double)parameters[9] != 5.0 ||
                    (double)parameters[10] != 6.0 ||
                    (int)parameters[11] != 1 ||
                    (int)parameters[12] != 2 ||
                    (int)parameters[13] != 3) { return FAIL("Invalid parameters!"); }
            }
            return PASS();
        }
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> WhereClauseSelectionTest()
        {
            Example4[] Ex4s = new Example4[100];
            for (int i = 0; i < Ex4s.Length; i++)
            {
                Ex4s[i] = new Example4(RandomVars.Guid(), RandomVars.Byte(1, 255), RandomVars.ULong(1, ulong.MaxValue));
            }
            Ex4s[0] = Ex4s[0] with { UnsignedLongTest = long.MaxValue };
            Ex4s[1] = Ex4s[1] with { UnsignedLongTest = (ulong)long.MaxValue + 1 };
            List<Func<Sqlite>> SQLDBs = SQLTests.GetSqliteDB;
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (Sqlite sql = SQLDBs[i]())
                    {
                        sql.BeginTransaction(System.Data.IsolationLevel.Serializable);
                        await SQLTests.ClearData(sql);
                        await Example4.CreateDB(sql);
                        for (int j = 0; j < Ex4s.Length; j++)
                        {
                            await Ex4s[j].Insert(sql);
                        }
                        sql.CommitTransaction();
                        for (int j = 0; j < Ex4s.Length; j++)
                        {
                            Example4? tester;
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.EQUAL_TO, Ex4s[j].ByteTest));
                            if(tester == null) { return FAIL("ByteTest Equal Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.EQUAL_TO, Ex4s[j].ByteTest - 1));
                            if (tester != null) { return FAIL("ByteTest Inverse Equal Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.LESS_THAN, Ex4s[j].ByteTest + 1));
                            if (tester == null) { return FAIL("ByteTest Less Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.LESS_THAN, Ex4s[j].ByteTest - 1));
                            if (tester != null) { return FAIL("ByteTest Inverse Less Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.GREATER_THAN, Ex4s[j].ByteTest - 1));
                            if (tester == null) { return FAIL("ByteTest Greater Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.GREATER_THAN, Ex4s[j].ByteTest + 1));
                            if (tester != null) { return FAIL("ByteTest Inverse Greater Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.BETWEEN, Ex4s[j].ByteTest - 1, Ex4s[j].ByteTest + 1));
                            if (tester == null) { return FAIL("ByteTest Between Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.BETWEEN, Ex4s[j].ByteTest + 1, Ex4s[j].ByteTest - 1));
                            if (tester != null) { return FAIL("ByteTest Inverse Between Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.EQUAL_TO, Ex4s[j].UnsignedLongTest));
                            if (tester == null) { return FAIL("UnsignedLongTest Equal Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.EQUAL_TO, Ex4s[j].UnsignedLongTest - 1));
                            if (tester != null) { return FAIL("UnsignedLongTest Inverse Equal Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.LESS_THAN, Ex4s[j].UnsignedLongTest + 1));
                            if (tester == null) { return FAIL("UnsignedLongTest Less Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.LESS_THAN, Ex4s[j].UnsignedLongTest - 1));
                            if (tester != null) { return FAIL("UnsignedLongTest Inverse Less Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.GREATER_THAN, Ex4s[j].UnsignedLongTest - 1));
                            if (tester == null) { return FAIL("UnsignedLongTest Greater Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.GREATER_THAN, Ex4s[j].UnsignedLongTest + 1));
                            if (tester != null) { return FAIL("UnsignedLongTest Inverse Greater Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.BETWEEN, Ex4s[j].UnsignedLongTest - 1, Ex4s[j].UnsignedLongTest + 1));
                            if (tester == null) { return FAIL("UnsignedLongTest Between Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.BETWEEN, Ex4s[j].UnsignedLongTest + 1, Ex4s[j].UnsignedLongTest - 1));
                            if (tester != null) { return FAIL("UnsignedLongTest Inverse Between Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.NOT_BETWEEN, Ex4s[j].UnsignedLongTest + 1, Ex4s[j].UnsignedLongTest - 1));
                            if (tester == null) { return FAIL("UnsignedLongTest Not Between Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.NOT_BETWEEN, Ex4s[j].UnsignedLongTest - 1, Ex4s[j].UnsignedLongTest + 1));
                            if (tester != null) { return FAIL("UnsignedLongTest Inverse Not Between Failure " + Ex4s[j].ToString()); }
#warning Add more tests for different types of queries and subqueries.
                        }
                    }
                }
                catch (Exception e)
                {
                    return FAIL(e.ToString());
                }
            }
            return PASS();
        }
                
    }
}
