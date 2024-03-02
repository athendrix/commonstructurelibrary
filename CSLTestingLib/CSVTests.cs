﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSL.Testing;
using CSL.Data;
using System.Reflection;

namespace CSLTesting
{
    public class CSVTests : Tests
    {
        protected static TestResponse EscapeUnescapeTest()
        {
            MethodInfo? EscapeString = (typeof (CSV)).GetMethod("EscapeString",BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo? UnescapeString = (typeof(CSV)).GetMethod("UnescapeString", BindingFlags.NonPublic | BindingFlags.Static);
            string test;
            string? validate;
            for (int i = 0; i < 10000; i++)
            {
                test = CSL.Encryption.RandomVars.String(CSL.Encryption.RandomVars.UShort(30));
                validate = (string?)UnescapeString?.Invoke(null, new object?[] { EscapeString?.Invoke(null, new object?[] { test,',','"' }), '"' });
                if (test != validate)
                {
                    return FAIL("Escape and Unescape did not roundtrip!");
                }
            }
            return PASS();
        }
        protected static TestResponse BasicCSVReadWriteTest()
        {
            string csv =
                "Col1, Col2, Col3, Col4, Col5\r\n" +
                ",5,\" Words \",\" New\r\nLine \",\" commas, go here\"\n" +
                "Line 2 , 7 , \"Unnecessary Escaping\" , \"\n,\r\", \"\", Extra Column\n" +
                "Too,Few,Columns\n" +
                "";
            StringBuilder rebuiltcsv = new StringBuilder();
            IEnumerable<DataStore<string>> CSVData = CSV.ReadCSVFromString(csv);
            IEnumerable<string> CSVLines = CSVData.WriteCSV(new string[] { "Col1", "Col2", "Col3", "Col4", "Col5" });
            foreach(string s in CSVLines)
            {
                rebuiltcsv.Append(s + "\n");
            }
            string csv2 = "Col1,Col2,Col3,Col4,Col5\n" +
                ",5,\" Words \",\" New\r\nLine \",\" commas, go here\"\n" +
                "Line 2,7,Unnecessary Escaping,\"\n,\r\",\"\"\n" +
                "Too,Few,Columns,,\n";
            if (rebuiltcsv.ToString() != csv2)
            {
                return FAIL("CSV did not read and write correctly!");
            }
            return PASS();
        }
        [TestType(TestType.ServerSide)]
        protected static TestResponse RandomCSVWriteReadTest()
        {
            string[] headers = new string[10];
            DataStore<string>[] CSVBase;
            DataStore<string> DS;
            DataStore<string>[] CSVComp;
            for (int i = 0; i < 100; i++)
            {
                for(int k = 0; k < headers.Length; k++)
                {
                    headers[k] = CSL.Encryption.RandomVars.String(CSL.Encryption.RandomVars.UShort(30));
                }
                CSVBase = new DataStore<string>[100];
                
                for(int j = 0; j < CSVBase.Length;j++)
                {
                    DS = new DataStore<string>();
                    for(int k = 0; k < headers.Length; k++)
                    {
                        DS[headers[k]] = CSL.Encryption.RandomVars.String(CSL.Encryption.RandomVars.UShort(30));
                    }
                    CSVBase[j] = DS;
                }
                CSVComp = CSV.ReadCSVFromString(string.Join("\r\n", CSVBase.WriteCSV(headers))).ToArray();
                if(CSVComp.Length != 100)
                {
                    return FAIL("CSV did not convert properly!");
                }
                for(int j = 0; j < CSVComp.Length; j++)
                {
                    for(int k = 0; k < headers.Length; k++)
                    {
                        if(CSVBase[j][headers[k]] != CSVComp[j][headers[k]])
                        {
                            return FAIL("CSV did not convert properly!");
                        }
                    }
                }
            }
            return PASS();
        }
    }
}
