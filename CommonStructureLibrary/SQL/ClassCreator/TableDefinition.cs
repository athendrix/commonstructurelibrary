//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CSL.ClassCreation;
//using CSL.SQL.ClassCreator.TableDefinitionExtensions;

//namespace CSL.SQL.ClassCreator
//{
//    public record TableDefinition(string Namespace, string TableName, Column[] Columns, int PrimaryKeyCount, int[][] UniqueKeyMaps, string[] SQLLines)
//    {
//        public IEnumerable<Column> MapKeys(int[] KeyMap) => KeyMap.Select(y => Columns[y]);
//        public IEnumerable<Column> PrimaryKeys => Columns.Take(PrimaryKeyCount);
//        public IEnumerable<IEnumerable<Column>> UniqueKeys => UniqueKeyMaps.Select(x => MapKeys(x));
//        public IEnumerable<Column> DataColumns => Columns.Skip(PrimaryKeyCount);
//        public string GenerateCode(bool ExampleEnums)
//        {
//            CodeGenerator gen = new CodeGenerator();
//            gen.Libraries("System", "System.Collections.Generic", "System.Data", "System.Linq", "System.Threading.Tasks", "CSL.SQL");
//            gen.BlankLine();
//            gen.Namespace(Namespace);

//            gen.BeginRecord(TableName, Columns.Select(x => new ParameterDefinition(x.CSharpTypeName, x.ColumnName)), " : IDBSet");

//            gen.Region("Static Functions");
//            gen.CreateDB(this);
//            gen.GetRecords(TableName, Columns);
//            List<TableDefinitionMapping> Mappings = TableDefinitionMapping.GetMappings(PrimaryKeyCount, UniqueKeyMaps);
//            gen.Select(this, Mappings);
//            gen.Delete(this, Mappings);
//            gen.TableManagement(TableName);
//            gen.EndRegion();

//            gen.Region("Instance Functions");
//            gen.IDBSetFunctions(this);
//            gen.ToArray(Columns);
//            gen.EndRegion();

//            gen.EndRecord();
//            if (ExampleEnums && Columns.Where(x => x.type == ColumnType.Enum).Any())
//            {
//                gen.Region("Example Enums");
//                gen.Enums(Columns);
//                gen.EndRegion();
//            }
//            gen.EndNamespace();
//            return gen.ToString();
//        }
//        public static TableDefinition ParseTabledef(string tabledef)
//        {
//            string[] toParse = tabledef.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x=>!string.IsNullOrEmpty(x)).ToArray();
//            SettingSection[] parseddef = SettingsParser.Parse(toParse);
//            #region Metadata
//            Setting[]? MetadataSettings = parseddef.Where((x) => x.name.ToLower() is "metadata").FirstOrDefault()?.settings;
//            string Namespace = "<INSERT NAMESPACE HERE>";
//            string TableName = "SomeTable";
//            int PrimaryKeyCount = 0;
//            if(MetadataSettings != null)
//            {
//                foreach(Setting setting in MetadataSettings)
//                {
//                    switch(setting.key.ToLower())
//                    {
//                        case "namespace":
//                            Namespace = setting.value;
//                            break;
//                        case "name":
//                        case "tablename":
//                        case "table name":
//                            TableName = setting.value;
//                            break;
//                        case "primarykeys":
//                        case "primary keys":
//                        case "primarykeycolumns":
//                            int.TryParse(setting.value, out PrimaryKeyCount);
//                            break;
//                    }
//                }
//            }
//            #endregion
//            #region Columns
//            List<Column> Columns = new List<Column>();
//            Setting[]? ColumnSettings = parseddef.Where((x) => x.name.ToLower() is "columns").FirstOrDefault()?.settings;
//            if(ColumnSettings != null)
//            {
//                foreach(Setting setting in ColumnSettings)
//                {
//                    Columns.Add(new Column(setting.key, setting.value));
//                }
//            }
//            #endregion
//            #region UniqueKeyMaps
//            List<int[]> UniqueKeyMaps = new List<int[]>();
//            Setting[]? UniqueKeyMapSettings = parseddef.Where((x) => x.name.ToLower() is "unique keys" or "unique").FirstOrDefault()?.settings;
//            if(UniqueKeyMapSettings != null)
//            {
//                foreach(Setting setting in UniqueKeyMapSettings)
//                {
//                    UniqueKeyMaps.Add(setting.value.Split(',').Select(x => x.Trim())
//                        .SelectMany((x) =>
//                        {
//                            if(int.TryParse(x, out int i))
//                            {
//                                return new int[] { i };
//                            }
//                            Column? NamedColumn = Columns.Where((col) => col.ColumnName == x).FirstOrDefault();
//                            if(NamedColumn != null)
//                            {
//                                return new int[] { Columns.IndexOf(NamedColumn) };
//                            }
//                            return new int[] { };
//                        }).ToArray());
//                }
//            }
//            #endregion
//            #region SQLLines
//            List<string> SQLLines = new List<string>();
//            Setting[]? SQLLinesSettings = parseddef.Where((x) => x.name.ToLower() is "sql").FirstOrDefault()?.settings;
//            if (SQLLinesSettings != null)
//            {
//                foreach (Setting setting in SQLLinesSettings)
//                {
//                    SQLLines.Add(setting.value);
//                }
//            }
//            #endregion
//            return new TableDefinition(Namespace, TableName, Columns.ToArray(), PrimaryKeyCount, UniqueKeyMaps.ToArray(), SQLLines.ToArray());
//        }
//    }
//}
//namespace CSL.SQL.ClassCreator.TableDefinitionExtensions
//{
//    public static class TableDefinitionCodeGenerationExtentions
//    {
//        #region Static Functions
//        public static void CreateDB(this CodeGenerator gen, TableDefinition tabledef)
//        {
//            List<string> SQLToAdd = new List<string>();
//            foreach (IEnumerable<Column> UniquePairs in tabledef.UniqueKeys)
//            {
//                SQLToAdd.Add($"UNIQUE(\"{string.Join("\", \"", UniquePairs.Select(x => x.ColumnName))}\")");
//            }
//            SQLToAdd.AddRange(tabledef.SQLLines);
//            bool extralines = SQLToAdd.Count != 0;
//            gen.Add("public static Task<int> CreateDB(SQLDB sql) => sql.ExecuteNonQuery(");
//            gen.Indent();

