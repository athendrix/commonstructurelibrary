using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using CSL.Data;
using CSL.Encryption;
using CSL.Helpers;
using CSL.SQL;
using CSL.Webserver;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace CommonStructureLibraryTester
{
    public static partial class Tests
    {
        #region Other Helpers
        public static bool GenericsTest1() => SyncTest(() =>
        {
            bool toReturn = true;
            int testint = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            toReturn &= Generics.TryParse(Generics.ToString(testint), out int outtest) && testint == outtest;
            toReturn &= Generics.TryParse(Generics.ToString(int.MinValue), out int outmin) && outmin == int.MinValue;
            toReturn &= Generics.TryParse(Generics.ToString(int.MaxValue), out int outmax) && outmax == int.MaxValue;
            return toReturn;
        });
        #endregion
    }
}
