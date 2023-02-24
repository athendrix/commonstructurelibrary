using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSL.SQL
{
    public class Conditional
    {
        private readonly string joinstring;
        private readonly string ColumnName;
        private readonly IS Condition;
        private readonly object?[] parameters;
        private readonly Conditional? previous;
        private Conditional? subConditional;
        private Conditional(string joinstring, string ColumnName, IS Condition, object?[] parameters, Conditional? previous)
        {
            this.joinstring = joinstring;
            this.ColumnName = ColumnName;
            this.Condition = Condition;
            this.parameters = parameters;
            this.previous = previous;
        }
        private Conditional(string joinstring, Conditional subConditional, Conditional previous)
        {
            this.joinstring = joinstring;
            this.subConditional = subConditional;
            this.previous = previous;
            this.ColumnName = "";
            this.Condition = IS.EQUAL_TO;
            this.parameters = new object[0];
        }
        public static Conditional WHERE(string ColumnName, IS Condition, params object?[] parameters) => new Conditional("", ColumnName, Condition, parameters, null);

        public Conditional AND(string ColumnName, IS Condition, params object?[] parameters) => new Conditional(" AND ", ColumnName, Condition, parameters, this);
        public Conditional OR(string ColumnName, IS Condition, params object?[] parameters) => new Conditional(" OR ", ColumnName, Condition, parameters, this);
        public Conditional AND(Conditional subConditional) => new Conditional(" AND ", subConditional, this);
        public Conditional OR(Conditional subConditional) => new Conditional(" OR ", subConditional, this);
        
        private static int AddOrIndex(ref List<object> list, object toAddorIndex)
        {
            int index;
            if ((index = list.IndexOf(toAddorIndex)) == -1)
            {
                index = list.Count;
                list.Add(toAddorIndex);
            }
            return index;
        }
        public string Build(SQLDB sql, ParameterInfo[] ValidColumns, ref List<object> ParametersList)
        {
            Stack<Conditional> conditionalStack = new Stack<Conditional>();
            if (sql is PostgreSQL)
            {
                conditionalStack.Push(this);
                Conditional? current;
                while ((current = conditionalStack.Peek().previous) != null) { conditionalStack.Push(current); }
                StringBuilder sb = new StringBuilder();
                while (conditionalStack.Count > 0)
                {
                    current = conditionalStack.Pop();
                    ParameterInfo? currentColumn = ValidColumns.Where(x => x.Name == current.ColumnName).FirstOrDefault();
                    //For injection protection we're making sure the column name is a valid part of the record.
                    if (currentColumn == null) { throw new ArgumentException($"{current.ColumnName} is not a valid column name!"); }

                    bool inequality = current.Condition is IS.LESS_THAN or IS.LESS_THAN_OR_EQUAL_TO or IS.GREATER_THAN or IS.GREATER_THAN_OR_EQUAL_TO;
                    bool unsignedType = currentColumn.ParameterType == typeof(ulong) || currentColumn.ParameterType == typeof(uint) || currentColumn.ParameterType == typeof(ushort);
                    bool notValue = current.Condition is IS.NOT_EQUAL_TO or IS.NOT_IN or IS.NOT_BETWEEN or IS.NOT_LIKE or IS.NOT_STARTING_WITH or IS.NOT_ENDING_WITH or IS.NOT_CONTAINING;


                    sb.Append(current.joinstring);

                    string notString = notValue ? " NOT" : "";
                    string a = Common.Escape(current.ColumnName);

                    switch (current.parameters.Length)
                    {
                        case 0:
                            if (current.subConditional == null) { throw new ArgumentOutOfRangeException("Conditionals require parameters to compare against."); }
                            sb.Append("(" + current.subConditional.Build(sql, ValidColumns, ref ParametersList) + ")");
                            break;
                        case 1:
                            #region 1 parameter
                            object? value = current.parameters[0];

                            if (value is null)
                            {
                                if (current.Condition is IS.EQUAL_TO or IS.IN or IS.NOT_EQUAL_TO or IS.NOT_IN)
                                {
                                    sb.Append($"{a} IS{notString} NULL");
                                    break;
                                }
                                throw new ArgumentNullException("A null value is only allowed for `=` or `IN` comparisons.");
                            }
                            //Fix Typing
                            if (value.GetType() != currentColumn.ParameterType) { value = Convert.ChangeType(value, currentColumn.ParameterType); }
                            string b = "@" + AddOrIndex(ref ParametersList, value);
                            if (unsignedType && inequality)
                            {
                                bool gt = current.Condition is IS.GREATER_THAN or IS.GREATER_THAN_OR_EQUAL_TO;
                                bool eq = current.Condition is IS.GREATER_THAN_OR_EQUAL_TO or IS.LESS_THAN_OR_EQUAL_TO;
                                //Black magic to convert inequality tests on a signed integer to inequality tests on an unsigned integer
                                //This is necessary because Postgres doesn't do unsigned integers, so we need to mimic inequality comparisons so values won't be wrong.
                                sb.Append($"({a} {(gt ? "<" : ">=")} 0 AND {b} {(gt ? ">=" : "<")} 0 OR {a} {(gt ? ">" : "<")}{(eq ? "=" : "")} {b} AND ({a} {(gt ? "<" : ">=")} 0 OR {b} {(gt ? ">=" : "<")} 0))");
                                break;
                            }
                            string comp = current.Condition switch
                            {
                                IS.EQUAL_TO => "=",
                                IS.NOT_EQUAL_TO => "!=",
                                IS.GREATER_THAN => ">",
                                IS.LESS_THAN => "<",
                                IS.GREATER_THAN_OR_EQUAL_TO => ">=",
                                IS.LESS_THAN_OR_EQUAL_TO => "<=",
                                IS.IN => "=",
                                IS.NOT_IN => "!=",
                                IS.BETWEEN => throw new ArgumentOutOfRangeException("Between comparisons require exactly 2 parameters."),
                                IS.NOT_BETWEEN => throw new ArgumentOutOfRangeException("Between comparisons require exactly 2 parameters."),
                                IS.LIKE => "LIKE",
                                IS.NOT_LIKE => "NOT LIKE",
                                IS.STARTING_WITH => "LIKE",
                                IS.NOT_STARTING_WITH => "NOT LIKE",
                                IS.ENDING_WITH => "LIKE",
                                IS.NOT_ENDING_WITH => "NOT LIKE",
                                IS.CONTAINING => "LIKE",
                                IS.NOT_CONTAINING => "NOT LIKE",
                                _ => throw new InvalidCastException("Not a valid condition."),
                            };
                            if (current.Condition is IS.STARTING_WITH or IS.NOT_STARTING_WITH or IS.CONTAINING or IS.NOT_CONTAINING)
                            {
                                value = value.ToString() + "%";
                            }
                            if (current.Condition is IS.ENDING_WITH or IS.NOT_ENDING_WITH or IS.CONTAINING or IS.NOT_CONTAINING)
                            {
                                value = "%" + value.ToString();
                            }

                            sb.Append($"{a} {comp} @{AddOrIndex(ref ParametersList, value)}");
                            break;
                        #endregion
                        case 2:
                            #region 2 parameters
                            if (current.Condition is IS.IN or IS.NOT_IN) { goto default; }//Handle all the IN cases in the default case. Only handle BETWEEN cases here
                            object? first = current.parameters[0];
                            object? last = current.parameters[1];
                            if (current.Condition is not IS.BETWEEN and not IS.NOT_BETWEEN) { throw new ArgumentOutOfRangeException("Too many parameters for comparison."); }
                            if (first is null || last is null) { throw new ArgumentNullException("Between comparisons cannot use null values."); }

                            #region Special unsigned BETWEEN cases
                            //We just rewrite the between as the equivilant GreaterThan/LessThan statements.
                            if (current.Condition is IS.BETWEEN && unsignedType)
                            {
                                current.subConditional = WHERE(current.ColumnName, IS.GREATER_THAN_OR_EQUAL_TO, first).AND(current.ColumnName, IS.LESS_THAN_OR_EQUAL_TO, last);
                                goto case 0;//handle subConditional
                            }
                            if (current.Condition is IS.NOT_BETWEEN && unsignedType)
                            {
                                current.subConditional = WHERE(current.ColumnName, IS.LESS_THAN, first).OR(current.ColumnName, IS.GREATER_THAN, last);
                                goto case 0;//handle subConditional
                            }
                            #endregion

                            if (first.GetType() != currentColumn.ParameterType) { first = Convert.ChangeType(first, currentColumn.ParameterType); }
                            if (last.GetType() != currentColumn.ParameterType) { last = Convert.ChangeType(last, currentColumn.ParameterType); }
                            sb.Append($"{a}{notString} BETWEEN @{AddOrIndex(ref ParametersList, first)} AND @{AddOrIndex(ref ParametersList, last)}");
                            break;
                        #endregion
                        default:
                            #region many parameters
                            if (current.Condition is not IS.IN and not IS.NOT_IN) { throw new ArgumentOutOfRangeException("Too many parameters for comparison."); }
                            bool nullinlist = false;
                            List<object> NonNullParameters = new List<object>();
                            for (int i = 0; i < current.parameters.Length; i++)
                            {
                                object? toAdd = current.parameters[i];
                                if (toAdd is null)
                                {
                                    nullinlist = true;
                                    continue;
                                }
                                if (toAdd.GetType() != currentColumn.ParameterType) { toAdd = Convert.ChangeType(toAdd, currentColumn.ParameterType); }
                                NonNullParameters.Add(toAdd);
                            }
                            string[] nnpstrings = new string[NonNullParameters.Count];
                            for (int i = 0; i < nnpstrings.Length; i++)
                            {
                                nnpstrings[i] = $"@{AddOrIndex(ref ParametersList, NonNullParameters[i])}";
                            }
                            bool nullandothers = nullinlist && nnpstrings.Length > 0;
                            if (nullandothers) { sb.Append("("); }
                            if (nullinlist) { sb.Append($"{a} IS{notString} NULL"); }
                            if (nullandothers) { sb.Append($" {(notValue ? "AND" : "OR")} "); }
                            if (nnpstrings.Length > 0) { sb.Append($"{a}{notString} IN ({string.Join(", ", nnpstrings)})"); }
                            if (nullandothers) { sb.Append(")"); }
                            break;
                            #endregion
                    }
                }
                return sb.ToString();
            }
            throw new NotImplementedException();
        }
    }

    public enum IS : byte
    {
        EQUAL_TO,
        NOT_EQUAL_TO,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_OR_EQUAL_TO,
        LESS_THAN_OR_EQUAL_TO,
        IN,
        NOT_IN,
        BETWEEN,
        NOT_BETWEEN,
        LIKE,
        NOT_LIKE,
        STARTING_WITH,
        NOT_STARTING_WITH,
        ENDING_WITH,
        NOT_ENDING_WITH,
        CONTAINING,
        NOT_CONTAINING
    }
}
