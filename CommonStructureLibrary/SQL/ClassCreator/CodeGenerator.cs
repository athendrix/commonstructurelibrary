using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL.ClassCreator
{
    public class CodeGenerator
    {
        #region Core
        private int CurrentIndentationLevel = 0;
        private readonly int IndentAmount;
        private List<string> toReturn = new List<string>();
        public CodeGenerator(int IndentAmount = 4) => this.IndentAmount = IndentAmount;
        
        public void Reset()
        {
            toReturn = new List<string>();
            CurrentIndentationLevel = 0;
        }
        public override string ToString() => string.Join(Environment.NewLine, toReturn);
        #endregion
        public void Libraries(params string[] Libs)
        {
            foreach (string Lib in Libs)
            {
                IndentAdd($"using {Lib};");
            }
        }
        public void Namespace(string namespacename)
        {
            IndentAdd("namespace " + namespacename);
            EnterBlock();
        }
        public void EndNamespace() => ExitBlock();
        public void Region(string RegionName) => IndentAdd("#region " + RegionName);
        public void EndRegion() => IndentAdd("#endregion");

        #region Basics
        public void BlankLine() => IndentAdd();
        public void IndentAdd(params string[] toAdd) => toReturn.Add(new string(' ', CurrentIndentationLevel) + string.Join("", toAdd));

        public void IndentAddQuoted(string line, string end = " +") => IndentAdd("\"" + line.Replace("\"", "\\\"") + "\"" + end);
        public void EnterBlock(bool commented = false)
        {
            IndentAdd(commented ? "//" : "", "{");
            Indent();
        }
        public void ExitBlock(bool commented = false)
        {
            Unindent();
            IndentAdd(commented ? "//" : "", "}");
        }
        public void ExitInlineBlock(bool commented = false)
        {
            Unindent();
            IndentAdd(commented ? "//" : "", "};");
        }
        public void Indent() => CurrentIndentationLevel += IndentAmount;
        public void Unindent() => CurrentIndentationLevel -= IndentAmount;
        #endregion
        #region Advanced
        public void BeginRecord(string RecordName, Column[] Columns, string Append = "")
        {
            string TypeColumns = string.Join(", ", Columns.Select((x) => x.CSharpTypeName + " " + x.ColumnName));
            IndentAdd("public record " + RecordName.Replace(' ', '_') + "(" + TypeColumns + ")" + Append);
            EnterBlock();
        }
        public void EndRecord() => ExitBlock();
        public void GetRecords(string RecordName, Column[] Columns)
        {
            if(Columns.Length == 0) {return;}
            if(Columns.Length == 1)
            {
                IndentAdd($"public static IEnumerable<{Columns[0].CSharpTypeName}> GetRecords(IDataReader dr)");
                EnterBlock();
                IndentAdd("while(dr.Read())");
                EnterBlock();
                string PrivCSType = Columns[0].CSharpPrivateTypeName;
                string CSType = Columns[0].CSharpTypeName;
                string Name = Columns[0].ColumnName;
                string pubpre = Columns[0].CSharpConvertPublicPrepend;
                string pubapp = Columns[0].CSharpConvertPublicAppend;
                bool nullable = Columns[0].nullable;
                if (CSType != PrivCSType)
                {
                    IndentAdd($"{PrivCSType} _{Name} = {(nullable ? "dr.IsDBNull(0) ? null :" : "")} ({PrivCSType.TrimEnd('?')})dr[0];");
                    IndentAdd($"yield return {pubpre}_{Name}{(nullable && !string.IsNullOrEmpty(pubapp) ? "?" : "")}{pubapp};");
                }
                else
                {
                    IndentAdd($"yield return {(nullable ? "dr.IsDBNull(0) ? null :" : "")} ({PrivCSType.TrimEnd('?')})dr[0];");
                }
                ExitBlock();
                IndentAdd("yield break;");
                ExitBlock();
                return;
            }
            IndentAdd($"public static IEnumerable<{RecordName.Replace(' ','_')}> GetRecords(IDataReader dr)");
            EnterBlock();
            IndentAdd("while(dr.Read())");
            EnterBlock();
            for (int i = 0; i < Columns.Length; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                string CSType = Columns[i].CSharpTypeName;
                string Name = Columns[i].ColumnName;
                string pubpre = Columns[i].CSharpConvertPublicPrepend;
                string pubapp = Columns[i].CSharpConvertPublicAppend;
                bool nullable = Columns[i].nullable;
                if (CSType != PrivCSType)
                {
                    IndentAdd($"{PrivCSType} _{Name} = {(nullable ? "dr.IsDBNull(" + i + ") ? null :" : "")} ({PrivCSType.TrimEnd('?')})dr[{i}];");
                    IndentAdd($"{CSType} {Name} = {pubpre}_{Name}{(nullable && !string.IsNullOrEmpty(pubapp) ? "?" : "")}{pubapp};");
                }
                else
                {
                    IndentAdd($"{CSType} {Name} = {(nullable ? "dr.IsDBNull(" + i + ") ? null :" : "")} ({PrivCSType.TrimEnd('?')})dr[{i}];");
                }
            }
            IndentAdd($"yield return new {RecordName.Replace(' ', '_')}({string.Join(", ", Columns.Select((x) => x.ColumnName))});");
            ExitBlock();
            IndentAdd("yield break;");
            ExitBlock();
        }
        #endregion
    }
}
