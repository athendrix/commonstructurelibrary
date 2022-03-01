using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CSL.Encryption
{
    public class RandomVars
    {
        static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        public static byte[] ByteArray(byte length)
        {
            byte[] toReturn = new byte[length];
            RNG.GetBytes(toReturn);
            return toReturn;
        }
        public static T PickOne<T>(params T[] input) => input[Int(input.Length)];
        public static Guid Guid() => new Guid(ByteArray(16));
        public static bool Bool() => PickOne(true, false);

        public static byte Byte() => ByteArray(1)[0];
        public static sbyte SByte() => (sbyte)ByteArray(1)[0];

        public static char Char() => BitConverter.ToChar(ByteArray(2), 0);

        public static ushort UShort() => BitConverter.ToUInt16(ByteArray(2), 0);
        public static short Short() => BitConverter.ToInt16(ByteArray(2), 0);

        public static uint UInt() => BitConverter.ToUInt32(ByteArray(4), 0);
        public static int Int() => BitConverter.ToInt32(ByteArray(4), 0);

        public static long Long() => BitConverter.ToInt64(ByteArray(8), 0);
        public static ulong ULong() => BitConverter.ToUInt64(ByteArray(8), 0);

        public static float Float() => BitConverter.ToSingle(ByteArray(4), 0);
        public static double Double() => BitConverter.ToDouble(ByteArray(8), 0);
        public static decimal Decimal() => new decimal(Int(), Int(), Int(), Bool(), Byte(29));

        public static string String(ushort length)
        {
            StringBuilder sb = new StringBuilder(length);
            for(int i = 0; i < length; i++)
            {
                sb.Append(Char());
            }
            return sb.ToString();
        }

        public static DateTime DateTime()
        {
            long Max = System.DateTime.MaxValue.Ticks;
            long Min = System.DateTime.MinValue.Ticks;
            return new DateTime(Long(Min, Max + 1), PickOne(DateTimeKind.Unspecified, DateTimeKind.Utc, DateTimeKind.Local));
        }

        #region Ranged
        public static ulong ULong(ulong toExclusive)
        {
            if (toExclusive == 0) { throw new ArgumentException("toExclusive: Range too small."); }
            ulong toInclusive = toExclusive - 1;

            ulong filter = ulong.MaxValue;
            while (filter >> 1 > toInclusive)
            {
                filter = filter >> 1;
            }
            ulong toReturn = ULong() & filter;
            while (toReturn > toInclusive)
            {
                toReturn = ULong() & filter;
            }
            return toReturn;
        }
        //These should just work as is.
        public static long Long(long toExclusive) => (long)ULong((ulong)Math.Max(toExclusive,0));
        public static uint UInt(uint toExclusive) => (uint)ULong(toExclusive);
        public static int Int(int toExclusive) => (int)ULong((uint)Math.Max(toExclusive, 0));
        public static ushort UShort(ushort toExclusive) => (ushort)ULong(toExclusive);
        public static short Short(short toExclusive) => (short)ULong((ushort)Math.Max(toExclusive, (short)0));
        public static byte Byte(byte toExclusive) => (byte)ULong(toExclusive);
        public static sbyte SByte(sbyte toExclusive) => (sbyte)ULong((ushort)Math.Max(toExclusive, (sbyte)0));
        #endregion
        #region Arbitrary Ranged
        public static ulong ULong(ulong fromInclusive, ulong toExclusive)
        {
            if(fromInclusive < toExclusive)
            {
                return ULong(toExclusive - fromInclusive) + fromInclusive;
            }
            else
            {
                return ULong(fromInclusive - toExclusive) + toExclusive + 1;
            }
        }
        public static uint UInt(uint fromInclusive, uint toExclusive) => (uint) ULong(fromInclusive, toExclusive);
        public static ushort UShort(ushort fromInclusive, ushort toExclusive) => (ushort)ULong(fromInclusive, toExclusive);
        public static byte Byte(byte fromInclusive, byte toExclusive) => (byte)ULong(fromInclusive, toExclusive);
        public static long Long(long fromInclusive, long toExclusive)
        {
            if (fromInclusive < toExclusive)
            {
                return (long)ULong((ulong)(toExclusive - fromInclusive)) + fromInclusive;
            }
            else
            {
                return (long)ULong((ulong)(fromInclusive - toExclusive)) + toExclusive + 1;
            }
        }
        public static int Int(int fromInclusive, int toExclusive) => (int)Long(fromInclusive, toExclusive);
        public static short Short(short fromInclusive, short toExclusive) => (short)Long(fromInclusive, toExclusive);
        public static sbyte SByte(sbyte fromInclusive, sbyte toExclusive) => (sbyte)Long(fromInclusive, toExclusive);
        #endregion
    }
}
