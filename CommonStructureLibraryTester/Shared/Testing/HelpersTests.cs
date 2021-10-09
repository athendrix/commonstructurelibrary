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

namespace CommonStructureLibraryTester.Testing
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
        #endregion
    }
}