//            gen.Add($@"""CREATE TABLE IF NOT EXISTS \""{tabledef.TableName}\"" ("" +");
//            for (int i = 0; i < tabledef.Columns.Length; i++)
//            {
//                gen.Add($@"""\""{tabledef.Columns[i].ColumnName}\"" {tabledef.Columns[i].SQLTypeName}, "" +");
//            }
//            gen.Add($"\"PRIMARY KEY(\\\"{string.Join("\\\", \\\"", tabledef.PrimaryKeys.Select((x) => x.ColumnName))}\\\"){(extralines ? ", " : "")}\" +");

//            for (int i = 0; i < SQLToAdd.Count; i++)
//            {
//                string SQLLine = SQLToAdd[i].Trim().TrimEnd(',');
//                string lineend = ", ";
//                if (i == SQLToAdd.Count - 1)
//                {
//                    lineend = " ";
//                }
//                gen.Add("\"" + SQLToAdd[i].Trim().Replace("\"", "\\\"") + lineend + "\" +");
//            }
//            gen.Add("\");\");");
//            gen.Unindent();
//        }
//        public static void Select(this CodeGenerator gen, TableDefinition tabledef, List<TableDefinitionMapping> Mappings)
//        {
//            gen.Region("Select");
//            gen.Add("public static async Task<AutoClosingEnumerable<" + tabledef.TableName.Replace(' ', '_') + ">> Select(SQLDB sql)");
//            gen.EnterBlock();
//            gen.Add($@"AutoClosingDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{tabledef.TableName}\"";"");");
//            gen.Add($"return new AutoClosingEnumerable<{tabledef.TableName.Replace(' ', '_')}>(GetRecords(dr),dr);");
//            gen.ExitBlock();

//            gen.Add("public static async Task<AutoClosingEnumerable<" + tabledef.TableName.Replace(' ', '_') + ">> Select(SQLDB sql, string query, params object[] parameters)");
//            gen.EnterBlock();
//            gen.Add($"AutoClosingDataReader dr = await sql.ExecuteReader(\"SELECT * FROM \\\"{tabledef.TableName}\\\" WHERE \" + query + \" ;\", parameters);");
//            gen.Add($"return new AutoClosingEnumerable<{tabledef.TableName.Replace(' ', '_')}>(GetRecords(dr),dr);");
//            gen.ExitBlock();

