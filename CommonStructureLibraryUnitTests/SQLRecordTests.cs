using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CSL.Encryption;
using CSL.SQL;
using NUnit.Framework;
using static CommonStructureLibraryUnitTests.TestingHelpers;

namespace CommonStructureLibraryUnitTests;

public class SQLRecordTests
{
    #region Testing Structures
    record GuidBuildTestRecord(string test, Guid? test2) : SQLRecord<GuidBuildTestRecord>;
    record Example(Guid ID, bool Data1, bool? Data2, byte Data3, byte? Data4, char Data5, char? Data6,
                          short Data7, short? Data8, int Data9, int? Data10, long Data11, long? Data12, ulong Data13, ulong? Data14,
                          float Data15, float? Data16, double Data17, double? Data18, decimal Data19, decimal? Data20, string Data21,
                          string? Data22, byte[] Data23, byte[]? Data24, Guid Data25, Guid? Data26, DateTime Data27, DateTime? Data28,
                          Data29 Data29, Data30? Data30) : SQLRecord<Example>;
    record Example3(Guid ID, string Data1, string? Data2, int Data25) : SQLRecord<Example3>;
    record Example4(Guid ID, byte ByteTest, ulong UnsignedLongTest) : SQLRecord<Example4>;
    [Flags]
    //Specifying ulong allows data to be auto converted for your convenience into the database.
    public enum Data29 : ulong
    {
        NoFlags = 0,
        Flag1 = 1UL  << 0,
        Flag2 = 1UL  << 1,
        Flag3 = 1UL  << 2,
        Flag4 = 1UL  << 3,
        Flag5 = 1UL  << 4,
        Flag6 = 1UL  << 5,
        Flag7 = 1UL  << 6,
        Flag8 = 1UL  << 7,
        Flag9 = 1UL  << 8,
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
        Flag1 = 1UL  << 0,
        Flag2 = 1UL  << 1,
        Flag3 = 1UL  << 2,
        Flag4 = 1UL  << 3,
        Flag5 = 1UL  << 4,
        Flag6 = 1UL  << 5,
        Flag7 = 1UL  << 6,
        Flag8 = 1UL  << 7,
        Flag9 = 1UL  << 8,
        Flag10 = 1UL << 9,
        Flag11 = 1UL << 10,
        Flag12 = 1UL << 11,
        Flag13 = 1UL << 12,
        Flag14 = 1UL << 13,
        Flag15 = 1UL << 14,
        Flag16 = 1UL << 15,
    }
    #endregion
    [SetUp]
    public void Setup()
    {
        CSL.DependencyInjection.SqliteConnectionConstructor       = (x) => new Microsoft.Data.Sqlite.SqliteConnection(x);
        CSL.DependencyInjection.SqliteConnectionStringConstructor = () => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
        CSL.DependencyInjection.SqliteOpenModeConverter           = (x) => (Microsoft.Data.Sqlite.SqliteOpenMode)x;
        CSL.DependencyInjection.SqliteCacheModeConverter          = (x) => (Microsoft.Data.Sqlite.SqliteCacheMode)x;
    }
    [Test]
    public async Task NullableGuidBuildTest()
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
        Assert.Pass();
    }

    [Test]
    public async Task Example1BasicFunctionalityTest()
    {
        #region DataInit
        Data29 D29AllFlags = Data29.Flag1  | Data29.Flag2  | Data29.Flag3  | Data29.Flag4  | Data29.Flag5  |
                             Data29.Flag6  | Data29.Flag7  | Data29.Flag8  | Data29.Flag9  | Data29.Flag10 | Data29.Flag11 |
                             Data29.Flag12 | Data29.Flag13 | Data29.Flag14 | Data29.Flag15 | Data29.Flag16;
        Data30 D30AllFlags = Data30.Flag1  | Data30.Flag2  | Data30.Flag3  | Data30.Flag4  | Data30.Flag5  |
                             Data30.Flag6  | Data30.Flag7  | Data30.Flag8  | Data30.Flag9  | Data30.Flag10 | Data30.Flag11 |
                             Data30.Flag12 | Data30.Flag13 | Data30.Flag14 | Data30.Flag15 | Data30.Flag16;
        Guid      Key0           = MinGuid;
        Guid      Key1           = MaxGuid;
        Guid      Key2           = RandomGuid;
        Guid      Key3           = RandomGuid;
        Guid      Key4           = RandomGuid;
        Guid      Key5           = RandomGuid;
        Guid      Key6           = RandomGuid;
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

        try
        {
            using (Sqlite sql = new Sqlite(":memory:"))
            {
                await Example.CreateDB(sql);
                for (int j = 0; j < exampleRecords.Length; j++)
                {
                    await exampleRecords[j].Insert(sql);
                }
                for (int j = 0; j < exampleRecords.Length; j++)
                {
                    Example? testRecord = await Example.SelectOne(sql, "\"ID\" = @0", exampleRecords[j].ID);
                    Example  template   = exampleRecords[j];
                    if (testRecord != template)
                    {
                        Assert.That(testRecord?.Data1, Is.EqualTo(template.Data1), "Record not equal! Data1 did not match! "                                                                                                                                                     + testRecord?.Data1 + " != " + template.Data1);
                        Assert.That(testRecord?.Data2, Is.EqualTo(template.Data2), "Record not equal! Data2 did not match! "                                                                                                                                                      + testRecord.Data2  + " != " + template.Data2);
                        Assert.That(testRecord.Data3, Is.EqualTo(template.Data3), "Record not equal! Data3 did not match! "                                                                                                                                                      + testRecord.Data3  + " != " + template.Data3);
                        Assert.That(testRecord.Data4, Is.EqualTo(template.Data4), "Record not equal! Data4 did not match! "                                                                                                                                                      + testRecord.Data4  + " != " + template.Data4);
                        Assert.That(testRecord.Data5, Is.EqualTo(template.Data5), "Record not equal! Data5 did not match! "                                                                                                                                                      + testRecord.Data5  + " != " + template.Data5);
                        Assert.That(testRecord.Data6, Is.EqualTo(template.Data6), "Record not equal! Data6 did not match! "                                                                                                                                                      + testRecord.Data6  + " != " + template.Data6);
                        Assert.That(testRecord.Data7, Is.EqualTo(template.Data7), "Record not equal! Data7 did not match! "                                                                                                                                                      + testRecord.Data7  + " != " + template.Data7);
                        Assert.That(testRecord.Data8, Is.EqualTo(template.Data8), "Record not equal! Data8 did not match! "                                                                                                                                                      + testRecord.Data8  + " != " + template.Data8);
                        Assert.That(testRecord.Data9, Is.EqualTo(template.Data9), "Record not equal! Data9 did not match! "                                                                                                                                                      + testRecord.Data9  + " != " + template.Data9);
                        Assert.That(testRecord.Data10, Is.EqualTo(template.Data10), "Record not equal! Data10 did not match! "                                                                                                                                                   + testRecord.Data10 + " != " + template.Data10);
                        Assert.That(testRecord.Data11, Is.EqualTo(template.Data11), "Record not equal! Data11 did not match! "                                                                                                                                                   + testRecord.Data11 + " != " + template.Data11);
                        Assert.That(testRecord.Data12, Is.EqualTo(template.Data12), "Record not equal! Data12 did not match! "                                                                                                                                                   + testRecord.Data12 + " != " + template.Data12);
                        Assert.That(testRecord.Data13, Is.EqualTo(template.Data13), "Record not equal! Data13 did not match! "                                                                                                                                                   + testRecord.Data13 + " != " + template.Data13);
                        Assert.That(testRecord.Data14, Is.EqualTo(template.Data14), "Record not equal! Data14 did not match! "                                                                                                                                                   + testRecord.Data14 + " != " + template.Data14);
                        Assert.That(testRecord.Data15 != template.Data15 && (!float.IsNaN(testRecord.Data15)  || !float.IsNaN(template.Data15)), Is.False, "Record not equal! Data15 did not match! "                                                                             + testRecord.Data15 + " != " + template.Data15);
                        Assert.That(testRecord.Data16 != template.Data16 && (testRecord.Data16 == null        || template.Data16 == null || !float.IsNaN(testRecord.Data16.Value) || !float.IsNaN(template.Data16.Value)), Is.False, "Record not equal! Data16 did not match! "   + testRecord.Data16 + " != " + template.Data16);
                        Assert.That(testRecord.Data17 != template.Data17 && (!double.IsNaN(testRecord.Data17) || !double.IsNaN(template.Data17)), Is.False, "Record not equal! Data17 did not match! "                                                                            + testRecord.Data17 + " != " + template.Data17);
                        Assert.That(testRecord.Data18 != template.Data18 && (testRecord.Data18 == null        || template.Data18 == null || !double.IsNaN(testRecord.Data18.Value) || !double.IsNaN(template.Data18.Value)), Is.False, "Record not equal! Data18 did not match! " + testRecord.Data18 + " != " + template.Data18);
                        Assert.That(testRecord.Data19, Is.EqualTo(template.Data19), "Record not equal! Data19 did not match! "                                                                                                                                                   + testRecord.Data19 + " != " + template.Data19);
                        Assert.That(testRecord.Data20, Is.EqualTo(template.Data20), "Record not equal! Data20 did not match! "                                                                                                                                                   + testRecord.Data20 + " != " + template.Data20);
                        Assert.That(testRecord.Data21, Is.EqualTo(template.Data21), "Record not equal! Data21 did not match! "                                                                                                                                                   + testRecord.Data21 + " != " + template.Data21);
                        Assert.That(testRecord.Data22, Is.EqualTo(template.Data22), "Record not equal! Data22 did not match! "                                                                                                                                                   + testRecord.Data22 + " != " + template.Data22);
                        Assert.That(testRecord.Data23.AsSpan().SequenceEqual(template.Data23), Is.True, "Record not equal! Data23 did not match! "                                                                                                                                + testRecord.Data23 + " != " + template.Data23);
                        Assert.That(testRecord.Data24 != template.Data24 && testRecord.Data24?.AsSpan().SequenceEqual(template.Data24) != true, Is.False, "Record not equal! Data24 did not match! "                                                                              + testRecord.Data24 + " != " + template.Data24);
                        Assert.That(testRecord.Data25, Is.EqualTo(template.Data25), "Record not equal! Data25 did not match! "                                                                                                                                                   + testRecord.Data25 + " != " + template.Data25);
                        Assert.That(testRecord.Data26, Is.EqualTo(template.Data26), "Record not equal! Data26 did not match! "                                                                                                                                                   + testRecord.Data26 + " != " + template.Data26);
                        Assert.That(testRecord.Data27, Is.EqualTo(template.Data27), "Record not equal! Data27 did not match! "                                                                                                                                                   + testRecord.Data27 + " != " + template.Data27);
                        Assert.That(testRecord.Data28, Is.EqualTo(template.Data28), "Record not equal! Data28 did not match! "                                                                                                                                                   + testRecord.Data28 + " != " + template.Data28);
                        Assert.That(testRecord.Data29, Is.EqualTo(template.Data29), "Record not equal! Data29 did not match! "                                                                                                                                                   + testRecord.Data29 + " != " + template.Data29);
                        Assert.That(testRecord.Data30, Is.EqualTo(template.Data30), "Record not equal! Data30 did not match! "                                                                                                                                                   + testRecord.Data30 + " != " + template.Data30);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
        Assert.Pass();
    }
    [Test]
    public async Task Example3SimpleSQLTest()
    {
        Example3 ex3a = new Example3(Guid.Empty, "foo", "bar", 69);
        Example3 ex3b = new Example3(Guid.Empty, "food", "bar", 609);
        Example3 ex3c = new Example3(Guid.Empty, "french", "fries", 103);
        try
        {
            using (Sqlite sql = new Sqlite(":memory:"))
            {
                await Example3.CreateDB(sql);
                await ex3a.Insert(sql);
                await ex3b.Update(sql);
                Example3? ex3bcomp = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                Assert.That(ex3bcomp, Is.EqualTo(ex3b), "Did not Update correctly!");
                await ex3c.Upsert(sql);
                Example3? ex3ccomp = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                Assert.That(ex3ccomp, Is.EqualTo(ex3c), "Did not Upsert correctly!");
                await Example3.Truncate(sql);
                Example3? emptycomp = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                Assert.That(emptycomp, Is.Null, "Did not Truncate correctly");
                await ex3c.Upsert(sql);
                Example3? ex3ccomp2 = await Example3.SelectOne(sql, "\"ID\" = @0", Guid.Empty);
                Assert.That(ex3ccomp2, Is.EqualTo(ex3c), "Did not Upsert correctly!");
            }
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }

        Assert.Pass();
    }
    [Test]
    public async Task ConditionGenerationTest()
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
            ParameterInfo[] pis = ExampleSet.GetType().GetConstructors()[0].GetParameters();
            using (Sqlite sql = new Sqlite(":memory:"))
            {
                List<object> parameters = new List<object>();
                string condition = c.Build(sql,pis,ref parameters) + ";";
                Assert.That(condition, Is.EqualTo(" WHERE" +
                    " \"Column1\" = @0 AND" +
                    " \"Column2\" != @0 AND" +
                    " \"Column3\" > @1 AND" +
                    " \"Column4\" < @2 AND" +
                    " \"Column5\" >= @3 AND" +
                    " \"Column6\" <= @4 AND" +
                    " (\"Column7\" IS NULL OR \"Column7\" IN (@5, @6, @7, @8, @9, @10)) AND" +
                    " \"Column8\" IN (@5, @6, @7, @8, @9, @10) AND" +
                    " (\"Column9\" IS NOT NULL AND \"Column9\" NOT IN (@5, @6, @7, @8, @9, @10)) AND" +
                    " \"Column10\" NOT IN (@11, @12, @13, @1, @2, @3);"), "Invalid SQL\n" + condition);
                Assert.That((string)parameters[0] != "SPAM" ||
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
                    (int)parameters[13] != 3, Is.False,"Invalid parameters!");
            }
            Assert.Pass();
        }
    [Test]
        public async Task WhereClauseSelectionTest()
        {
            Example4[] Ex4s = new Example4[100];
            for (int i = 0; i < Ex4s.Length; i++)
            {
                Ex4s[i] = new Example4(RandomVars.Guid(), RandomVars.Byte(1, 255), RandomVars.ULong(1, ulong.MaxValue));
            }
            Ex4s[0] = Ex4s[0] with { UnsignedLongTest = long.MaxValue };
            Ex4s[1] = Ex4s[1] with { UnsignedLongTest = (ulong)long.MaxValue + 1 };

                try
                {
                    using (Sqlite sql = new Sqlite(":memory:"))
                    {
                        sql.BeginTransaction(System.Data.IsolationLevel.Serializable);
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
                            if(tester == null) { Assert.Fail("ByteTest Equal Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.EQUAL_TO, Ex4s[j].ByteTest - 1));
                            if (tester != null) { Assert.Fail("ByteTest Inverse Equal Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.LESS_THAN, Ex4s[j].ByteTest + 1));
                            if (tester == null) { Assert.Fail("ByteTest Less Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.LESS_THAN, Ex4s[j].ByteTest - 1));
                            if (tester != null) { Assert.Fail("ByteTest Inverse Less Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.GREATER_THAN, Ex4s[j].ByteTest - 1));
                            if (tester == null) { Assert.Fail("ByteTest Greater Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.GREATER_THAN, Ex4s[j].ByteTest + 1));
                            if (tester != null) { Assert.Fail("ByteTest Inverse Greater Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.BETWEEN, Ex4s[j].ByteTest - 1, Ex4s[j].ByteTest + 1));
                            if (tester == null) { Assert.Fail("ByteTest Between Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("ByteTest", IS.BETWEEN, Ex4s[j].ByteTest + 1, Ex4s[j].ByteTest - 1));
                            if (tester != null) { Assert.Fail("ByteTest Inverse Between Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.EQUAL_TO, Ex4s[j].UnsignedLongTest));
                            if (tester == null) { Assert.Fail("UnsignedLongTest Equal Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.EQUAL_TO, Ex4s[j].UnsignedLongTest - 1));
                            if (tester != null) { Assert.Fail("UnsignedLongTest Inverse Equal Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.LESS_THAN, Ex4s[j].UnsignedLongTest + 1));
                            if (tester == null) { Assert.Fail("UnsignedLongTest Less Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.LESS_THAN, Ex4s[j].UnsignedLongTest - 1));
                            if (tester != null) { Assert.Fail("UnsignedLongTest Inverse Less Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.GREATER_THAN, Ex4s[j].UnsignedLongTest - 1));
                            if (tester == null) { Assert.Fail("UnsignedLongTest Greater Than Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.GREATER_THAN, Ex4s[j].UnsignedLongTest + 1));
                            if (tester != null) { Assert.Fail("UnsignedLongTest Inverse Greater Than Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.BETWEEN, Ex4s[j].UnsignedLongTest - 1, Ex4s[j].UnsignedLongTest + 1));
                            if (tester == null) { Assert.Fail("UnsignedLongTest Between Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.BETWEEN, Ex4s[j].UnsignedLongTest + 1, Ex4s[j].UnsignedLongTest - 1));
                            if (tester != null) { Assert.Fail("UnsignedLongTest Inverse Between Failure " + Ex4s[j].ToString()); }

                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.NOT_BETWEEN, Ex4s[j].UnsignedLongTest + 1, Ex4s[j].UnsignedLongTest - 1));
                            if (tester == null) { Assert.Fail("UnsignedLongTest Not Between Failure " + Ex4s[j].ToString()); }
                            tester = await Example4.SelectOne(sql, Conditional.WHERE("ID", IS.EQUAL_TO, Ex4s[j].ID).AND("UnsignedLongTest", IS.NOT_BETWEEN, Ex4s[j].UnsignedLongTest - 1, Ex4s[j].UnsignedLongTest + 1));
                            if (tester != null) { Assert.Fail("UnsignedLongTest Inverse Not Between Failure " + Ex4s[j].ToString()); }
#warning Add more tests for different types of queries and subqueries.
                        }
                    }
                }
                catch (Exception e)
                {
                    Assert.Fail(e.ToString());
                }

                Assert.Pass();
        }
                
}