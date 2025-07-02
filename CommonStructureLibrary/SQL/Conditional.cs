using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSL.SQL
{
    public abstract class Conditional
    {
        protected readonly Conditional? previous;
        protected readonly ConditionalType ctype;
        protected abstract bool Validate(ConditionalType pctype);
        public string Build(SQLDB sql, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional = false)
        {
            if (sql is PostgreSQL) { return Build(BuildType.PostgreSQL, ValidColumns, ref ParametersList, SubConditional); }
            if (sql is Sqlite) { return Build(BuildType.Sqlite, ValidColumns, ref ParametersList, SubConditional); }
            throw new NotImplementedException();
        }
        protected static int AddOrIndex(ref List<object> list, object toAddorIndex)
        {
            int index;
            if ((index = list.IndexOf(toAddorIndex)) == -1)
            {
                index = list.Count;
                list.Add(toAddorIndex);
            }
            return index;
        }
        public string Build(BuildType buildType, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional = false)
        {
            Stack<Conditional> conditionalStack = new Stack<Conditional>();
            for (Conditional? c = this; c != null; c = c.previous) { conditionalStack.Push(c); }
            StringBuilder toReturn = new StringBuilder();
            while (conditionalStack.Count > 0)
            {
                toReturn.Append(conditionalStack.Pop().BuildSegment(buildType, ValidColumns, ref ParametersList, SubConditional));
            }
            return toReturn.ToString();
        }

        protected abstract string BuildSegment(BuildType buildType, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional);
        
        protected Conditional(Conditional? previous, ConditionalType ctype)
        {
            this.previous = previous;
            this.ctype = ctype;
            if(!Validate(previous?.ctype ?? ConditionalType.NULL))
            {
                throw new ArgumentException("Invalid conditional chain!");
            }
        }
        protected internal enum ConditionalType :byte
        {
            NULL,
            WHERE,
            AND,
            OR,
            ORDERBY,
            THENBY,
            LIMIT
        }
        #region SubClass Instance Creators
        public static WhereClauseSegment WHERE(string ColumnName, IS Condition, params object?[]? parameters) => WhereClauseSegment.WhereClauseWhere(ColumnName, Condition, parameters ?? new object?[] { null });
        public static SubconditionalSegment WHERE(Conditional subConditional) => SubconditionalSegment.SubCondtionalWhere(subConditional);
        public static WhereClauseSegment WHERE(bool boolval) => WhereClauseSegment.WhereClauseWhere("1", IS.EQUAL_TO, new object?[] { boolval ? 1 : 0 });
        #endregion
    }
    public sealed class WhereClauseSegment : Conditional
    {
        private readonly string ColumnName;
        private readonly IS Condition;
        private readonly object?[] parameters;
        #region Chains
        internal static WhereClauseSegment WhereClauseWhere(string ColumnName, IS Condition, object?[] parameters) => new WhereClauseSegment(null, ConditionalType.WHERE, ColumnName, Condition, parameters);
        public WhereClauseSegment AND(string ColumnName, IS Condition, params object?[]? parameters) => new WhereClauseSegment(this, ConditionalType.AND, ColumnName, Condition, parameters ?? new object?[] { null });
        public WhereClauseSegment OR(string ColumnName, IS Condition, params object?[]? parameters) => new WhereClauseSegment(this, ConditionalType.OR, ColumnName, Condition, parameters ?? new object?[] { null });
        public SubconditionalSegment AND(Conditional subConditional) => new SubconditionalSegment(this, ConditionalType.AND, subConditional);
        public SubconditionalSegment OR(Conditional subConditional) => new SubconditionalSegment(this, ConditionalType.OR, subConditional);
        public OrderClauseSegment ORDERBY(string ColumnName) => new OrderClauseSegment(this, ConditionalType.ORDERBY, ColumnName, true);
        public OrderClauseSegment ORDERBYDESC(string ColumnName) => new OrderClauseSegment(this, ConditionalType.ORDERBY, ColumnName, false);
        #endregion
        internal WhereClauseSegment(Conditional? previous, ConditionalType ctype, string ColumnName, IS Condition, object?[] parameters) : base(previous, ctype)
        {
            this.ColumnName = ColumnName;
            this.Condition = Condition;
            
            if(this.Condition is IS.IN or IS.BETWEEN && parameters.Length is 1 && parameters[0] is Array a && parameters[0] is not byte[])
            {
                this.parameters = new object[a.Length];
                Array.Copy(a, this.parameters, a.Length);
            }
            else
            {
                this.parameters = parameters;
            }
        }
        protected override string BuildSegment(BuildType buildType, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional)
        {
            string joinstring = ctype switch
            {
                ConditionalType.WHERE => SubConditional ? "" : " WHERE ",
                ConditionalType.AND => " AND ",
                ConditionalType.OR => " OR ",
                _ => throw new ArgumentException("Invalid Conditional Type.")
            };
            if (ColumnName is "1" && Condition is IS.EQUAL_TO && parameters.Length is 1 && parameters[0] is 0 or 1)
            {
                return $"{joinstring}1 = {parameters[0]}";
            }
            RecordParameter? currentColumn = ValidColumns.Where(x => x.Name == ColumnName).FirstOrDefault();
            //For injection protection we're making sure the column name is a valid part of the record.
            if (currentColumn == null) { throw new ArgumentException($"\"{ColumnName}\" is not a valid column name!"); }

            bool inequality = Condition is IS.LESS_THAN or IS.LESS_THAN_OR_EQUAL_TO or IS.GREATER_THAN or IS.GREATER_THAN_OR_EQUAL_TO;
            bool unsignedType = currentColumn.Type == typeof(ulong) || currentColumn.Type == typeof(uint) || currentColumn.Type == typeof(ushort);
            bool notValue = Condition is IS.NOT_EQUAL_TO or IS.NOT_IN or IS.NOT_BETWEEN or IS.NOT_LIKE or IS.NOT_STARTING_WITH or IS.NOT_ENDING_WITH or IS.NOT_CONTAINING or IS.NOT_ILIKE;


            string notString = notValue ? " NOT" : "";
            string a = Common.Escape(ColumnName);
            if(Condition is IS.ILIKE or IS.NOT_ILIKE)
            {
                a = $"UPPER({a})";
            }

            switch (parameters.Length)
            {
                case 0:
                    throw new ArgumentOutOfRangeException("Conditionals require parameters to compare against.");
                case 1:
                    #region 1 parameter
                    object? value = parameters[0];

                    if (value is null)
                    {
                        if (Condition is IS.EQUAL_TO or IS.IN or IS.NOT_EQUAL_TO or IS.NOT_IN)
                        {
                            return $"{joinstring}{a} IS{notString} NULL";
                        }
                        throw new ArgumentNullException("A null value is only allowed for `=` or `IN` comparisons.");
                    }
                    //Fix Typing
                    if (value.GetType() != currentColumn.Type && value.GetType() != Nullable.GetUnderlyingType(currentColumn.Type)) { value = Convert.ChangeType(value, currentColumn.Type); }
                    if (Condition is IS.STARTING_WITH or IS.NOT_STARTING_WITH or IS.CONTAINING or IS.NOT_CONTAINING)
                    {
                        value = value.ToString() + "%";
                    }
                    if (Condition is IS.ENDING_WITH or IS.NOT_ENDING_WITH or IS.CONTAINING or IS.NOT_CONTAINING)
                    {
                        value = "%" + value.ToString();
                    }
                    string b = "@" + AddOrIndex(ref ParametersList, value);
                    if (Condition is IS.ILIKE or IS.NOT_ILIKE)
                    {
                        b = $"UPPER({b})";
                    }
                    if (unsignedType && inequality)
                    {
                        bool gt = Condition is IS.GREATER_THAN or IS.GREATER_THAN_OR_EQUAL_TO;
                        bool eq = Condition is IS.GREATER_THAN_OR_EQUAL_TO or IS.LESS_THAN_OR_EQUAL_TO;
                        //Black magic to convert inequality tests on a signed integer to inequality tests on an unsigned integer
                        //This is necessary because Postgres doesn't do unsigned integers, so we need to mimic inequality comparisons so values won't be wrong.
                        return $"{joinstring}({a} {(gt ? "<" : ">=")} 0 AND {b} {(gt ? ">=" : "<")} 0 OR {a} {(gt ? ">" : "<")}{(eq ? "=" : "")} {b} AND ({a} {(gt ? "<" : ">=")} 0 OR {b} {(gt ? ">=" : "<")} 0))";
                    }
                    string comp = Condition switch
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
                        IS.ILIKE => "LIKE",
                        IS.LIKE => "LIKE",
                        IS.NOT_LIKE => "NOT LIKE",
                        IS.NOT_ILIKE => "NOT LIKE",
                        IS.STARTING_WITH => "LIKE",
                        IS.NOT_STARTING_WITH => "NOT LIKE",
                        IS.ENDING_WITH => "LIKE",
                        IS.NOT_ENDING_WITH => "NOT LIKE",
                        IS.CONTAINING => "LIKE",
                        IS.NOT_CONTAINING => "NOT LIKE",
                        _ => throw new InvalidCastException("Not a valid condition."),
                    };

                    return $"{joinstring}{a} {comp} {b}";
                #endregion
                case 2:
                    #region 2 parameters
                    if (Condition is IS.IN or IS.NOT_IN) { goto default; }//Handle all the IN cases in the default case. Only handle BETWEEN cases here
                    object? first = parameters[0];
                    object? last = parameters[1];
                    if (Condition is not IS.BETWEEN and not IS.NOT_BETWEEN) { throw new ArgumentOutOfRangeException("Too many parameters for comparison."); }
                    if (first is null || last is null) { throw new ArgumentNullException("Between comparisons cannot use null values."); }

                    #region Special unsigned BETWEEN cases
                    //We just rewrite the between as the equivilant GreaterThan/LessThan statements.
                    if (Condition is IS.BETWEEN && unsignedType)
                    {
                        return $"{joinstring}({WHERE(ColumnName, IS.GREATER_THAN_OR_EQUAL_TO, first).AND(ColumnName, IS.LESS_THAN_OR_EQUAL_TO, last).Build(buildType, ValidColumns, ref ParametersList, true)})";
                    }
                    if (Condition is IS.NOT_BETWEEN && unsignedType)
                    {
                        return $"{joinstring}({WHERE(ColumnName, IS.LESS_THAN, first).OR(ColumnName, IS.GREATER_THAN, last).Build(buildType, ValidColumns, ref ParametersList, true)})";
                    }
                    #endregion

                    if (first.GetType() != currentColumn.Type) { first = Convert.ChangeType(first, currentColumn.Type); }
                    if (last.GetType() != currentColumn.Type) { last = Convert.ChangeType(last, currentColumn.Type); }
                    return $"{joinstring}{a}{notString} BETWEEN @{AddOrIndex(ref ParametersList, first)} AND @{AddOrIndex(ref ParametersList, last)}";
                #endregion
                default:
                    #region many parameters
                    if (Condition is not IS.IN and not IS.NOT_IN) { throw new ArgumentOutOfRangeException("Too many parameters for comparison."); }
                    bool nullinlist = false;
                    List<object> NonNullParameters = new List<object>();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        object? toAdd = parameters[i];
                        if (toAdd is null)
                        {
                            nullinlist = true;
                            continue;
                        }
                        if (toAdd.GetType() != currentColumn.Type) { toAdd = Convert.ChangeType(toAdd, currentColumn.Type); }
                        NonNullParameters.Add(toAdd);
                    }
                    string[] nnpstrings = new string[NonNullParameters.Count];
                    for (int i = 0; i < nnpstrings.Length; i++)
                    {
                        nnpstrings[i] = $"@{AddOrIndex(ref ParametersList, NonNullParameters[i])}";
                    }
                    bool nullandothers = nullinlist && nnpstrings.Length > 0;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(joinstring);
                    if (nullandothers) { sb.Append("("); }
                    if (nullinlist) { sb.Append($"{a} IS{notString} NULL"); }
                    if (nullandothers) { sb.Append($" {(notValue ? "AND" : "OR")} "); }
                    if (nnpstrings.Length > 0) { sb.Append($"{a}{notString} IN ({string.Join(", ", nnpstrings)})"); }
                    if (nullandothers) { sb.Append(")"); }
                    return sb.ToString();
                    #endregion
            }
        }
        protected override bool Validate(ConditionalType pctype) => (ctype is ConditionalType.WHERE && pctype is ConditionalType.NULL) ||
                (ctype is ConditionalType.AND or ConditionalType.OR && pctype is ConditionalType.WHERE or ConditionalType.AND or ConditionalType.OR);
    }
    public sealed class SubconditionalSegment : Conditional
    {
        private readonly Conditional subConditional;
        #region Chains
        internal static SubconditionalSegment SubCondtionalWhere(Conditional subConditional) => new SubconditionalSegment(null, ConditionalType.WHERE, subConditional);
        public WhereClauseSegment AND(string ColumnName, IS Condition, params object?[]? parameters) => new WhereClauseSegment(this, ConditionalType.AND, ColumnName, Condition, parameters ?? new object?[] { null });
        public WhereClauseSegment OR(string ColumnName, IS Condition, params object?[]? parameters) => new WhereClauseSegment(this, ConditionalType.OR, ColumnName, Condition, parameters ?? new object?[] { null });
        public SubconditionalSegment AND(Conditional subConditional) => new SubconditionalSegment(this, ConditionalType.AND, subConditional);
        public SubconditionalSegment OR(Conditional subConditional) => new SubconditionalSegment(this, ConditionalType.OR, subConditional);
        public OrderClauseSegment ORDERBY(string ColumnName) => new OrderClauseSegment(this, ConditionalType.ORDERBY, ColumnName, true);
        public OrderClauseSegment ORDERBYDESC(string ColumnName) => new OrderClauseSegment(this, ConditionalType.ORDERBY, ColumnName, false);
        #endregion
        internal SubconditionalSegment(Conditional? previous, ConditionalType ctype, Conditional subConditional) : base(previous, ctype) => this.subConditional = subConditional;
        protected override string BuildSegment(BuildType buildType, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional)
        {
            string joinstring = ctype switch
            {
                ConditionalType.AND => " AND ",
                ConditionalType.OR => " OR ",
                ConditionalType.WHERE => SubConditional ? "" : " WHERE ",
                _ => throw new ArgumentException("Invalid Conditional Type.")
            };
            return $"{joinstring}({subConditional.Build(buildType, ValidColumns, ref ParametersList, true)})";
        }
        protected override bool Validate(ConditionalType pctype) => (ctype is ConditionalType.WHERE && pctype is ConditionalType.NULL) ||
                (ctype is ConditionalType.AND or ConditionalType.OR && pctype is ConditionalType.WHERE or ConditionalType.AND or ConditionalType.OR);
    }
    public sealed class OrderClauseSegment : Conditional
    {
        private readonly string ColumnName;
        private readonly bool ascending;
        public OrderClauseSegment THENBY(string ColumnName, bool ascending = true) => new OrderClauseSegment(this, ConditionalType.THENBY, ColumnName, ascending);
        public LimitSegment LIMIT(int rowcount, int offset = 0) => new LimitSegment(this, ConditionalType.LIMIT, rowcount, offset);
        internal OrderClauseSegment(Conditional? previous, ConditionalType ctype, string ColumnName, bool ascending) : base(previous, ctype)
        {
            this.ColumnName = ColumnName;
            this.ascending = ascending;
        }

        protected override string BuildSegment(BuildType buildType, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional)
        {
            if (!ValidColumns.Where(x => x.Name == ColumnName).Any()) { throw new ArgumentException($"\"{ColumnName}\" is not a valid column name!"); }

            string joinstring = ctype switch
            {
                ConditionalType.ORDERBY => " ORDER BY ",
                ConditionalType.THENBY => ", ",
                _ => throw new ArgumentException("Invalid Conditional Type.")
            };
            return $"{joinstring}{Common.Escape(ColumnName)} {(ascending ? "ASC" : "DESC")}";
        }
        protected override bool Validate(ConditionalType pctype) => (ctype is ConditionalType.THENBY && pctype is ConditionalType.ORDERBY) ||
                (ctype is ConditionalType.ORDERBY && pctype is ConditionalType.WHERE or ConditionalType.AND or ConditionalType.OR);
    }

    public sealed class LimitSegment : Conditional
    {
        private readonly int rowcount;
        private readonly int offset;
        internal LimitSegment(Conditional? previous, ConditionalType ctype, int rowcount, int offset) : base(previous, ctype)
        {
            this.rowcount = rowcount;
            this.offset = offset;
        }

        protected override string BuildSegment(BuildType buildType, RecordParameter[] ValidColumns, ref List<object> ParametersList, bool SubConditional)
        {
            string offsetstring = offset == 0 ? "" : $" OFFSET {offset}";
            return $" LIMIT {rowcount}{offsetstring}";
        }
        protected override bool Validate(ConditionalType pctype) =>
                (ctype is ConditionalType.LIMIT && pctype is ConditionalType.WHERE or ConditionalType.AND or ConditionalType.OR or ConditionalType.ORDERBY or ConditionalType.THENBY);
    }

    public enum BuildType : byte
    {
        PostgreSQL,
        Sqlite
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
        ILIKE,
        NOT_LIKE,
        NOT_ILIKE,
        STARTING_WITH,
        NOT_STARTING_WITH,
        ENDING_WITH,
        NOT_ENDING_WITH,
        CONTAINING,
        NOT_CONTAINING,
        MATCHING_REGEX,
        NOT_MATCHING_REGEX
    }
}
