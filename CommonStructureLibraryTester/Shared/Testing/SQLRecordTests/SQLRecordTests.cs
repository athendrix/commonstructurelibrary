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
using static CommonStructureLibraryTester.Shared.Testing.TestingHelpers;
using System.Reflection;

using static CSL.SQL.Conditional;

namespace CommonStructureLibraryTester.Testing
{
    public class SQLRecordTests : Tests
    {
        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> Example1BasicFunctionalityTest()
        {
            #region DataInit
            List<Func<Task<PostgreSQL>>> SQLDBs = SQLTests.GetTestDB;
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
                default, default, default, default, default, default, default, default, "", default, new byte[0], default, default, default, default, default,
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
                default, default, FNaN, Fi, DNaN, Di, default, default, "", default, new byte[0], default, default, default, default, default,
                default, default);
            exampleRecords[6] = new Example(Key6, default, default, default, default, (char)0x01, default, default, default, default, default, default, default,
                default, default, FNegZero, FEps, DNegZero, Di, DecNegZero, default, "", default, new byte[0], default, default, default, default, default,
                default, default);
            #endregion
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (PostgreSQL sql = await SQLTests.GetTestDB[i]())
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
            List<Func<Task<PostgreSQL>>> SQLDBs = SQLTests.GetTestDB;
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (PostgreSQL sql = await SQLTests.GetTestDB[i]())
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

