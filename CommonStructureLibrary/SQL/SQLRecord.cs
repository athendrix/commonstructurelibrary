using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq;

using static CSL.Helpers.Generics;

namespace CSL.SQL
{
    #region Attributes
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SQLRecordAttribute : Attribute
    {
        public readonly int PrimaryKeys;
        /// <summary>
        /// SQLLines are for adding things that aren't directly supported by SQLRecords.
        /// </summary>
        public string[] SQLLines { get; set; }
        /// <summary>
        /// Allows you to specify how many keys your record has starting from the left. Primary Keys should not be nullable, and errors will occur if they are.
        /// Optionally allows you to specify extra SQL Lines to be added to the Database creation SQL statement.
        /// </summary>
        /// <param name="PrimaryKeys">How many columns starting from the left are Primary Keys (minimum 1)</param>
        /// <exception cref="ArgumentException">All Advanced Records must have at least 1 primary key!</exception>
        public SQLRecordAttribute(int PrimaryKeys)
        {
            if (PrimaryKeys < 1) throw new ArgumentException("All SQL Records must have at least 1 primary key!");
            this.PrimaryKeys = PrimaryKeys;
            SQLLines = new string[0];
        }
    }
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class UniqueAttribute : Attribute
    {
        public readonly int Group;
        /// <summary>
        /// Lets you specify groups of Unique keys. Unique keys with the same Group are considered unique in aggregate.
        /// </summary>
        /// <param name="Group">The aggregate group this Unique key is part of.</param>
        public UniqueAttribute(int Group = 0) => this.Group = Group;
    }
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class CheckAttribute : Attribute
    {
        public readonly string CheckString;
        /// <summary>
        /// Lets you specify a comparison that any parameters have to satisfy. Check comparisons will be checked on Insert or Update.
        /// </summary>
        /// <param name="CheckString">The SQL to use for a check. For exmaple if I wanted to make sure a date happened after Dec 31, 1999, I'd put "> '1999-12-31'"</param>
        public CheckAttribute(string CheckString) => this.CheckString = CheckString;
    }
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class FKAttribute : Attribute
    {
        public readonly string ForeignTable;
        public readonly string ForeignKey;
        /// <summary>
        /// Lets you specify a foreign key relationship.
        /// </summary>
        /// <param name="Table">The Table the Foreign Key is in.</param>
        /// <param name="Key">The Foreign Key</param>
        public FKAttribute(string Table, string Key)
        {
            ForeignTable = Table;
            ForeignKey = Key;
        }
    }
    #endregion

