using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using CSL.Helpers;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using CSL.Testing;

namespace CSLTesting
{
    public class HelpersTests : Tests
    {
        #region Other Helpers
        protected static TestResponse GenericsTest1()
        {
            int testint = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);

            if(!Generics.TryParse(Generics.ToString(testint), out int? outtest) || testint != outtest)
            {
                return FAIL(testint.ToString() + " did not roundtrip!");
            }
            if (!Generics.TryParse(Generics.ToString(int.MinValue), out int? outmin) || outmin != int.MinValue)
            {
                return FAIL(int.MinValue.ToString() + " did not roundtrip!");
            }
            if (!Generics.TryParse(Generics.ToString(int.MaxValue), out int? outmax) || outmax != int.MaxValue)
            {
                return FAIL(int.MaxValue.ToString() + " did not roundtrip!");
            }
            return PASS();
        }
        protected static TestResponse ByteArrayTests()
        {
            byte[] testdata = new byte[RandomNumberGenerator.GetInt32(0, 100)];
            RandomNumberGenerator.Fill(testdata);
            byte[] compdata1 = ByteArray.DecodeFromHexString(ByteArray.EncodeToHexString(testdata));
            if(!testdata.AsSpan().SequenceEqual(compdata1))
            {
                return FAIL("Encode and Decode HexString didn't roundtrip!");
            }
            byte[] compdata2 = ByteArray.DecodeFromHexString(ByteArray.EncodeToHexString(testdata).ToLower());
            if (!testdata.AsSpan().SequenceEqual(compdata2))
            {
                return FAIL("Encode and Decode HexString didn't roundtrip!");
            }
            byte[] compdata3 = ByteArray.DecodeFromHexString(ByteArray.EncodeToHexString(testdata).ToUpper());
            if (!testdata.AsSpan().SequenceEqual(compdata3))
            {
                return FAIL("Encode and Decode HexString didn't roundtrip!");
            }
            byte[] compdata4 = ByteArray.DecodeFromWebBase64(ByteArray.EncodeToWebBase64(testdata));
            if (!testdata.AsSpan().SequenceEqual(compdata4))
            {
                return FAIL("Encode and Decode WebBase64 didn't roundtrip!");
            }
            return PASS();
        }
        #endregion
    }
}
