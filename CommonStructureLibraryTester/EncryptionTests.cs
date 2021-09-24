using System;
using System.Collections.Generic;
using CSL.Encryption;
using System.Text.RegularExpressions;

namespace CommonStructureLibraryTester
{
    public static partial class Tests
    {
        public static bool FriendlyPasswordTest() => SyncTest(() =>
        {
            HashSet<string> testAdj = new HashSet<string>();
            testAdj.UnionWith(Passwords.Adjectives);
            if (testAdj.Count != 256)//256 Unique Values
            {
                return false;
            }
            HashSet<string> testPoke = new HashSet<string>();
            testPoke.UnionWith(Passwords.Pokemon);
            if (testPoke.Count != 256)//256 Unique Values
            {
                return false;
            }
            HashSet<string> testVerb = new HashSet<string>();
            testVerb.UnionWith(Passwords.Verbs);
            if (testVerb.Count != 256)//256 Unique Values
            {
                return false;
            }
            Regex r = new Regex("^[A-Z][a-z]+$", RegexOptions.Compiled);
            for (int i = 0; i < 256; i++)
            {
                if (!r.IsMatch(Passwords.Adjectives[i]))
                {
                    Console.WriteLine("\"" + Passwords.Adjectives[i] + "\" is not a proper password term.");
                    return false;
                }
                if (!r.IsMatch(Passwords.Pokemon[i]))
                {
                    Console.WriteLine("\"" + Passwords.Pokemon[i] + "\" is not a proper password term.");
                    return false;
                }
                if (!r.IsMatch(Passwords.Verbs[i]))
                {
                    Console.WriteLine("\"" + Passwords.Verbs[i] + "\" is not a proper password term.");
                    return false;
                }
            }
            return Passwords.FriendlyPassGen() != null &&
            Passwords.FriendlyPassPhrase40Bit() != null &&
            Passwords.FriendlyPassPhrase56Bit() != null &&
            Passwords.ThreeLetterWordPassword(5) != null;
        });
    }
}
