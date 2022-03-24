using System;
using System.Collections.Generic;
using System.Linq;

namespace CSL.ClassCreation
{
    public class CodeGenerator
    {
        #region Core
        private int CurrentIndentationLevel = 0;
        private readonly int IndentAmount;
        private List<string> toReturn = new List<string>();
        private bool Commented = false;
        private int CurrentCommentIndentationLevel = 0;
        public CodeGenerator(int IndentAmount = 4) => this.IndentAmount = IndentAmount;
        
        public void Reset()
        {
            toReturn = new List<string>();
            CurrentIndentationLevel = 0;
            Commented = false;
            CurrentCommentIndentationLevel = 0;
        }
        public override string ToString() => string.Join(Environment.NewLine, toReturn);
        #endregion
        public void Libraries(params string[] Libs)
        {
            foreach (string Lib in Libs)
            {
                Add($"using {Lib};");
            }
        }
        public void Namespace(string namespacename)
        {
            Add("namespace " + namespacename);
            EnterBlock();
        }
        public void EndNamespace() => ExitBlock();
        public void Region(string RegionName) => Add("#region " + RegionName);
        public void EndRegion() => Add("#endregion");

        #region Basics
        public void BlankLine() => Add();
        public void Add(params string[] toAdd) => toReturn.Add(new string(' ', CurrentIndentationLevel) + (Commented ? ("//" + new string(' ', CurrentCommentIndentationLevel)) : "") + string.Join("", toAdd));

        public void AddQuoted(string line, string end = " +") => Add("\"" + line.Replace("\"", "\\\"") + "\"" + end);
        public void EnterBlock()
        {
            Add("{");
            Indent();
        }
        public void ExitBlock()
        {
            Unindent();
            Add("}");
        }
        public void ExitInlineBlock()
        {
            Unindent();
            Add("};");
        }
        public void Indent()
        {
            if(Commented)
            {
                CurrentCommentIndentationLevel += IndentAmount;
            }
            else
            {
                CurrentIndentationLevel += IndentAmount;
            }
        }

        public void Unindent()
        {
            if (Commented)
            {
                CurrentCommentIndentationLevel -= IndentAmount;
            }
            else
            {
                CurrentIndentationLevel -= IndentAmount;
            }
        }
        public void EnterComment()
        {
            Commented = true;
            CurrentCommentIndentationLevel = 0;
        }

        public void ExitComment()
        {
            Commented = false;
            CurrentCommentIndentationLevel = 0;
        }
        public void Comment(string comment) => Add("//" , comment);
        #endregion
        #region Advanced
        //public void BeginRecord(string RecordName, IEnumerable<string> ColumnTypesAndNames, string Append = "")
        public void BeginRecord(string RecordName, IEnumerable<ParameterDefinition> Parameters, string Append = "")
        {
            string TypeColumns = string.Join(", ", Parameters);
            Add("public record " + RecordName.Replace(' ', '_') + "(" + TypeColumns + ")" + Append);
            EnterBlock();
        }
        public void EndRecord() => ExitBlock();
        #endregion
    }
    public record ParameterDefinition(string parameterType, string parameterName)
    {
        public override string ToString() => parameterType + " " + parameterName;
    }
}
