using CSL.Data;
using CSL.Encryption;
using CSL.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSLTesting
{
    public class DataStoreTests : Tests
    {
        protected static TestResponse DataStoreStringTests()
        {
            DataStore<string> ds = new DataStore<string>(true);
            ds.Add("zero", "0");
            ds.Add("one", "1");
            ds.Add("two", "2");
            ds.Add("three", "3");
            ds.Add("four", "4");

            if(ds.Count != 5) {return FAIL("Wrong count");}
            for(int i = 0; i < ds.Count; i++)
            {
                if(ds[i] != i.ToString())
                {
                    return FAIL($"Wrong Ordinal! Expected {i} but got {ds[i]}");
                }
            }
            if (ds["zero"] != "0") { return FAIL("Wrong value for zero!"); }
            if (ds["one"] != "1") { return FAIL("Wrong value for one!"); }
            if (ds["two"] != "2") { return FAIL("Wrong value for two!"); }
            if (ds["three"] != "3") { return FAIL("Wrong value for three!"); }
            if (ds["four"] != "4") { return FAIL("Wrong value for four!"); }
            if(ds.GetInt("zero") != 0) { return FAIL("GetInt failed to convert!"); }
            if (ds.Get<double>("Zero") != 0.0) { return FAIL("Get<double> failed to convert!"); }
            ds[0] = "AAAA";
            byte[]? test = ds.GetByteArray("zero");
            if(test == null || test.Length != 3 || test[0] != 0 || test[1] != 0 || test[2] != 0) { return FAIL("Didn't handle byte array properly."); }
            return PASS();
        }
        protected static TestResponse SetTest()
        {
            DataStore<int?> ds = new DataStore<int?>(true);
            ds["foo"] = 0;
            ds["bar"] = 1;
            ds["baz"] = 2;
            int? test = ds["far"];
            if(test != null) { return FAIL("Should be null;"); }

            int far = ds["far"] ??= 5;
            if(far != 5 || ds["far"] != 5)
            {
                return FAIL("Failed to passthrough compare.");
            }
            
            return PASS();
        }
    }
}
