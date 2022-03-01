using CSL.Encryption;
using CSL.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonStructureLibraryTester.Testing
{
    public class RandomTests : Tests
    {
        private static bool HasZeros(int[] array)
        {
            for(int i = 0; i < array.Length; i++)
            {
                if(array[i] == 0)
                {
                    return true;
                }
            }
            return false;
        }
        #region Bytes
        protected static TestResponse SimpleRandomByteTest()
        {
            int[] byteList = new int[byte.MaxValue + 1];
            int counter = 0;
            while(HasZeros(byteList) && counter < 1000)
            {
                for(int i = 0; i < 1000; i++)
                {
                    byteList[RandomVars.Byte()]++;
                }
                counter++;
            }
            for (int i = 0; i < byteList.Length; i++)
            {
                if (byteList[i] == 0)
                {
                    return FAIL($"Random Byte Generation never selected {i}.");
                }
            }
            return PASS();
        }
        protected static TestResponse RangedRandomByteTest()
        {
            int[] byteList = new int[9];
            int counter = 0;
            while (HasZeros(byteList) && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    byteList[RandomVars.Byte(9)]++;
                }
                counter++;
            }
            for (int i = 0; i < byteList.Length; i++)
            {
                if (byteList[i] == 0)
                {
                    return FAIL($"Random Byte Generation never selected {i}.");
                }
            }
            return PASS();
        }
        protected static TestResponse DoubleRangedRandomByteTest()
        {
            int[] byteList = new int[9];
            int counter = 0;
            while (HasZeros(byteList) && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    byteList[RandomVars.Byte(9,18) - 9]++;
                }
                counter++;
            }
            for (int i = 0; i < byteList.Length; i++)
            {
                if (byteList[i] == 0)
                {
                    return FAIL($"Random Byte Generation never selected {i}.");
                }
            }
            return PASS();
        }
        protected static TestResponse ReverseDoubleRangedRandomByteTest()
        {
            int[] byteList = new int[9];
            int counter = 0;
            while (HasZeros(byteList) && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    byteList[RandomVars.Byte(17, 8) - 9]++;
                }
                counter++;
            }
            for (int i = 0; i < byteList.Length; i++)
            {
                if (byteList[i] == 0)
                {
                    return FAIL($"Random Byte Generation never selected {i}.");
                }
            }
            return PASS();
        }
        #endregion

        #region SBytes
        protected static TestResponse SimpleRandomSByteTest()
        {
            Dictionary<sbyte, int> DataList = new Dictionary<sbyte, int>();
            for(int i = sbyte.MinValue; i <= sbyte.MaxValue; i++)
            {
                DataList[(sbyte)i] = 0;
            }
            int counter = 0;
            while (DataList.Values.Where(x => x == 0).Any() && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    DataList[RandomVars.SByte()]++;
                }
                counter++;
            }

            foreach(KeyValuePair<sbyte,int> kp in DataList)
            {
                if (kp.Value == 0)
                {
                    return FAIL($"Random Byte Generation never selected {kp.Key}.");
                }
            }
            return PASS();
        }
        protected static TestResponse RangedRandomSByteTest()
        {
            Dictionary<sbyte, int> DataList = new Dictionary<sbyte, int>();
            for (int i = 0; i < 9; i++)
            {
                DataList[(sbyte)i] = 0;
            }
            int counter = 0;
            while (DataList.Values.Where(x => x == 0).Any() && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    DataList[RandomVars.SByte(9)]++;
                }
                counter++;
            }

            foreach (KeyValuePair<sbyte, int> kp in DataList)
            {
                if (kp.Value == 0)
                {
                    return FAIL($"Random Byte Generation never selected {kp.Key}.");
                }
            }
            return PASS();
        }
        protected static TestResponse DoubleRangedRandomSByteTest()
        {
            Dictionary<sbyte, int> DataList = new Dictionary<sbyte, int>();
            for (int i = 9; i < 18; i++)
            {
                DataList[(sbyte)i] = 0;
            }
            int counter = 0;
            while (DataList.Values.Where(x => x == 0).Any() && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    DataList[RandomVars.SByte(9, 18)]++;
                }
                counter++;
            }
            foreach (KeyValuePair<sbyte, int> kp in DataList)
            {
                if (kp.Value == 0)
                {
                    return FAIL($"Random Byte Generation never selected {kp.Key}.");
                }
            }
            return PASS();
        }
        protected static TestResponse ReverseDoubleRangedRandomSByteTest()
        {
            Dictionary<sbyte, int> DataList = new Dictionary<sbyte, int>();
            for (int i = 17; i > 17; i--)
            {
                DataList[(sbyte)i] = 0;
            }
            int counter = 0;
            while (DataList.Values.Where(x => x == 0).Any() && counter < 1000)
            {
                for (int i = 0; i < 1000; i++)
                {
                    DataList[RandomVars.SByte(17, 8)]++;
                }
                counter++;
            }
            foreach (KeyValuePair<sbyte, int> kp in DataList)
            {
                if (kp.Value == 0)
                {
                    return FAIL($"Random Byte Generation never selected {kp.Key}.");
                }
            }
            return PASS();
        }
        #endregion

        #region Other Random Tests
        protected static TestResponse StaticDecimalTests()
        {
            decimal[] StaticTests = new decimal[] {
            new decimal(-1, -1, -1, true, 28),
            new decimal(-1, -1, -1, false, 28),
            new decimal(-1, -1, -1, true, 0),
            new decimal(-1, -1, -1, false, 0),
            new decimal(0, 0, 0, true, 28),
            new decimal(0, 0, 0, false, 28),
            new decimal(0, 0, 0, true, 0),
            new decimal(0, 0, 0, false, 0),
            };
            if(StaticTests != null)
            {
                return PASS();
            }
            return FAIL();
        }
        protected static TestResponse RandomDecimalTests()
        {
            List<decimal> RandomTests = new List<decimal>();
            for(int i = 0; i < 10000; i++)
            {
                RandomTests.Add(RandomVars.Decimal());
            }
            if (RandomTests != null)
            {
                return PASS();
            }
            return FAIL();
        }
        protected static TestResponse StaticDateTimeTests()
        {
            long Max = DateTime.MaxValue.Ticks;
            long Min = DateTime.MinValue.Ticks;
            DateTime[] StaticTests = new DateTime[] {
                new DateTime(Max,DateTimeKind.Unspecified),
                new DateTime(Max,DateTimeKind.Utc),
                new DateTime(Max,DateTimeKind.Local),
                new DateTime(Min,DateTimeKind.Unspecified),
                new DateTime(Min,DateTimeKind.Utc),
                new DateTime(Min,DateTimeKind.Local),
            };
            if (StaticTests != null)
            {
                return PASS();
            }
            return FAIL();
        }
        protected static TestResponse RandomDateTimeTests()
        {
            List<DateTime> RandomTests = new List<DateTime>();
            for (int i = 0; i < 10000; i++)
            {
                RandomTests.Add(RandomVars.DateTime());
            }
            if (RandomTests != null)
            {
                return PASS();
            }
            return FAIL();
        }
        protected static TestResponse StringTests()
        {
            string test = RandomVars.String(65535);
            if(test.Length != 65535)
            {
                return FAIL("Random char error");
            }
            return PASS();
        }
        #endregion
    }
}
