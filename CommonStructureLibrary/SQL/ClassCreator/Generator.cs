using System;
using System.Collections.Generic;
using System.Linq;

namespace CSL.SQL.ClassCreator
{
    public class Generator
    {
        public static string Generate(string NameSpace, string TableName, List<Column> PrimaryKeys, List<Column> Columns, string[]? ExtraSQLLines = null)
        {
            Generator gen = new Generator();
            gen.Libraries();
            gen.BlankLine();
            gen.BeginNamespace(NameSpace);
            gen.BeginFactory(TableName, PrimaryKeys);
            gen.CreateDB(TableName, PrimaryKeys, Columns, ExtraSQLLines?? new string[0]);
            gen.GetEnumerator(TableName, PrimaryKeys);
            gen.Select(TableName, PrimaryKeys);
            gen.Delete(TableName, PrimaryKeys);
            gen.EndFactory();
            gen.BeginRecord(TableName, PrimaryKeys, Columns);
            gen.RecordPKs(PrimaryKeys);
            gen.SQLConverters(TableName, Columns);
            gen.IDBSetFunctions(TableName, PrimaryKeys, Columns);
            gen.EndRecord();
            gen.Enums(Columns);
            gen.EndNamespace();
            return gen.ToString();
        }
        private int CurrentIndentationLevel = 0;
        List<string> toReturn = new List<string>();
        #region Core
        public void Reset()
        {
            toReturn = new List<string>();
            CurrentIndentationLevel = 0;
        }
        public void BlankLine() => IndentAdd();
        public void Libraries()
        {
            IndentAdd("using System;");
            IndentAdd("using System.IO;");
            IndentAdd("using System.Collections.Generic;");
            IndentAdd("using System.Data;");
            IndentAdd("using System.Linq;");
            IndentAdd("using System.Runtime.InteropServices;");
            IndentAdd("using System.Threading.Tasks;");
            IndentAdd("using CSL.SQL;");
        }
        public void BeginNamespace(string namespacename)
        {
            IndentAdd("namespace " + namespacename);
            EnterBlock();
        }
        public void EndNamespace() => ExitBlock();
        public void Region(string RegionName) => IndentAdd("#region " + RegionName);
        public void EndRegion() => IndentAdd("#endregion");
        public override string ToString() => string.Join(Environment.NewLine, toReturn);
        #endregion
        #region Factory Functions
        public void BeginFactory(string TableName, List<Column> PrimaryKeys)
        {
            IndentAdd("public class " + TableName + "Factory : IDBSetFactory<" + string.Join(",", PrimaryKeys.Select((x) => x.CSharpTypeName)) + ">");
            EnterBlock();
        }
        public void EndFactory() => ExitBlock();
        public void CreateDB(string TableName, List<Column> PrimaryKeys, List<Column> Columns, string[] ExtraSQLLines)
        {
            bool extralines = ExtraSQLLines.Length != 0;
            IndentAdd("public Task<int> CreateDB(SQLDB sql)");
            EnterBlock();
            IndentAdd("return sql.ExecuteNonQuery(");
            IndentAdd($@"""CREATE TABLE IF NOT EXISTS \""{TableName}\"" ("" +");
            for (int i = 0; i < Columns.Count; i++)
            {
                IndentAdd($@"""\""{Columns[i].ColumnName}\"" {Columns[i].SQLTypeName}, "" +");
            }
            IndentAdd($"\"PRIMARY KEY(\\\"{string.Join("\\\", \\\"", PrimaryKeys.Select((x) => x.ColumnName))}\\\"){(extralines ? ", " : "")}\" +");

