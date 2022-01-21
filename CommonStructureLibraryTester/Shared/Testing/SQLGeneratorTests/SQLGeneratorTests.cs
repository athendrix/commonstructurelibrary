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

namespace CommonStructureLibraryTester.Testing
{
    public class SQLGeneratorTests : Tests
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
                            Example? testRecord = await Example.SelectBy_ID(sql, exampleRecords[j].ID);
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
    }
}
