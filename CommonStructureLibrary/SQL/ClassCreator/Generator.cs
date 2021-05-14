using System;
using System.Collections.Generic;
using System.Linq;

namespace CSL.SQL.ClassCreator
{
    public class Generator
    {
        public static string Generate(string NameSpace, string TableName, List<Column> PrimaryKeys, List<Column> Columns, string[] ExtraSQLLines = null)
        {
            Generator gen = new Generator();
            gen.Libraries();
            gen.BlankLine();
            gen.BeginNamespace(NameSpace);
            gen.BeginFactory(TableName, PrimaryKeys);
            gen.CreateDB(TableName, PrimaryKeys, Columns, ExtraSQLLines);
            gen.GetEnumerator(TableName, PrimaryKeys);
            gen.Select(TableName, PrimaryKeys);
            gen.Delete(TableName, PrimaryKeys);
            gen.EndFactory();
            gen.BeginRowClass(TableName, PrimaryKeys);
            gen.Properties(PrimaryKeys, Columns);
            gen.Constructors(TableName, PrimaryKeys, Columns);
            gen.IDBSetFunctions(TableName, PrimaryKeys, Columns);
            gen.EndRowClass();
            gen.EnumsAndStructs(Columns);
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
        public override string ToString()
        {
            return string.Join(Environment.NewLine, toReturn);
        }
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
            IndentAdd("public Task<int> CreateDB(SQL sql)");
            EnterBlock();
            IndentAdd("return sql.ExecuteNonQuery(");
            IndentAdd($@"""CREATE TABLE IF NOT EXISTS \""{TableName}\"" ("" +");
            for (int i = 0; i < Columns.Count; i++)
            {
                IndentAdd($@"""\""{Columns[i].ColumnName}\"" {Columns[i].SQLTypeName}, "" +");
            }
            IndentAdd($@"""PRIMARY KEY(\""{string.Join(@"\"", \""", PrimaryKeys.Select((x) => x.ColumnName))}\"")"" +");
            if (ExtraSQLLines != null)
            {
                foreach (string SQLLine in ExtraSQLLines)
                {
                    IndentAdd("\"" + SQLLine.Trim().Replace("\"", "\\\"") + "\" +");
                }
            }
            IndentAdd("\");\");");
            ExitBlock();
        }
        public void GetEnumerator(string TableName, List<Column> PrimaryKeys)
        {
            string types = string.Join(", ", PrimaryKeys.Select((x) => x.CSharpTypeName));
            IndentAdd($"IEnumerable<IDBSet<{ types }>> IDBSetFactory<{types}>.GetEnumerator(IDataReader dr) => GetEnumerator(dr);");
            IndentAdd($"public IEnumerable<{TableName}Row> GetEnumerator(IDataReader dr)");
            EnterBlock();
            IndentAdd("while(dr.Read())");
            EnterBlock();
            IndentAdd($"yield return new {TableName}Row(dr);");
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
            IndentAdd("IAsyncEnumerable<IDBSet<" + returnType + ">> IDBSetFactory<" + returnType + ">.Select(SQL sql) => Select(sql);");
            IndentAdd("public async IAsyncEnumerable<" + TableName + "Row> Select(SQL sql)");
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"";""))");
            EnterBlock();
            IndentAdd($"foreach ({TableName}Row item in GetEnumerator(dr))");
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
            string FnParams = "(SQL sql, " + string.Join(", ", CO.Select((x) => x.Item1.CSharpTypeName + " " + x.Item1.ColumnName)) + ")";
            string BareFnParams = string.Join(", ", CO.Select((x) => x.Item1.ColumnName));
            for (int i = 0; i < CO.Length; i++)
            {
                CO[i].Item2 = i;
            }
            IndentAdd((partial ? "" : "async ") + parentType + "<IDBSet<" + iReturnType + ">> IDBSetFactory<" + iReturnType + ">.SelectByPK" + FnNumSuffix + FnParams + " => " +
                (partial ? "" : "await ") + "SelectByPK" + FnNumSuffix + "(sql, " + BareFnParams + ");");
            IndentAdd("public async " + parentType + "<" + TableName + "Row> SelectByPK" + FnNumSuffix + FnParams);
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"" WHERE {string.Join(" AND ", CO.Select((x) => $@"\""{x.Item1.ColumnName}\"" = @{x.Item2}"))};"", " + BareFnParams + "))");
            EnterBlock();
            if (partial)
            {
                IndentAdd($"foreach ({TableName}Row item in GetEnumerator(dr))");
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
            IndentAdd("public Task<int> DeleteByPK" + (partial ? string.Join("", CO.Select((x) => x.Item2)) : "") + "(SQL sql, "
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
        #region Row Functions
        public void BeginRowClass(string TableName, List<Column> PrimaryKeys)
        {
            IndentAdd("public class " + TableName + "Row : IDBSet<" + string.Join(",", PrimaryKeys.Select((x) => x.CSharpTypeName)) + ">");
            EnterBlock();
        }
        public void EndRowClass() => ExitBlock();
        public void Properties(List<Column> PrimaryKeys, List<Column> Columns)
        {
            Region("Properties");
            int i;
            for (i = 0; i < PrimaryKeys.Count; i++)
            {
                string PrivCSType = PrimaryKeys[i].CSharpPrivateTypeName;
                string CSType = PrimaryKeys[i].CSharpTypeName;
                string Name = PrimaryKeys[i].ColumnName;
                string pubpre = PrimaryKeys[i].CSharpConvertPublicPrepend;
                string pubapp = PrimaryKeys[i].CSharpConvertPublicAppend;
                bool nullable = PrimaryKeys[i].nullable;
                IndentAdd("private readonly " + PrivCSType + " _" + Name + ";");
                IndentAdd("public " + CSType + " " + Name + " => " + (nullable ? "_" + Name + " == null?default:" : "") + pubpre + "_" + Name + pubapp + ";");
                IndentAdd("public " + CSType + " PK" + (PrimaryKeys.Count == 1 ? "" : (i + 1).ToString()) + " => " + Name + ";");
                BlankLine();
            }
            for (; i < Columns.Count; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                string CSType = Columns[i].CSharpTypeName;
                string Name = Columns[i].ColumnName;
                string pubpre = Columns[i].CSharpConvertPublicPrepend;
                string pubapp = Columns[i].CSharpConvertPublicAppend;
                string privpre = Columns[i].CSharpConvertPrivatePrepend;
                string privapp = Columns[i].CSharpConvertPrivateAppend;
                bool nullable = Columns[i].nullable;
                IndentAdd("private " + PrivCSType + " _" + Name + ";");
                IndentAdd("public " + CSType + " " + Name + " { get => " + (nullable ? "_" + Name + " == null?default:" : "") + pubpre + "_" + Name + pubapp + "; set => _" + Name + " = " + privpre + "value" + privapp + ";}");
                BlankLine();
            }
            EndRegion();
        }
        public void Constructors(string TableName, List<Column> PrimaryKeys, List<Column> Columns)
        {
            Region("Constructors");
            IndentAdd($"public {TableName}Row(IDataReader dr)");
            EnterBlock();
            for (int i = 0; i < Columns.Count; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                string Name = Columns[i].ColumnName;
                IndentAdd($"_{Name} = dr.IsDBNull({i.ToString()}) ? default : ({PrivCSType.TrimEnd('?')})dr[{i.ToString()}];");
            }
            ExitBlock();
            string FunctionParams = string.Join(", ", Columns.Select((x) => x.CSharpTypeName + " " + x.ColumnName));
            IndentAdd($"public {TableName}Row({FunctionParams})");
            EnterBlock();
            for (int i = 0; i < Columns.Count; i++)
            {
                string Name = Columns[i].ColumnName;
                string privpre = Columns[i].CSharpConvertPrivatePrepend;
                string privapp = Columns[i].CSharpConvertPrivateAppend;
                IndentAdd($"_{Name} = {(Columns[i].nullable ? ("(" + Name + " == null) ? default : ") : "")}{privpre}{Name}{privapp};");
            }
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
            IndentAdd("public Task<int> Insert(SQL sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""INSERT INTO \""{TableName}\"" ({SQLCols}) "" +");
            IndentAdd($"\"VALUES({SQLParams});\", ToArray());");
            ExitBlock();
            IndentAdd("public Task<int> Update(SQL sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""UPDATE \""{TableName}\"" "" +");
            IndentAdd($"\"SET {SetData} \" +");
            IndentAdd($"\"WHERE {WhereData};\", ToArray());");
            ExitBlock();
            IndentAdd("public Task<int> Upsert(SQL sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""INSERT INTO \""{TableName}\"" ({SQLCols}) "" +");
            IndentAdd($"\"VALUES({SQLParams}) \" +");
            IndentAdd($"\"ON CONFLICT ({ConflictKeys}) DO UPDATE \" +");
            IndentAdd($"\"SET {SetData};\", ToArray());");
            ExitBlock();
            IndentAdd("public object[] ToArray()");
            EnterBlock();
            IndentAdd("return new object[] { " + ToObjectList + " };");
            ExitBlock();
            //IndentAdd("public Dictionary<string,object> ToDictionary()");
            //EnterBlock();
            //IndentAdd("return new Dictionary<string, object>()");
            //EnterBlock();
            //for(int i = 0; i< Columns.Count; i++)
            //{
            //    IndentAdd("{\"@" + Columns[i].ColumnName + "\", _" + Columns[i].ColumnName + "},");
            //}
            //ExitInlineBlock();
            //ExitBlock();
            EndRegion();
        }
        #endregion
        #region Enums, Structs, and Classes
        public void EnumsAndStructs(List<Column> Columns)
        {
            foreach (Column c in Columns)
            {
                switch (c.type)
                {
                    case ColumnType.Enum:
                        IndentAdd("[Flags]");
                        IndentAdd("public enum " + c.CSharpTypeName.TrimEnd('?') + " : ulong");
                        EnterBlock();
                        IndentAdd("NoFlags = 0,");
                        for (int i = 0; i < 16; i++)
                        {
                            IndentAdd("Flag" + (i + 1).ToString() + ((i + 1) >= 10 ? "  " : "   ") + "= 1UL << " + i.ToString() + ",");
                        }
                        ExitBlock();
                        break;
                    case ColumnType.Struct:
                        IndentAdd("public struct " + c.CSharpTypeName.TrimEnd('?'));
                        EnterBlock();
                        IndentAdd("public uint Dummy;");
                        ExitBlock();
                        break;
                    case ColumnType.Class:
                        IndentAdd("public class " + c.CSharpTypeName.TrimEnd('?') + ":IBinaryWritable");
                        EnterBlock();
                        IndentAdd("public " + c.CSharpTypeName.TrimEnd('?') + "(byte[] data)");
                        EnterBlock();
                        IndentAdd("using (MemoryStream ms = new MemoryStream(data))");
                        IndentAdd("using (BinaryReader br = new BinaryReader(ms))");
                        EnterBlock();
                        IndentAdd("Dummy = br.ReadString();");
                        IndentAdd("Dummy2 = br.ReadStruct<Guid>();");
                        ExitBlock();
                        ExitBlock();
                        IndentAdd("public byte[] ToByteArray()");
                        EnterBlock();
                        IndentAdd("using (MemoryStream ms = new MemoryStream())");
                        IndentAdd("using (BinaryWriter bw = new BinaryWriter(ms))");
                        EnterBlock();
                        IndentAdd("bw.WriteMany(Dummy,Dummy2);");
                        IndentAdd("return ms.ToArray();");
                        ExitBlock();
                        ExitBlock();
                        IndentAdd("public string Dummy;");
                        IndentAdd("public Guid Dummy2;");
                        ExitBlock();
                        break;
                    default: continue;
                }
            }
        }
        #endregion
        #region Helpers
        public void IndentAdd(params string[] toAdd)
        {
            toReturn.Add(new string(' ', CurrentIndentationLevel) + string.Join("", toAdd));
        }
        public void EnterBlock()
        {
            IndentAdd("{");
            CurrentIndentationLevel += 4;
        }
        public void ExitBlock()
        {
            CurrentIndentationLevel -= 4;
            IndentAdd("}");
        }
        public void ExitInlineBlock()
        {
            CurrentIndentationLevel -= 4;
            IndentAdd("};");
        }
        #endregion
    }
}
