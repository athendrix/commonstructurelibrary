using CSL.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL.ClassCreator;

namespace CommonStructureLibraryTester.Server.Controllers
{
    public record SQLTableDefinition(string Namespace, string Tablename, string[] PKNames, string[] PKTypes, string[] ColumnNames, string[] ColumnTypes, string[]? ExtraSQL = null);
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ExampleController : ControllerBase
    {
        [HttpGet]
        public string GetSQLCode(SQLTableDefinition sql)
        {
            if(sql.PKNames.Length != sql.PKTypes.Length)
            {
                throw new Exception("Each PKName must have a PKType.");
            }
            if (sql.ColumnNames.Length != sql.ColumnTypes.Length)
            {
                throw new Exception("Each ColumnName must have a ColumnType.");
            }
            List<Column> PrimaryKeys = new List<Column>();
            for(int i = 0; i < sql.PKNames.Length; i++)
            {
                PrimaryKeys.Add(new Column(Sanitize(sql.PKNames[i]), Sanitize(sql.PKTypes[i])));
            }
            List<Column> Columns = new List<Column>();
            for (int i = 0; i < sql.ColumnNames.Length; i++)
            {
                Columns.Add(new Column(Sanitize(sql.ColumnNames[i]), Sanitize(sql.ColumnTypes[i])));
            }
            if (sql.ExtraSQL != null)
            {
                for (int i = 0; i < sql.ExtraSQL.Length; i++)
                {
                    if (sql.ExtraSQL[i].Length > 250)
                    {
                        sql.ExtraSQL[i] = sql.ExtraSQL[i].Substring(0, 250);
                    }
                }
            }
            return Generator.Generate(Sanitize(sql.Namespace), Sanitize(sql.Tablename), PrimaryKeys, Columns, sql.ExtraSQL);
        }
        private const string AcceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_.?";
        private static string Sanitize(string input, int maxlength = 80) => new string(input.Where((x) => AcceptableChars.Contains(x)).Take(maxlength).ToArray());
        #region Example1
        const string Example1Namespace = "CommonStructureLibraryTester.Example";
        const string Example1Tablename = "Example";
        static readonly string[] Example1PKNames = new string[] { "ID" };
        static readonly string[] Example1PKTypes = new string[] { "UUID" };
        static readonly string[] Example1ColumnNames = Example1PKNames.Concat(Enumerable.Range(1, 30).Select((x) => "Data" + x.ToString())).ToArray();
        static readonly string[] Example1ColumnTypes = Example1PKTypes.Concat((new string[]
        { "BOOL", "BYTE", "CHAR", "SHORT", "INT", "LONG", "ULONG", "FLOAT", "DOUBLE", "DECIMAL", "STRING", "BYTEA", "UUID", "DateTime", "Enum" })
            .SelectMany((x) => new string[] { x, x + "?" })).ToArray();
        static readonly string[] Example1ExtraSQL = new string[] { "UNIQUE(\"Data25\")", "CHECK(\"Data27\" > '1990-05-20')", "FOREIGN KEY(\"Data26\") REFERENCES \"Example\"(\"ID\")" };
        [HttpGet]
        public string GetExample1() => GetSQLCode(new SQLTableDefinition(Example1Namespace, Example1Tablename, Example1PKNames, Example1PKTypes, Example1ColumnNames, Example1ColumnTypes, Example1ExtraSQL));
        #endregion
        #region Example2
        const string Example2Namespace = "ExampleNamespace.SomeSubNamespace";
        const string Example2Tablename = "Example2";
        static readonly string[] Example2PKNames = new string[] { "ID", "ID2", "ID3", "ID4" };
        static readonly string[] Example2PKTypes = new string[] { "UUID", "UUID", "UUID", "UUID" };
        static readonly string[] Example2ColumnNames = Example2PKNames.Concat(Enumerable.Range(1, 30).Select((x) => "Data" + x.ToString())).ToArray();
        static readonly string[] Example2ColumnTypes = Example2PKTypes.Concat((new string[]
        { "BOOL", "BYTE", "CHAR", "SHORT", "INT", "LONG", "ULONG", "FLOAT", "DOUBLE", "DECIMAL", "STRING", "BYTEA", "UUID", "DateTime", "Enum" })
            .SelectMany((x) => new string[] { x, x + "?" })).ToArray();
        static readonly string[] Example2ExtraSQL = new string[] { "UNIQUE(\"Data25\")", "CHECK(\"Data27\" > '1990-05-20')" };
        [HttpGet]
        public string GetExample2() => GetSQLCode(new SQLTableDefinition(Example2Namespace, Example2Tablename, Example2PKNames, Example2PKTypes, Example2ColumnNames, Example2ColumnTypes, Example2ExtraSQL));
        #endregion
    }
}