    [SQLRecord(1)]
    public abstract record SQLRecord<T>() where T : SQLRecord<T>
    {
        private static readonly Dictionary<Type, string> SQLTypes = new Dictionary<Type, string>()
    {
        {typeof(bool),      "BOOLEAN"},
        {typeof(bool?),     "BOOLEAN"},
        {typeof(sbyte),     "SMALLINT"},
        {typeof(sbyte?),    "SMALLINT"},
        {typeof(byte),      "SMALLINT"},
        {typeof(byte?),     "SMALLINT"},
        {typeof(char),      "CHAR(1)"}, //Small bit of C# conversion required
        {typeof(char?),     "CHAR(1)"}, //Small bit of C# conversion required
        {typeof(short),     "SMALLINT"},
        {typeof(short?),    "SMALLINT"},
        {typeof(ushort),    "SMALLINT"},
        {typeof(ushort?),   "SMALLINT"},
        {typeof(int),       "INTEGER"},
        {typeof(int?),      "INTEGER"},
        {typeof(uint),      "INTEGER"},
        {typeof(uint?),     "INTEGER"},
        {typeof(long),      "BIGINT"},
        {typeof(long?),     "BIGINT"},
        {typeof(ulong),     "BIGINT"},
        {typeof(ulong?),    "BIGINT"},
        {typeof(float),     "FLOAT4"},
        {typeof(float?),    "FLOAT4"},
        {typeof(double),    "FLOAT8"},
        {typeof(double?),   "FLOAT8"},
        {typeof(decimal),   "NUMERIC"},
        {typeof(decimal?),  "NUMERIC"},
        {typeof(Guid),      "UUID"},
        {typeof(Guid?),     "UUID"},
        {typeof(DateTime),  "TIMESTAMP"},
        {typeof(DateTime?), "TIMESTAMP"},
        {typeof(string),    "TEXT"},
        {typeof(byte[]),    "BYTEA" }
    };
        private static string GetSQLType(Type type)
        {
            if (SQLTypes.ContainsKey(type)) { return SQLTypes[type]; }
            if (type.IsEnum) { return GetSQLType(type.GetEnumUnderlyingType()); }
            Type? UnderlyingNullableType = Nullable.GetUnderlyingType(type);
            if (UnderlyingNullableType != null) { return GetSQLType(UnderlyingNullableType); }
            //TODO: SQLRecords? Maybe something with Foreign Keys?
            throw new ArgumentException($"Type \"{type.Name}\" is not a valid SQL Type!");
            //return "<FIXME>";
        }
        #region Helper Properties
        protected static string TableName = Escape(typeof(T).Name);
        protected static SQLRecordAttribute ARA = typeof(T).GetCustomAttributes(true).SelectMany(x => x is SQLRecordAttribute r ? new SQLRecordAttribute[] { r } : new SQLRecordAttribute[0]).First();
        protected static ParameterInfo[] RecordParameters = typeof(T).GetConstructors()[0].GetParameters();
        protected static ParameterInfo[] PKs = RecordParameters.Take(ARA.PrimaryKeys).ToArray();
        protected static ParameterInfo[] Datas = RecordParameters.Skip(ARA.PrimaryKeys).ToArray();
        protected static string[] PKStrings = PKs.Select(x => $"{Escape(x.Name)} = @{x.Position}").ToArray();
        protected static string[] DataStrings = Datas.Select(x => $"{Escape(x.Name)} = @{x.Position}").ToArray();
        protected static string[] ExtraLines = GetExtraLines(RecordParameters.SelectMany(x => x.GetCustomAttributes().Select(y => new Tuple<ParameterInfo, Attribute>(x, y))).GroupBy(z => z.Item2.GetType()).ToArray());
        protected static string[] ParameterNumbers = Enumerable.Range(0, RecordParameters.Length).Select(i => $"@{i}").ToArray();
        #endregion
        #region Function Helpers
        private static string[] GetExtraLines(IGrouping<Type, Tuple<ParameterInfo, Attribute>>[] attributes)
        {
            List<string> toReturn = new List<string>();
            foreach (IGrouping<Type, Tuple<ParameterInfo, Attribute>> group in attributes)
            {
                if (group.Key == typeof(UniqueAttribute))
                {
                    foreach (IGrouping<int, Tuple<ParameterInfo, Attribute>> innergroup in group.GroupBy(x => ((UniqueAttribute)x.Item2).Group))
                    {
                        toReturn.Add($"UNIQUE({GetEscapedParameterNames(innergroup.Select(x => x.Item1))})");
                    }
                }
                if (group.Key == typeof(CheckAttribute))
                {
                    toReturn.AddRange(group.Select(x => $"CHECK({Escape(x.Item1.Name)} {((CheckAttribute)x.Item2).CheckString})"));
                }
                if (group.Key == typeof(FKAttribute))
                {
                    toReturn.AddRange(group.Select(x =>
                    {
                        FKAttribute fk = (FKAttribute)x.Item2;
                        return $"FOREIGN KEY({Escape(x.Item1.Name)}) REFERENCES {Escape(fk.ForeignTable)}({Escape(fk.ForeignKey)})";
                    }));
                }
            }
            return toReturn.ToArray();
        }
        protected static string FormatParameterInfos(IEnumerable<ParameterInfo> pis) => string.Join(", ", pis.Select(x => FormatParameterInfo(x)));
        protected static string GetEscapedParameterNames(IEnumerable<ParameterInfo> pis) => string.Join(", ", pis.Select(x => Escape(x.Name)));
        protected static string FormatParameterInfo(ParameterInfo pi) => $"{Escape(pi.Name)} {GetSQLType(pi.ParameterType)}{(IsNullable(pi) ? "" : " NOT NULL")}";
        protected static string Escape(string? name) => $"\"{name?.Replace("\"", "")}\"";
        protected static string JoinCommas(IEnumerable<string?> toJoin) => string.Join(", ", toJoin);
        protected static string JoinANDs(IEnumerable<string?> toJoin) => string.Join(" AND ", toJoin);
        #endregion
        #region Creation and Destruction
        public static Task<int> CreateDB(SQLDB sql)
        {
            StringBuilder command = new StringBuilder($"CREATE TABLE IF NOT EXISTS {TableName} (");


            //Regular Parameter Definitions
            for (int i = 0; i < RecordParameters.Length; i++)
            {
                ParameterInfo pi = RecordParameters[i];
                command.Append($"{FormatParameterInfo(pi)}, ");
            }
            //Primary Key Definitions - This works because I require at least 1 Primary Key
            command.Append($"PRIMARY KEY({GetEscapedParameterNames(PKs)})");

            foreach (string Line in ExtraLines)
            {
                command.Append($", {Line}");
            }
            foreach (string Line in ARA.SQLLines)
            {
                command.Append($", {Line}");
            }

            command.Append(");");
            return sql.ExecuteNonQuery(command.ToString());
        }
        public static Task Truncate(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"TRUNCATE {TableName}{(cascade ? " CASCADE" : "")};");
        public static Task Drop(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"DROP TABLE IF EXISTS {TableName}{(cascade ? " CASCADE" : "")};");
        #endregion
        #region Postgres Format Conversions
        public static T GetRecord(AutoClosingDataReader acdr)
        {
            object?[] recordItems = new object?[acdr.FieldCount];
            if (RecordParameters.Length != recordItems.Length) { throw new Exception("Record does not match data!"); }
            for (int i = 0; i < recordItems.Length; i++)
            {
                if (acdr.IsDBNull(i))
                {
                    if (!IsNullable(RecordParameters[i])) { throw new Exception("Record does not match data!"); }
                    recordItems[i] = null;
                    continue;
                }
                Type ParameterType = RecordParameters[i].ParameterType;
                ParameterType = Nullable.GetUnderlyingType(ParameterType) ?? ParameterType;
                if (ParameterType == typeof(byte))
                {
                    recordItems[i] = (byte)(short)acdr[i];
                    continue;
                }
                if (ParameterType == typeof(sbyte))
                {
                    recordItems[i] = (sbyte)(short)acdr[i];
                    continue;
                }
                if (ParameterType == typeof(char))
                {
                    recordItems[i] = ((string)acdr[i])[0];
                    continue;
                }
                if (ParameterType == typeof(ushort))
                {
                    recordItems[i] = (ushort)(short)acdr[i];
                    continue;
                }
                if (ParameterType == typeof(uint))
                {
                    recordItems[i] = (uint)(int)acdr[i];
                    continue;
                }
                if (ParameterType == typeof(ulong))
                {
                    recordItems[i] = (ulong)(long)acdr[i];
                    continue;
                }
                if (ParameterType.IsEnum)
                {
                    recordItems[i] = Enum.ToObject(ParameterType, Convert.ChangeType(acdr[i], Enum.GetUnderlyingType(ParameterType)));
                    continue;
                }
                recordItems[i] = acdr[i];
            }
            return (T)typeof(T).GetConstructors()[0].Invoke(recordItems);
        }
        public object?[] ToArray(SQLDB sql)
        {
            if (typeof(T) != GetType()) { throw new Exception("SQLRecord T Type must be the same as the class!"); }

            object?[] toReturn = new object[RecordParameters.Length];
            for (int i = 0; i < RecordParameters.Length; i++)
            {
                toReturn[i] = typeof(T).GetProperty(RecordParameters[i]?.Name ?? "")?.GetValue(this);
            }
            if (sql is PostgreSQL) { return ConvertToPostgresFriendlyParameters(toReturn); }
            throw new NotImplementedException();
        }
        private static object? ConvertToPostgresFriendlyParameter(object? parameter)
        {
            object? toReturn = parameter;
            Type? ParameterType = toReturn?.GetType();
            if (ParameterType == null) { return toReturn; }
            ParameterType = Nullable.GetUnderlyingType(ParameterType) ?? ParameterType;
            if (ParameterType.IsEnum)
            {
                ParameterType = Enum.GetUnderlyingType(ParameterType);
                toReturn = Convert.ChangeType(toReturn, ParameterType);
            }
            if (ParameterType == typeof(char))
            {
                char? val = (char?)toReturn;
                return val != null ? new string(val.Value, 1) : null;
            }
            if (ParameterType == typeof(ushort)) { return (short?)(ushort?)toReturn; }
            if (ParameterType == typeof(uint)) { return (int?)(uint?)toReturn; }
            if (ParameterType == typeof(ulong)) { return (long?)(ulong?)toReturn; }
            return toReturn;
        }
        private static object?[] ConvertToPostgresFriendlyParameters(object?[] parameters) => parameters.Select(x => ConvertToPostgresFriendlyParameter(x)).ToArray();
        #endregion

        #region SELECT
        public static Task<T?> SelectOne(SQLDB sql, Conditional conditional)
        {
            List<object> parameters = new List<object>();
            string condition = conditional.Build(sql, RecordParameters, ref parameters) + ";";
            return SelectOne(sql, condition, parameters.ToArray());
        }
        public static async Task<T?> SelectOne(SQLDB sql, string condition, params object?[] parameters)
        {
            if (sql is PostgreSQL) { parameters = ConvertToPostgresFriendlyParameters(parameters); }
            using (AutoClosingEnumerable<T> ace = await Select(sql, condition, parameters))
            {
                return ace.FirstOrDefault();
            }
        }
        public static async Task<AutoClosingEnumerable<T>> Select(SQLDB sql)
        {
            AutoClosingDataReader acdr = await sql.ExecuteReader($"SELECT * FROM {TableName};");
            return new AutoClosingEnumerable<T>(SelectHelper(acdr), acdr);
        }
        public static Task<AutoClosingEnumerable<T>> Select(SQLDB sql, Conditional conditional)
        {
            List<object> parameters = new List<object>();
            string condition = conditional.Build(sql, RecordParameters, ref parameters) + ";";
            return Select(sql, condition, parameters.ToArray());
        }
        public static async Task<AutoClosingEnumerable<T>> Select(SQLDB sql, string condition, params object?[] parameters)
        {
            if (sql is PostgreSQL) { parameters = ConvertToPostgresFriendlyParameters(parameters); }
            AutoClosingDataReader acdr = await sql.ExecuteReader($"SELECT * FROM {TableName} WHERE {condition};", parameters);
            return new AutoClosingEnumerable<T>(SelectHelper(acdr), acdr);
        }
        private static IEnumerable<T> SelectHelper(AutoClosingDataReader acdr)
        {
            while (acdr.Read()) { yield return GetRecord(acdr); }
        }

        #endregion
        #region DELETE
        public static Task<int> Delete(SQLDB sql, Conditional conditional)
        {
            List<object> parameters = new List<object>();
            string condition = conditional.Build(sql, RecordParameters, ref parameters) + ";";
            object?[] parameterarr = parameters.ToArray();
            if (sql is PostgreSQL) { parameterarr = ConvertToPostgresFriendlyParameters(parameterarr); }
            return Delete(sql, condition, parameterarr);
        }
        public static Task<int> Delete(SQLDB sql, string condition, params object?[] parameters)
        {
            if (sql is PostgreSQL) { parameters = ConvertToPostgresFriendlyParameters(parameters); }
            return sql.ExecuteNonQuery($"DELETE FROM {TableName} WHERE {condition};", parameters);
        }

        public Task<int> Delete(SQLDB sql) => sql.ExecuteNonQuery($"DELETE FROM {TableName} WHERE {JoinANDs(PKStrings)};", ToArray(sql));
        #endregion
        #region UPDATE INSERT and UPSERT
        public Task<int> Insert(SQLDB sql) => sql.ExecuteNonQuery(
                $"INSERT INTO {Escape(typeof(T).Name)} ({GetEscapedParameterNames(RecordParameters)}) " +
                $"VALUES({JoinCommas(ParameterNumbers)}) " +
                $"ON CONFLICT({GetEscapedParameterNames(PKs)}) DO NOTHING;", ToArray(sql));
        public Task<int> Update(SQLDB sql) => sql.ExecuteNonQuery(
                $"UPDATE {TableName} " +
                $"SET {JoinCommas(DataStrings)} " +
                $"WHERE {JoinANDs(PKStrings)};", ToArray(sql));
        public Task<int> Upsert(SQLDB sql) => sql.ExecuteNonQuery(
                $"INSERT INTO {TableName} ({GetEscapedParameterNames(RecordParameters)}) " +
                $"VALUES({JoinCommas(ParameterNumbers)}) " +
                $"ON CONFLICT({GetEscapedParameterNames(PKs)}) DO UPDATE " +
                $"SET {JoinCommas(DataStrings)};", ToArray(sql));
        #endregion
    }
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
        private static string EscapePostgres(string? name) => $"\"{name?.Replace("\"", "")}\"";
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
                    string a = EscapePostgres(current.ColumnName);

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
                            sb.Append($"{a}{notString} BETWEEN @{AddOrIndex(ref ParametersList,first)} AND @{AddOrIndex(ref ParametersList, last)}");
                            break;
                        #endregion
                        default:
                            #region many parameters
                            if (current.Condition is not IS.IN and not IS.NOT_IN) { throw new ArgumentOutOfRangeException("Too many parameters for comparison."); }
                            bool nullinlist = false;
                            List<object> NonNullParameters = new List<object>();
                            for(int i = 0; i < current.parameters.Length; i++)
                            {
                                object? toAdd = current.parameters[i];
                                if(toAdd is null)
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