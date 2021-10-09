using CSL.Encryption;
using CSL.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonStructureLibraryTester.Testing
{
    public class EncryptionTests : Tests
    {
        protected static TestResponse FriendlyPasswordTest()
        {
            HashSet<string> testAdj = new HashSet<string>();
            testAdj.UnionWith(Passwords.Adjectives);
            if (testAdj.Count != 256)//256 Unique Values
            {
                return FAIL("Test Adjective Count is " + testAdj.Count);
            }
            HashSet<string> testPoke = new HashSet<string>();
            testPoke.UnionWith(Passwords.Pokemon);
            if (testPoke.Count != 256)//256 Unique Values
            {
                return FAIL("Test Pokemon Count is " + testPoke.Count);
            }
            HashSet<string> testVerb = new HashSet<string>();
            testVerb.UnionWith(Passwords.Verbs);
            if (testVerb.Count != 256)//256 Unique Values
            {
                return FAIL("Test Verb Count is " + testVerb.Count);
            }
            Regex r = new Regex("^[A-Z][a-z]+$", RegexOptions.Compiled);
            for (int i = 0; i < 256; i++)
            {
                if (!r.IsMatch(Passwords.Adjectives[i]))
                {
                    return FAIL("\"" + Passwords.Adjectives[i] + "\" is not a proper password term.");
                }
                if (!r.IsMatch(Passwords.Pokemon[i]))
                {
                    return FAIL("\"" + Passwords.Pokemon[i] + "\" is not a proper password term.");
                }
                if (!r.IsMatch(Passwords.Verbs[i]))
                {
                    return FAIL("\"" + Passwords.Verbs[i] + "\" is not a proper password term.");
                }
            }
            if((Passwords.FriendlyPassGen() ?? Passwords.FriendlyPassPhrase40Bit() ?? Passwords.FriendlyPassPhrase56Bit() ?? Passwords.ThreeLetterWordPassword(5)) != null)
            {
                return PASS();
            }
            return FAIL("Generated password was null!");
        }
    }
}