//            foreach(TableDefinitionMapping mapping in Mappings)
//            {
//                SelectHelper(gen, tabledef.TableName, mapping.unique, tabledef.MapKeys(mapping.map).ToList());
//            }
//            gen.EndRegion();
//        }
//        private static void SelectHelper(CodeGenerator gen, string TableName, bool unique, List<Column> columns)
//        {
//            string FnNumSuffix = string.Join("_", columns.Select((x) => x.ColumnName));
//            string FnParams = "(SQLDB sql, " + string.Join(", ", columns.Select((x) => x.CSharpTypeName + " " + x.ColumnName)) + ")";
//            string BareFnParams = string.Join(", ", columns.Select((x) => x.ConvertPrivate));
//            string[] ParameterMatching = new string[columns.Count];
//            for (int i = 0; i < ParameterMatching.Length; i++)
//            {
//                ParameterMatching[i] = $@"\""{columns[i].ColumnName}\"" = @{i}";
//            }
//            gen.Add("public static async Task<" + (unique ? "" : "AutoClosingEnumerable<") + TableName.Replace(' ', '_') + (unique ? "?" : ">") + "> SelectBy_" + FnNumSuffix + FnParams);
//            gen.EnterBlock();
//            gen.Add($@"{(unique ? "using(" : "")}AutoClosingDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"" WHERE {string.Join(" AND ", ParameterMatching)};"", {BareFnParams}){(unique ? ")":";")}");
//            gen.EnterBlock();
//            if (unique)
//            {
//                gen.Add("return GetRecords(dr).FirstOrDefault();");
//            }
//            else
//            {
//                gen.Add($"return new AutoClosingEnumerable<{TableName.Replace(' ', '_')}>(GetRecords(dr),dr);");
//            }
//            gen.ExitBlock();
//            gen.ExitBlock();
//        }
//        public static void Delete(this CodeGenerator gen, TableDefinition tabledef, List<TableDefinitionMapping> Mappings)
//        {
//            gen.Region("Delete");
//            foreach (TableDefinitionMapping mapping in Mappings)
//            {
//                DeleteHelper(gen, tabledef.TableName, tabledef.MapKeys(mapping.map).ToList());
//            }
//            gen.EndRegion();
//        }
//        private static void DeleteHelper(CodeGenerator gen, string TableName, List<Column> columns)
//        {
//            string BareFnParams = string.Join(", ", columns.Select((x) => x.ConvertPrivate));
//            string part1 = "public static Task<int> DeleteBy_" + string.Join("_", columns.Select((x) => x.ColumnName)) + "(SQLDB sql, "
//            + string.Join(", ", columns.Select((x) => x.CSharpTypeName + " " + x.ColumnName)) + ") =>";
//            string[] ParameterMatching = new string[columns.Count];
//            for (int i = 0; i < ParameterMatching.Length; i++)
//            {
//                ParameterMatching[i] = $@"\""{columns[i].ColumnName}\"" = @{i}";
//            }
//            gen.Add($@"{part1} sql.ExecuteNonQuery(""DELETE FROM \""{TableName}\"" WHERE {string.Join(" AND ", ParameterMatching)};"", " + BareFnParams + ");");
//        }
//        public static void TableManagement(this CodeGenerator gen, string TableName)
//        {
//            gen.Region("Table Management");
//            gen.Add($"public static Task Truncate(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($\"TRUNCATE \\\"{TableName}\\\"{{(cascade?\" CASCADE\":\"\")}};\");");
//            gen.Add($"public static Task Drop(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($\"DROP TABLE IF EXISTS \\\"{TableName}\\\"{{(cascade?\" CASCADE\":\"\")}};\");");
//            gen.EndRegion();
//        }

