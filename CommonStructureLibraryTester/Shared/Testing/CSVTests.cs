using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSL.Testing;
using CSL.Data;

namespace CommonStructureLibraryTester.Shared.Testing
{
    public class CSVTests : Tests
    {
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
    }
}
