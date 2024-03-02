using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

using CSL;
using CSL.Data;
using CSL.Testing;
using System.Linq;

namespace CSLTesting
{
    internal class SettingsParserTests : Tests
    {
        protected static TestResponse BasicReadWriteTest()
        {
            string[] lines = new string[]
            {
                "key=value",
                "key2=\"  value  \"",
                "key3=C:\\Users\\SomeUser",
                "key4=\"C:\\\\Users\\\\\\\"MyUser\\\"\"",
                "#This is a comment and will not be read",
                "[New Section]",
                "key=alternateValue",
                "key4=\"\\t\\tHello World!\""
            };
            DataStore<string> Settings = SettingsParser.Parse(lines,false);
            if (Settings["key"] is not "value" ||
                Settings["key2"] is not "  value  " ||
                Settings["key3"] is not "C:\\Users\\SomeUser" ||
                Settings["key4"] is not "C:\\Users\\\"MyUser\"" ||
                Settings["New Section.key"] is not "alternateValue" ||
                Settings["New Section.key4"] is not "\t\tHello World!")
            {
                return FAIL("Did not parse correctly!");
            }
            Settings["New Section.Subsection.key5"] = "   \"Final Test\"   ";
            string[] outlines = Settings.ToLines().ToArray();
            if (outlines[0] != lines[0] ||
                outlines[1] != lines[1] ||
                outlines[2] != lines[2] ||
                outlines[3] != lines[3] ||
                outlines[4] != lines[5] ||
                outlines[5] != lines[6] ||
                outlines[6] != lines[7] ||
                outlines[7] != "[[Subsection]]" ||
                outlines[8] != "key5=\"   \\\"Final Test\\\"   \"")
            {
                return FAIL("Did not roundtrip correctly!");
            }
            return PASS();
        }
    }
}