//        #endregion
//        #region Instance Functions
//        public static void ToArray(this CodeGenerator gen, Column[] Columns)
//        {
//            string ToObjectList = string.Join(", ", Columns.Select((x) => "_" + x.ColumnName));
//            gen.Add("public object?[] ToArray()");
//            gen.EnterBlock();
//            for (int i = 0; i < Columns.Length; i++)
//            {
//                string PrivCSType = Columns[i].CSharpPrivateTypeName;
//                string Name = Columns[i].ColumnName;
//                gen.Add($"{PrivCSType} _{Name} = {Columns[i].ConvertPrivate};");
//            }
//            gen.Add("return new object?[] { " + ToObjectList + " };");
//            gen.ExitBlock();
//        }
//        public static void IDBSetFunctions(this CodeGenerator gen, TableDefinition tabledef)
//        {
//            Dictionary<string, string> CN = new Dictionary<string, string>();
//            List<string> ColumnNumbers = new List<string>();
//            for (int i = 0; i < tabledef.Columns.Length; i++)
//            {
//                ColumnNumbers.Add("@" + i);
//                CN.Add(tabledef.Columns[i].ColumnName, "@" + i);
//            }
//            string SQLCols = @"\""" + string.Join(@"\"", \""", tabledef.Columns.Select((x) => x.ColumnName)) + @"\""";
//            string SQLParams = string.Join(", ", ColumnNumbers);
//            string SetData = string.Join(", ", tabledef.DataColumns.Select((x) => "\\\"" + x.ColumnName + "\\\" = " + CN[x.ColumnName]));
//            string WhereData = string.Join(" AND ", tabledef.PrimaryKeys.Select((x) => "\\\"" + x.ColumnName + "\\\" = " + CN[x.ColumnName]));
//            string ConflictKeys = string.Join(", ", tabledef.PrimaryKeys.Select((x) => "\\\"" + x.ColumnName + "\\\""));
//            string ToObjectList = string.Join(", ", tabledef.Columns.Select((x) => "_" + x.ColumnName));
//            gen.Add("public Task<int> Insert(SQLDB sql) =>");
//            gen.Indent();
//            gen.Add($@"sql.ExecuteNonQuery(""INSERT INTO \""{tabledef.TableName}\"" ({SQLCols}) "" +");
//            gen.Add($"\"VALUES({SQLParams}) \" +");
//            gen.Add($"\"ON CONFLICT ({ConflictKeys}) DO NOTHING;\", ToArray());");
//            gen.Unindent();
//            gen.Add("public Task<int> Update(SQLDB sql) =>");
//            gen.Indent();
//            gen.Add($@"sql.ExecuteNonQuery(""UPDATE \""{tabledef.TableName}\"" "" +");
//            gen.Add($"\"SET {SetData} \" +");
//            gen.Add($"\"WHERE {WhereData};\", ToArray());");
//            gen.Unindent();
//            gen.Add("public Task<int> Upsert(SQLDB sql) =>");
//            gen.Indent();
//            gen.Add($@"sql.ExecuteNonQuery(""INSERT INTO \""{tabledef.TableName}\"" ({SQLCols}) "" +");
//            gen.Add($"\"VALUES({SQLParams}) \" +");
//            gen.Add($"\"ON CONFLICT ({ConflictKeys}) DO UPDATE \" +");
//            gen.Add($"\"SET {SetData};\", ToArray());");
//            gen.Unindent();
//        }

//        #endregion
//        #region Enums
//        public static void Enums(this CodeGenerator gen, Column[] Columns)
//        {
//            foreach (Column c in Columns)
//            {
//                if (c.type == ColumnType.Enum)
//                {
//                    gen.BlankLine();
//                    gen.EnterComment();
//                    gen.Add("//", "Example Enum");
//                    gen.Add("[Flags]");
//                    gen.Add("//", "Specifying ulong allows data to be auto converted for your convenience into the database.");
//                    gen.Add("public enum " + c.CSharpTypeName.TrimEnd('?') + " : ulong");
//                    gen.EnterBlock();
//                    gen.Add("NoFlags = 0,");
//                    for (int i = 0; i < 64; i++)
//                    {
//                        gen.Add("Flag" + (i + 1).ToString() + ((i + 1) >= 10 ? "  " : "   ") + "= 1UL << " + i.ToString() + ",");
//                    }
//                    gen.ExitBlock();
//                    gen.ExitComment();
//                }
//            }
//        }
//        #endregion
//    }
//}