        [TestType(TestType.Both)]
        protected static TestResponse ConditionGenerationTest()
        {
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
            ParameterInfo[] pis = (new { Column1 = "SPAM", Column2 = "SPAMMER", Column3 = 22, Column4 = 2, Column5 = 6, Column6 = 6.5, Column7 = 3.0, Column8 = 5.0, Column9 = 4.5, Column10 = 22 })
                .GetType().GetConstructors()[0].GetParameters();

            List<object> parameters = new List<object>();
            string condition = c.Build(BuildType.PostgreSQL, pis, ref parameters) + ";";
            if (condition != " WHERE" +
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
            return PASS();
        }

        [TestType(TestType.ServerSide)]
        protected static TestResponse ExtraLinesGenerationTest()
        {
            using (Sqlite test = new Sqlite(":memory:"))
            {
                string testsql = "CREATE TABLE IF NOT EXISTS \"Example2\" " +
                    "(\"ID\" TEXT NOT NULL, " +
                    "\"ID2\" TEXT NOT NULL, " +
                    "\"ID3\" TEXT NOT NULL, " +
                    "\"ID4\" TEXT NOT NULL, " +
                    "\"Company\" TEXT NOT NULL, " +
                    "\"WrongClient\" TEXT NOT NULL, " +
                    "\"Client\" TEXT NOT NULL, " +
                    "\"NormalFK\" TEXT NOT NULL, " +
                    "PRIMARY KEY(\"ID\", \"ID2\", \"ID3\", \"ID4\"), " +
                    "FOREIGN KEY(\"Company\", \"Client\") REFERENCES \"OtherTable\"(\"CompanyID\", \"ClientID\"), " +
                    "FOREIGN KEY(\"WrongClient\") REFERENCES \"WrongTable\"(\"ClientID\"), " +
                    "FOREIGN KEY(\"NormalFK\") REFERENCES \"YetAnotherTable\"(\"SomeID\"));";
                string generatedsql = Example2.CreateDBSQLCode(test);
                if (generatedsql == testsql)
                {
                    return PASS();
                }
                return FAIL($"SQL was:{Environment.NewLine}{generatedsql}");
            }
                
        }

        record ConditionTestRecord(int key, string data, DateTime recordtime, ulong hardcase) : SQLRecord<ConditionTestRecord>;
        [TestType(TestType.ServerSide)]
        protected async static Task<TestResponse> AdvancedConditionGenerationTest()
        {
            List<object> DummyList = new List<object>();

            string[] responses = new string[9]; 
            using (PostgreSQL sql = await SQLTests.GetTestDB[0]())
            {
                responses[0] = WHERE(false).Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                responses[1] = WHERE(true).Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                responses[2] = WHERE("key", IS.GREATER_THAN, 0)
                    .AND(WHERE("data", IS.CONTAINING, "foo").OR("data", IS.CONTAINING, "bar"))
                    .Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                responses[3] = WHERE(true).ORDERBY("hardcase").LIMIT(5)
                    .Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                responses[4] = WHERE(true).ORDERBYDESC("recordtime").LIMIT(3, 5)
                    .Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                WhereClauseSegment wcs = WHERE(false);
                for(int i = 0; i < 10; i++)
                {
                    wcs = wcs.OR("data", IS.CONTAINING, $"{i}");
                }
                responses[5] = wcs.Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                wcs = WHERE(true);
                for (int i = 0; i < 10; i++)
                {
                    wcs = wcs.AND("data", IS.NOT_CONTAINING, $"{i}");
                }
                responses[6] = wcs.ORDERBY("key").Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);

                DummyList.Clear();
                responses[7] = WHERE("hardcase", IS.GREATER_THAN, long.MaxValue).AND("hardcase", IS.BETWEEN, 22, long.MaxValue + 22ul).ORDERBYDESC("recordtime").LIMIT(3, 5)
                    .Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
                DummyList.Clear();
                responses[8] = WHERE("hardcase", IS.NOT_EQUAL_TO, null).Build(BuildType.PostgreSQL, ConditionTestRecord.RecordParameters, ref DummyList);
            }
            string[] tests = new string[]
            {
                " WHERE 1 = 0",
                " WHERE 1 = 1",
                " WHERE \"key\" > @0 AND (\"data\" LIKE @1 OR \"data\" LIKE @2)",
                " WHERE 1 = 1 ORDER BY \"hardcase\" ASC LIMIT 5",
                " WHERE 1 = 1 ORDER BY \"recordtime\" DESC LIMIT 3 OFFSET 5",
                " WHERE 1 = 0 OR \"data\" LIKE @0 OR \"data\" LIKE @1 OR \"data\" LIKE @2 OR \"data\" LIKE @3 OR \"data\" LIKE @4 OR \"data\" LIKE @5 OR \"data\" LIKE @6 OR \"data\" LIKE @7 OR \"data\" LIKE @8 OR \"data\" LIKE @9",
                " WHERE 1 = 1 AND \"data\" NOT LIKE @0 AND \"data\" NOT LIKE @1 AND \"data\" NOT LIKE @2 AND \"data\" NOT LIKE @3 AND \"data\" NOT LIKE @4 AND \"data\" NOT LIKE @5 AND \"data\" NOT LIKE @6 AND \"data\" NOT LIKE @7 AND \"data\" NOT LIKE @8 AND \"data\" NOT LIKE @9 ORDER BY \"key\" ASC",
                " WHERE (\"hardcase\" < 0 AND @0 >= 0 OR \"hardcase\" > @0 AND (\"hardcase\" < 0 OR @0 >= 0)) AND ((\"hardcase\" < 0 AND @1 >= 0 OR \"hardcase\" >= @1 AND (\"hardcase\" < 0 OR @1 >= 0)) AND (\"hardcase\" >= 0 AND @2 < 0 OR \"hardcase\" <= @2 AND (\"hardcase\" >= 0 OR @2 < 0))) ORDER BY \"recordtime\" DESC LIMIT 3 OFFSET 5",
                " WHERE \"hardcase\" IS NOT NULL"
            };
            for(int i = 0; i < tests.Length; i++)
            {
                if (responses[i] != tests[i])
                {
                    return FAIL($"TEST FAILED\nEXCECTED:{tests[i]}\nGOT:{responses[i]}");
                }
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
            List<Func<Task<PostgreSQL>>> SQLDBs = SQLTests.GetTestDB;
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (PostgreSQL sql = await SQLTests.GetTestDB[i]())
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

        [TestType(TestType.ServerSide)]
        protected static async Task<TestResponse> CustomTableNameTest()
        {
            #region DataInit
            List<Func<Task<PostgreSQL>>> SQLDBs = SQLTests.GetTestDB;
            DB.uniqueprotocoltermsWD Test1 = new DB.uniqueprotocoltermsWD("foo", "bar", "baz", "qux", "spam", null, null, null, null, null, null, null);
            DB.uniqueprotocoltermsWD Test2 = new DB.uniqueprotocoltermsWD("food", "bard", "barz", "quex", "spammed", Guid.Empty, Guid.Empty, Guid.Empty, "true", DateTime.Now, "no", DateTime.UtcNow);
            #endregion
            for (int i = 0; i < SQLDBs.Count; i++)
            {
                try
                {
                    using (PostgreSQL sql = await SQLTests.GetTestDB[i]())
                    {
                        await SQLTests.ClearData(sql);
                        await DB.uniqueprotocoltermsWD.CreateDB(sql);
                        await Test1.Insert(sql);
                        await Test2.Insert(sql);
                        DB.uniqueprotocoltermsWD[] results = await DB.uniqueprotocoltermsWD.SelectArray(sql);
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
    public partial class DB
    {
        [TableName("uniqueprotocolterms")]
        [SQLRecord(5)]
        public record uniqueprotocoltermsWD(string sponsor, string protocol, string verbatimterm, string route, string indication, Guid? sponsorguid, Guid? protocolguid,
     Guid? pcid, string? pmoapprove, DateTime? pmoapprovedt, string? cmoapprove, DateTime? cmoapprovedt) : SQLRecord<uniqueprotocoltermsWD>;
    }
}