            for (int i = 0; i < ExtraSQLLines.Length; i++)
            {
                string SQLLine = ExtraSQLLines[i].Trim().TrimEnd(',');
                string lineend = ", ";
                if (i == ExtraSQLLines.Length - 1)
                {
                    lineend = " ";
                }
                IndentAdd("\"" + ExtraSQLLines[i].Trim().Replace("\"", "\\\"") + lineend + "\" +");
            }
            IndentAdd("\");\");");
            ExitBlock();
        }
        public void GetEnumerator(string TableName, List<Column> PrimaryKeys)
        {
            string types = string.Join(", ", PrimaryKeys.Select((x) => x.CSharpTypeName));
            IndentAdd($"IEnumerable<IDBSet<{ types }>> IDBSetFactory<{types}>.GetEnumerator(IDataReader dr) => GetEnumerator(dr);");
            IndentAdd($"public IEnumerable<{TableName}Record> GetEnumerator(IDataReader dr)");
            EnterBlock();
            IndentAdd("while(dr.Read())");
            EnterBlock();
            IndentAdd($"yield return {TableName}Record.FromDataReader(dr);");
            ExitBlock();
            IndentAdd("yield break;");
            ExitBlock();
        }
        public void Select(string TableName, List<Column> PrimaryKeys)
        {
            string returnType = string.Join(",", PrimaryKeys.Select((x) => x.CSharpTypeName));
            ValueTuple<Column, int>[] CO = new ValueTuple<Column, int>[PrimaryKeys.Count];
            for (int i = 0; i < CO.Length; i++)
            {
                CO[i] = new ValueTuple<Column, int>(PrimaryKeys[i], i + 1);
            }

            Region("Select");
            IndentAdd("IAsyncEnumerable<IDBSet<" + returnType + ">> IDBSetFactory<" + returnType + ">.Select(SQLDB sql) => Select(sql);");
            IndentAdd("public async IAsyncEnumerable<" + TableName + "Record> Select(SQLDB sql)");
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"";""))");
            EnterBlock();
            IndentAdd($"foreach ({TableName}Record item in GetEnumerator(dr))");
            EnterBlock();
            IndentAdd("yield return item;");
            ExitBlock();
            ExitBlock();
            ExitBlock();

            IndentAdd("IAsyncEnumerable<IDBSet<" + returnType + ">> IDBSetFactory<" + returnType + ">.Select(SQLDB sql, string query, params object[] parameters) => Select(sql, query, parameters);");
            IndentAdd("public async IAsyncEnumerable<" + TableName + "Record> Select(SQLDB sql, string query, params object[] parameters)");
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(query, parameters))");
            EnterBlock();
            IndentAdd($"foreach ({TableName}Record item in GetEnumerator(dr))");
            EnterBlock();
            IndentAdd("yield return item;");
            ExitBlock();
            ExitBlock();
            ExitBlock();

            switch (CO.Length)
            {
                case 1:
                    SelectHelper(TableName, returnType, false, CO[0]);
                    break;
                case 2:
                    SelectHelper(TableName, returnType, true, CO[0]);
                    SelectHelper(TableName, returnType, true, CO[1]);
                    SelectHelper(TableName, returnType, false, CO[0], CO[1]);
                    break;
                case 3:
                    SelectHelper(TableName, returnType, true, CO[0]);
                    SelectHelper(TableName, returnType, true, CO[1]);
                    SelectHelper(TableName, returnType, true, CO[2]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[1]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[2]);
                    SelectHelper(TableName, returnType, true, CO[1], CO[2]);
                    SelectHelper(TableName, returnType, false, CO[0], CO[1], CO[2]);
                    break;
                case 4:
                    SelectHelper(TableName, returnType, true, CO[0]);
                    SelectHelper(TableName, returnType, true, CO[1]);
                    SelectHelper(TableName, returnType, true, CO[2]);
                    SelectHelper(TableName, returnType, true, CO[3]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[1]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[2]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[3]);
                    SelectHelper(TableName, returnType, true, CO[1], CO[2]);
                    SelectHelper(TableName, returnType, true, CO[1], CO[3]);
                    SelectHelper(TableName, returnType, true, CO[2], CO[3]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[1], CO[2]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[1], CO[3]);
                    SelectHelper(TableName, returnType, true, CO[0], CO[2], CO[3]);
                    SelectHelper(TableName, returnType, true, CO[1], CO[2], CO[3]);
                    SelectHelper(TableName, returnType, false, CO[0], CO[1], CO[2], CO[3]);
                    break;
            }
            EndRegion();
        }
        private void SelectHelper(string TableName, string iReturnType, bool partial, params ValueTuple<Column, int>[] CO)
        {
            string parentType = (partial ? "IAsyncEnumerable" : "Task");
            string FnNumSuffix = (partial ? string.Join("", CO.Select((x) => x.Item2)) : "");
            string FnParams = "(SQLDB sql, " + string.Join(", ", CO.Select((x) => x.Item1.CSharpTypeName + " " + x.Item1.ColumnName)) + ")";
            string BareFnParams = string.Join(", ", CO.Select((x) => x.Item1.ColumnName));
            for (int i = 0; i < CO.Length; i++)
            {
                CO[i].Item2 = i;
            }
            IndentAdd((partial ? "" : "async ") + parentType + "<IDBSet<" + iReturnType + ">" + (partial ? "" : "?") + "> IDBSetFactory<" + iReturnType + ">.SelectByPK" + FnNumSuffix + FnParams + " => " +
                (partial ? "" : "await ") + "SelectByPK" + FnNumSuffix + "(sql, " + BareFnParams + ");");
            IndentAdd("public async " + parentType + "<" + TableName + "Record" + (partial ? "" : "?") + "> SelectByPK" + FnNumSuffix + FnParams);
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"" WHERE {string.Join(" AND ", CO.Select((x) => $@"\""{x.Item1.ColumnName}\"" = @{x.Item2}"))};"", " + BareFnParams + "))");
            EnterBlock();
            if (partial)
            {
                IndentAdd($"foreach ({TableName}Record item in GetEnumerator(dr))");
                EnterBlock();
                IndentAdd("yield return item;");
                ExitBlock();
            }
            else
            {
                IndentAdd("return GetEnumerator(dr).FirstOrDefault();");
            }
            ExitBlock();
            ExitBlock();
        }
        public void Delete(string TableName, List<Column> PrimaryKeys)
        {
            Region("Delete");
            ValueTuple<Column, int>[] CO = new ValueTuple<Column, int>[PrimaryKeys.Count];
            for (int i = 0; i < CO.Length; i++)
            {
                CO[i] = new ValueTuple<Column, int>(PrimaryKeys[i], i + 1);
            }
            switch (CO.Length)
            {
                case 1:
                    DeleteHelper(TableName, false, CO[0]);
                    break;
                case 2:
                    DeleteHelper(TableName, true, CO[0]);
                    DeleteHelper(TableName, true, CO[1]);
                    DeleteHelper(TableName, false, CO[0], CO[1]);
                    break;
                case 3:
                    DeleteHelper(TableName, true, CO[0]);
                    DeleteHelper(TableName, true, CO[1]);
                    DeleteHelper(TableName, true, CO[2]);
                    DeleteHelper(TableName, true, CO[0], CO[1]);
                    DeleteHelper(TableName, true, CO[0], CO[2]);
                    DeleteHelper(TableName, true, CO[1], CO[2]);
                    DeleteHelper(TableName, false, CO[0], CO[1], CO[2]);
                    break;
                case 4:
                    DeleteHelper(TableName, true, CO[0]);
                    DeleteHelper(TableName, true, CO[1]);
                    DeleteHelper(TableName, true, CO[2]);
                    DeleteHelper(TableName, true, CO[3]);
                    DeleteHelper(TableName, true, CO[0], CO[1]);
                    DeleteHelper(TableName, true, CO[0], CO[2]);
                    DeleteHelper(TableName, true, CO[0], CO[3]);
                    DeleteHelper(TableName, true, CO[1], CO[2]);
                    DeleteHelper(TableName, true, CO[1], CO[3]);
                    DeleteHelper(TableName, true, CO[2], CO[3]);
                    DeleteHelper(TableName, true, CO[0], CO[1], CO[2]);
                    DeleteHelper(TableName, true, CO[0], CO[1], CO[3]);
                    DeleteHelper(TableName, true, CO[0], CO[2], CO[3]);
                    DeleteHelper(TableName, true, CO[1], CO[2], CO[3]);
                    DeleteHelper(TableName, false, CO[0], CO[1], CO[2], CO[3]);
                    break;
            }
            EndRegion();
        }
        private void DeleteHelper(string TableName, bool partial, params ValueTuple<Column, int>[] CO)
        {
            string BareFnParams = string.Join(", ", CO.Select((x) => x.Item1.ColumnName));
            IndentAdd("public Task<int> DeleteByPK" + (partial ? string.Join("", CO.Select((x) => x.Item2)) : "") + "(SQLDB sql, "
            + string.Join(", ", CO.Select((x) => x.Item1.CSharpTypeName + " " + x.Item1.ColumnName)) + ")");
            for (int i = 0; i < CO.Length; i++)
            {
                CO[i].Item2 = i;
            }
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""DELETE FROM \""{TableName}\"" WHERE {string.Join(" AND ", CO.Select((x) => $@"\""{x.Item1.ColumnName}\"" = @{x.Item2}"))};"", " + BareFnParams + ");");
            ExitBlock();
        }
        #endregion
        #region Record Functions
        public void BeginRecord(string TableName, List<Column> PrimaryKeys, List<Column> Columns)
        {
            string TypeColumns = string.Join(", ", Columns.Select((x) => x.CSharpTypeName + " " + x.ColumnName));
            string KeyTypes = string.Join(", ", PrimaryKeys.Select((x) => x.CSharpTypeName));
            IndentAdd("public record " + TableName + "Record(" + TypeColumns + ") : IDBSet<" + KeyTypes + " > ");
            EnterBlock();
        }
        public void RecordPKs(List<Column> PrimaryKeys)
        {
            Region("Primary Keys");
            int i;
            for (i = 0; i < PrimaryKeys.Count; i++)
            {
                string CSType = PrimaryKeys[i].CSharpTypeName;
                string Name = PrimaryKeys[i].ColumnName;
                IndentAdd("public " + CSType + " PK" + (PrimaryKeys.Count == 1 ? "" : (i + 1).ToString()) + " => " + Name + ";");
                BlankLine();
            }
            EndRegion();
        }
        public void SQLConverters(string TableName, List<Column> Columns)
        {
            Region("SQLConverters");
            IndentAdd($"public static {TableName}Record FromDataReader(IDataReader dr)");
            EnterBlock();
            for (int i = 0; i < Columns.Count; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                string CSType = Columns[i].CSharpTypeName;
                string Name = Columns[i].ColumnName;
                string pubpre = Columns[i].CSharpConvertPublicPrepend;
                string pubapp = Columns[i].CSharpConvertPublicAppend;
                bool nullable = Columns[i].nullable;
                if (CSType != PrivCSType)
                {
                    IndentAdd($"{PrivCSType} _{Name} = {(nullable ? "dr.IsDBNull("+i+") ? null :":"")} ({PrivCSType.TrimEnd('?')})dr[{i}];");
                    IndentAdd($"{CSType} {Name} = {(nullable ? "_" + Name + " == null ? null :" : "")}{pubpre}_{Name}{pubapp};");
                }
                else
                {
                    IndentAdd($"{CSType} {Name} = {(nullable ? "dr.IsDBNull(" + i + ") ? null :" : "")} ({PrivCSType.TrimEnd('?')})dr[{i}];");
                }
            }
            IndentAdd($"return new {TableName}Record({string.Join(", ",Columns.Select((x)=>x.ColumnName))});");
            ExitBlock();
            string ToObjectList = string.Join(", ", Columns.Select((x) => "_" + x.ColumnName));
            IndentAdd("public object[] ToArray()");
            EnterBlock();
            for (int i = 0; i < Columns.Count; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                bool cast = Columns[i].CSharpTypeName == PrivCSType;
                string Name = Columns[i].ColumnName;
                string privpre = Columns[i].CSharpConvertPrivatePrepend;
                string privapp = Columns[i].CSharpConvertPrivateAppend;
                bool nullable = Columns[i].nullable;
                IndentAdd($"{PrivCSType} _{Name} = {(nullable ? Name + " == null?default:" : "")}{privpre}{Name}{privapp};");
            }
            IndentAdd("return new object[] { " + ToObjectList + " };");
            ExitBlock();
            EndRegion();
        }
        public void IDBSetFunctions(string TableName, List<Column> PrimaryKeys, List<Column> Columns)
        {
            Region("IDBSetFunctions");
            List<Column> DataColumns = new List<Column>();
            for (int i = PrimaryKeys.Count; i < Columns.Count; i++)
            {
                DataColumns.Add(Columns[i]);
            }
            Dictionary<string, string> CN = new Dictionary<string, string>();
            List<string> ColumnNumbers = new List<string>();
            for (int i = 0; i < Columns.Count; i++)
            {
                ColumnNumbers.Add("@" + i);
                CN.Add(Columns[i].ColumnName, "@" + i);
            }
            string SQLCols = @"\""" + string.Join(@"\"", \""", Columns.Select((x) => x.ColumnName)) + @"\""";
            string SQLParams = string.Join(", ", ColumnNumbers);
            string SetData = string.Join(", ", DataColumns.Select((x) => "\\\"" + x.ColumnName + "\\\" = " + CN[x.ColumnName]));
            string WhereData = string.Join(" AND ", PrimaryKeys.Select((x) => "\\\"" + x.ColumnName + "\\\" = " + CN[x.ColumnName]));
            string ConflictKeys = string.Join(", ", PrimaryKeys.Select((x) => "\\\"" + x.ColumnName + "\\\""));
            string ToObjectList = string.Join(", ", Columns.Select((x) => "_" + x.ColumnName));
            IndentAdd("public Task<int> Insert(SQLDB sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""INSERT INTO \""{TableName}\"" ({SQLCols}) "" +");
            IndentAdd($"\"VALUES({SQLParams});\", ToArray());");
            ExitBlock();
            IndentAdd("public Task<int> Update(SQLDB sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""UPDATE \""{TableName}\"" "" +");
            IndentAdd($"\"SET {SetData} \" +");
            IndentAdd($"\"WHERE {WhereData};\", ToArray());");
            ExitBlock();
            IndentAdd("public Task<int> Upsert(SQLDB sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""INSERT INTO \""{TableName}\"" ({SQLCols}) "" +");
            IndentAdd($"\"VALUES({SQLParams}) \" +");
            IndentAdd($"\"ON CONFLICT ({ConflictKeys}) DO UPDATE \" +");
            IndentAdd($"\"SET {SetData};\", ToArray());");
            ExitBlock();
            EndRegion();
        }
        public void EndRecord() => ExitBlock();
        #endregion
        #region Enums
        public void Enums(List<Column> Columns)
        {
            foreach (Column c in Columns)
            {
                if (c.type == ColumnType.Enum)
                {
                    BlankLine();
                    IndentAdd("////", "Example Enum");
                    IndentAdd("//", "[Flags]");
                    IndentAdd("////", "Specifying ulong allows data to be auto converted for your convenience into the database.");
                    IndentAdd("//", "public enum " + c.CSharpTypeName.TrimEnd('?') + " : ulong");
                    EnterBlock(commented: true);
                    IndentAdd("//", "NoFlags = 0,");
                    for (int i = 0; i < 16; i++)
                    {
                        IndentAdd("//", "Flag" + (i + 1).ToString() + ((i + 1) >= 10 ? "  " : "   ") + "= 1UL << " + i.ToString() + ",");
                    }
                    ExitBlock(commented: true);
                }
            }
        }
        #endregion
        #region Helpers
        public void IndentAdd(params string[] toAdd) => toReturn.Add(new string(' ', CurrentIndentationLevel) + string.Join("", toAdd));
        public void EnterBlock(bool commented = false)
        {
            IndentAdd(commented ? "//" : "" , "{");
            CurrentIndentationLevel += 4;
        }
        public void ExitBlock(bool commented = false)
        {
            CurrentIndentationLevel -= 4;
            IndentAdd(commented ? "//" : "" , "}");
        }
        public void ExitInlineBlock(bool commented = false)
        {
            CurrentIndentationLevel -= 4;
            IndentAdd(commented ? "//" : "" , "};");
        }
        #endregion
    }
}
