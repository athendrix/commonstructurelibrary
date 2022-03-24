using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSL.ClassCreation;
using CSL.SQL.ClassCreator.FunctionDefinitionExtensions;

namespace CSL.SQL.ClassCreator
{
    public record FunctionDefinition(string Namespace, string FunctionName, Column[] Input, Column[] Output, bool SingleRow, string[] SQLFunctionCode, string Language, Volitility Volitility)
    {
        public string GenerateCode()
        {
            CodeGenerator gen = new CodeGenerator();
            gen.Libraries("System", "System.Collections.Generic", "System.Data", "System.Linq", "System.Threading.Tasks", "CSL.SQL");
            gen.BlankLine();
            gen.Namespace(Namespace);
            gen.BeginRecord(FunctionName, Output.Select(x => new ParameterDefinition(x.CSharpTypeName, x.ColumnName)));
            gen.Region("Static Functions");
            gen.CreateFunction(this);
            gen.DropFunction(FunctionName);
            gen.CallFunction(this);
            gen.GetRecords(FunctionName, Output);
            gen.EndRegion();
            gen.EndRecord();
            gen.EndNamespace();
            return gen.ToString();
        }

        public static FunctionDefinition ParseFunctiondef(string funcdef)
        {
            string[] toParse = funcdef.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            SettingSection[] parseddef = SettingsParser.Parse(toParse);
            #region Metadata
            Setting[]? MetadataSettings = parseddef.Where((x) => x.name.ToLower() is "metadata").FirstOrDefault()?.settings;
            string Namespace = "<INSERT NAMESPACE HERE>";
            string FunctionName = "SomeFunction";
            bool SingleRow = false;
            string Language = "SQL";
            Volitility Volitility = Volitility.Volatile;
            if (MetadataSettings != null)
            {
                foreach (Setting setting in MetadataSettings)
                {
                    switch (setting.key.ToLower())
                    {
                        case "namespace":
                            Namespace = setting.value;
                            break;
                        case "name":
                        case "functionname":
                        case "function name":
                            FunctionName = setting.value;
                            break;
                        case "language":
                            Language = setting.value;
                            break;
                        case "volatility":
                            switch(setting.value.ToLower())
                            {
                                case "immutable":
                                    Volitility = Volitility.Immutable;
                                    break;
                                case "volatile":
                                    Volitility = Volitility.Volatile;
                                    break;
                                case "stable":
                                    Volitility = Volitility.Stable;
                                    break;
                                default:
                                    Console.Error.WriteLine("Warning:" + setting.value + " is not a proper Volatility setting. Options are Immutable, Volatile, and Stable. Defaulting to \"Volatile\"");
                                    break;
                            }
                            break;
                        case "singlerow":
                        case "single row":
                            if(!bool.TryParse(setting.value, out SingleRow))
                            {
                                Console.Error.WriteLine("Warning:" + setting.value + " is not a boolean for the Single Row setting. Options are True and False. Defaulting to \"False\"");
                            }
                            break;
                    }
                }
            }
            #endregion
            #region Columns
            #region Input
            List<Column> Input = new List<Column>();
            Setting[]? InputSettings = parseddef.Where((x) => x.name.ToLower() is "input" or "inputcolumns" or "input columns").FirstOrDefault()?.settings;
            if (InputSettings != null)
            {
                foreach (Setting setting in InputSettings)
                {
                    Input.Add(new Column(setting.key, setting.value));
                }
            }
            #endregion
            #region Output
            List<Column> Output = new List<Column>();
            Setting[]? OutputSettings = parseddef.Where((x) => x.name.ToLower() is "output" or "outputcolumns" or "output columns").FirstOrDefault()?.settings;
            if (OutputSettings != null)
            {
                foreach (Setting setting in OutputSettings)
                {
                    Output.Add(new Column(setting.key, setting.value));
                }
            }
            #endregion
            #endregion
            #region SQLLines
            List<string> SQLLines = new List<string>();
            Setting[]? SQLLinesSettings = parseddef.Where((x) => x.name.ToLower() is "sql").FirstOrDefault()?.settings;
            if (SQLLinesSettings != null)
            {
                foreach (Setting setting in SQLLinesSettings)
                {
                    SQLLines.Add(setting.value);
                }
            }
            #endregion
            return new FunctionDefinition(Namespace, FunctionName, Input.ToArray(), Output.ToArray(), SingleRow, SQLLines.ToArray(), Language, Volitility);
        }
    }
    public enum Volitility
    {
        Immutable,
        Stable,
        Volatile
    }
}

namespace CSL.SQL.ClassCreator.FunctionDefinitionExtensions
{
    public static class FunctionDefinitionCodeGenerationExtentions
    {
        public static void CreateFunction(this CodeGenerator gen, FunctionDefinition funcdef)
        {
            gen.Add("public static Task<int> CreateFunction(SQLDB sql) => sql.ExecuteNonQuery(");
            gen.Indent();
            gen.AddQuoted($"CREATE OR REPLACE FUNCTION \"{funcdef.FunctionName}\" ( ");
            for (int i = 0; i < funcdef.Input.Length; i++)
            {
                gen.AddQuoted($"\"{funcdef.Input[i].ColumnName}\" {funcdef.Input[i].SQLTypePlain}{(i == funcdef.Input.Length - 1?"":",")} ");
            }
            gen.AddQuoted(") ");
            switch(funcdef.Output.Length)
            {
                case 0:
                    gen.AddQuoted("RETURNS void ");
                    break;
                case 1:
                    if (funcdef.SingleRow)
                    {
                        gen.AddQuoted($"RETURNS {funcdef.Output[0].SQLTypePlain} ");
                    }
                    else
                    {
                        gen.AddQuoted($"RETURNS TABLE(\"{funcdef.Output[0].ColumnName}\" {funcdef.Output[0].SQLTypePlain}) ");
                    }
                    break;
                default:
                    gen.AddQuoted($"RETURNS TABLE({string.Join(", ", funcdef.Output.Select(x => "\"" + x.ColumnName + "\" " + x.SQLTypePlain))}) ");
                    break;
            }
            gen.AddQuoted("AS ");
            gen.AddQuoted("$$ ");
            foreach(string s in funcdef.SQLFunctionCode)
            {
                gen.AddQuoted(s.Replace("$$","") + " ");
            }
            gen.AddQuoted("$$ ");
            gen.AddQuoted($"Language {funcdef.Language} ");
            gen.AddQuoted(funcdef.Volitility.ToString().ToUpper() + ";",");");
            gen.Unindent();
        }
        public static void CallFunction(this CodeGenerator gen, FunctionDefinition funcdef)
        {
            string funcparams = string.Join("", funcdef.Input.Select(x => ", " + x.CSharpTypeName + " " + x.ColumnName));
            string numparams = string.Join(", ", Enumerable.Range(0, funcdef.Input.Length).Select(x => "@" + x));
            string standalonefuncparams = string.Join("", funcdef.Input.Select(x => ", " + x.ColumnName));
            switch (funcdef.Output.Length)
            {
                case 0:
                    gen.Add($"public static Task CallFunction(SQLDB sql{funcparams}) => ");
                    gen.Indent();
                    gen.Add($"sql.ExecuteNonQuery(\"\\\"{funcdef.FunctionName}\\\"({numparams});\"{standalonefuncparams});");
                    gen.Unindent();
                    break;
                case 1:
                    if (funcdef.SingleRow)
                    {
                        bool hasNull = funcdef.Output[0].CSharpTypeName.EndsWith("?");
                        gen.Add($"public static async Task<{funcdef.Output[0].CSharpTypeName}{(hasNull?"":"?")}> CallFunction(SQLDB sql{funcparams})");
                        gen.EnterBlock();
                        gen.Add($"using(AutoClosingDataReader dr = await sql.ExecuteReader(\"SELECT * FROM \\\"{funcdef.FunctionName}\\\"({numparams});\"{standalonefuncparams}))");
                        gen.EnterBlock();
                        gen.Add("return GetRecords(dr).FirstOrDefault();");
                        gen.ExitBlock();
                        gen.ExitBlock();
                    }
                    else
                    {
                        gen.Add($"public static async Task<AutoClosingEnumerable<{funcdef.Output[0].CSharpTypeName}>> CallFunction(SQLDB sql{funcparams})");
                        gen.EnterBlock();
                        gen.Add($"AutoClosingDataReader dr = await sql.ExecuteReader(\"SELECT * FROM \\\"{funcdef.FunctionName}\\\"({numparams});\"{standalonefuncparams});");
                        gen.Add($"return new AutoClosingEnumerable<{funcdef.Output[0].CSharpTypeName}>(GetRecords(dr), dr);");
                        gen.ExitBlock();
                    }
                    break;
                default:
                    if (funcdef.SingleRow)
                    {
                        gen.Add($"public static async Task<{funcdef.FunctionName}?> CallFunction(SQLDB sql{funcparams})");
                        gen.EnterBlock();
                        gen.Add($"using(AutoClosingDataReader dr = await sql.ExecuteReader(\"SELECT * FROM \\\"{funcdef.FunctionName}\\\"({numparams});\"{standalonefuncparams}))");
                        gen.EnterBlock();
                        gen.Add("return GetRecords(dr).FirstOrDefault();");
                        gen.ExitBlock();
                        gen.ExitBlock();
                    }
                    else
                    {
                        gen.Add($"public static async Task<AutoClosingEnumerable<{funcdef.FunctionName}>> CallFunction(SQLDB sql{funcparams})");
                        gen.EnterBlock();
                        gen.Add($"AutoClosingDataReader dr = await sql.ExecuteReader(\"SELECT * FROM \\\"{funcdef.FunctionName}\\\"({numparams});\"{standalonefuncparams});");
                        gen.Add($"return new AutoClosingEnumerable<{funcdef.FunctionName}>(GetRecords(dr), dr);");
                        gen.ExitBlock();
                    }
                    break;
            }
        }
        public static void DropFunction(this CodeGenerator gen, string FunctionName)
        {
            gen.Add($"public static Task DropFunction(SQLDB sql) => ");
            gen.Indent();
            gen.Add($"sql.ExecuteNonQuery(\"DROP FUNCTION \\\"{FunctionName}\\\";\");");
            gen.Unindent();
        }
    }
}