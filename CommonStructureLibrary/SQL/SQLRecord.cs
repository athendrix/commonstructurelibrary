using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq;

using static CSL.Helpers.Generics;
using System.Text.RegularExpressions;

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
        public readonly int? Group;
        /// <summary>
        /// Lets you specify groups of Unique keys. Unique keys with the same Group are considered unique in aggregate.
        /// </summary>
        /// <param name="Group">The aggregate group this Unique key is part of.</param>
        public UniqueAttribute() => this.Group = null;
        public UniqueAttribute(int Group) => this.Group = Group;
        
    }
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class CheckAttribute : Attribute
    {
        public readonly string CheckString;
        /// <summary>
        /// Lets you specify a comparison that any parameters have to satisfy. Check comparisons will be checked on Insert or Update.
        /// </summary>
        /// <param name="CheckString">The SQL to use for a check. For example if I wanted to make sure a date happened after Dec 31, 1999, I'd put "> '1999-12-31'"</param>
        public CheckAttribute(string CheckString) => this.CheckString = CheckString;
    }
    public record FKGroup(string Table, int Group);
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class FKAttribute : Attribute
    {
        public readonly string ForeignTable;
        public readonly string ForeignKey;
        public readonly int? Group;
        public FKGroup? Grouping => Group is null ? null : new FKGroup(ForeignTable, Group.Value);
        /// <summary>
        /// Lets you specify a foreign key relationship.
        /// </summary>
        /// <param name="Table">The Table the Foreign Key is in.</param>
        /// <param name="Key">The Foreign Key</param>
        public FKAttribute(string Table, string Key)
        {
            this.Group = null;
            ForeignTable = Table;
            ForeignKey = Key;
        }
        /// <summary>
        /// Lets you specify a foreign key relationship.
        /// </summary>
        /// <param name="Table">The Table the Foreign Key is in.</param>
        /// <param name="Key">The Foreign Key</param>
        /// <param name="Group">A Group for a Composite Foreign Key</param>
        public FKAttribute(string Table, string Key, int Group)
        {
            this.Group = Group;
            ForeignTable = Table;
            ForeignKey = Key;
        }

    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TableNameAttribute : Attribute
    {
        public readonly string TableName;
        /// <summary>
        /// Allows overriding the default TableName. The default TableName is the name of the record itself.
        /// </summary>
        /// <param name="TableName">The actual name of the table in the SQL Database</param>
        public TableNameAttribute(string TableName) => this.TableName = Common.Escape(TableName);
    }
    #endregion
    
    public interface ISQLRecord
    {
        public Task<int> Delete(SQLDB sql);
        public Task<int> Insert(SQLDB sql);
        public Task<int> Update(SQLDB sql);
        public Task<int> Upsert(SQLDB sql);
        public object?[] ToArray();
    }

    [SQLRecord(1)]
    public abstract record SQLRecord<T>() :CSLRecord<T>, ISQLRecord where T : SQLRecord<T>
    {

        private static string GetSQLType(SQLDB sql, Type type)
        {
            string? toReturn = sql.GetSQLType(type);
            if (toReturn != null) { return toReturn; }
            if (type.IsEnum) { return GetSQLType(sql, type.GetEnumUnderlyingType()); }
            Type? UnderlyingNullableType = Nullable.GetUnderlyingType(type);
            if (UnderlyingNullableType != null) { return GetSQLType(sql, UnderlyingNullableType); }
            //TODO: SQLRecords? Maybe something with Foreign Keys?
            throw new ArgumentException($"Type \"{type.Name}\" is not a valid SQL Type!");
            //return "<FIXME>";
        }
        #region Helper Properties
        public static readonly string TableName = TNA?.TableName ?? Common.Escape(typeof(T).Name);
        private static TableNameAttribute? TNA => typeof(T).GetCustomAttributes(false).SelectMany(x => x is TableNameAttribute r ? new TableNameAttribute[] { r } : new TableNameAttribute[0]).FirstOrDefault();
        protected static readonly SQLRecordAttribute ARA = typeof(T).GetCustomAttributes(true).SelectMany(x => x is SQLRecordAttribute r ? new SQLRecordAttribute[] { r } : new SQLRecordAttribute[0]).First();

        protected static readonly RecordParameter[] PKs = RecordParameters.Take(ARA.PrimaryKeys).ToArray();
        protected static readonly RecordParameter[] Datas = RecordParameters.Skip(ARA.PrimaryKeys).ToArray();
        protected static readonly string[] PKStrings = PKs.Select(x => $"{Common.Escape(x.Name)} = @{x.Position}").ToArray();
        protected static readonly string[] DataStrings = Datas.Select(x => $"{Common.Escape(x.Name)} = @{x.Position}").ToArray();
        protected static readonly string[] ExtraLines = GetExtraLines(RecordParameters.SelectMany(x => x.Attributes.Select(y => new Tuple<RecordParameter, Attribute>(x, y))).GroupBy(z => z.Item2.GetType()).ToArray());
        protected static readonly string[] ParameterNumbers = Enumerable.Range(0, RecordParameters.Length).Select(i => $"@{i}").ToArray();
        #endregion
        #region Function Helpers
        private static string[] GetExtraLines(IGrouping<Type, Tuple<RecordParameter, Attribute>>[] attributes)
        {
            List<string> toReturn = new List<string>();
            foreach (IGrouping<Type, Tuple<RecordParameter, Attribute>> group in attributes)
            {
                if (group.Key == typeof(UniqueAttribute))
                {
                    foreach (IGrouping<int?, Tuple<RecordParameter, Attribute>> innergroup in group.GroupBy(x => ((UniqueAttribute)x.Item2).Group))
                    {
                        if (innergroup.Key == null)
                        {
                            foreach (RecordParameter rp in innergroup.Select(x => x.Item1))
                            {
                                toReturn.Add($"UNIQUE({Common.Escape(rp.Name)})");
                            }
                        }
                        else
                        {
                            toReturn.Add($"UNIQUE({GetEscapedParameterNames(innergroup.Select(x => x.Item1))})");
                        }
                    }
                }
                if (group.Key == typeof(CheckAttribute))
                {
                    toReturn.AddRange(group.Select(x => $"CHECK({Common.Escape(x.Item1.Name)} {((CheckAttribute)x.Item2).CheckString})"));
                }
                if (group.Key == typeof(FKAttribute))
                {
                    foreach (IGrouping<FKGroup?, Tuple<RecordParameter, Attribute>> innergroup in group.GroupBy(x => ((FKAttribute)x.Item2).Grouping))
                    {
                        if (innergroup.Key == null)
                        {
                            toReturn.AddRange(innergroup.Select(x =>
                            {
                                FKAttribute fk = (FKAttribute)x.Item2;
                                return $"FOREIGN KEY({Common.Escape(x.Item1.Name)}) REFERENCES {Common.Escape(fk.ForeignTable)}({Common.Escape(fk.ForeignKey)})";
                            }));
                        }
                        else
                        {
                            toReturn.Add($"FOREIGN KEY({GetEscapedParameterNames(innergroup.Select(x => x.Item1))}) " +
                                $"REFERENCES {Common.Escape(innergroup.Key.Table)}({string.Join(", ", innergroup.Select(x => Common.Escape(((FKAttribute)x.Item2).ForeignKey)))})");
                        }
                    }
                }
            }
            return toReturn.ToArray();
        }
        protected static string FormatRecordParameters(SQLDB sql, IEnumerable<RecordParameter> rps) => string.Join(", ", rps.Select(x => FormatRecordParameter(sql, x)));
        protected static string GetEscapedParameterNames(IEnumerable<RecordParameter> rps) => string.Join(", ", rps.Select(x => Common.Escape(x.Name)));
        protected static string FormatRecordParameter(SQLDB sql, RecordParameter rp) => $"{Common.Escape(rp.Name)} {GetSQLType(sql, rp.Type)}{(rp.Nullable ? "" : " NOT NULL")}";
        protected static string JoinCommas(IEnumerable<string?> toJoin) => string.Join(", ", toJoin);
        protected static string JoinANDs(IEnumerable<string?> toJoin) => string.Join(" AND ", toJoin);
        #endregion
        #region Creation and Destruction
        public static string CreateDBSQLCode(SQLDB sql)
        {

                StringBuilder command = new StringBuilder($"CREATE TABLE IF NOT EXISTS {TableName} (");
                //Regular Parameter Definitions
                for (int i = 0; i < RecordParameters.Length; i++)
                {
                    RecordParameter rp = RecordParameters[i];
                    command.Append($"{FormatRecordParameter(sql, rp)}, ");
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
                return command.ToString();
            
        }

        public static Task<int> CreateDB(SQLDB sql) => sql.ExecuteNonQuery(CreateDBSQLCode(sql));
        public static Task Truncate(SQLDB sql, bool cascade = false) => sql.Truncate(TableName, cascade);
        public static Task Drop(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"DROP TABLE IF EXISTS {TableName}{(cascade ? " CASCADE" : "")};");
        #endregion

        #region SELECT
        #region SelectOne
        public static async Task<ISQLRecord?> GenericSelectOne(SQLDB sql, Conditional conditional) => await SelectOne(sql, conditional);
        public static async Task<T?> SelectOne(SQLDB sql, Conditional conditional)
        {
            using (AutoClosingEnumerable<T> ace = await Select(sql, conditional))
            {
                return ace.FirstOrDefault();
            }
        }
        public static async Task<ISQLRecord?> GenericSelectOne(SQLDB sql, string condition, params object?[] parameters) => await SelectOneBase(sql, condition, parameters);
        public static Task<T?> SelectOne(SQLDB sql, string condition, params object?[] parameters) => SelectOneBase(sql, condition, parameters); 
        private static async Task<T?> SelectOneBase(SQLDB sql, string condition, object?[] parameters)
        {
            using (AutoClosingEnumerable<T> ace = await SelectBase(sql, condition, parameters))
            {
                return ace.FirstOrDefault();
            }
        }
        #endregion
        #region Select
        public static async Task<AutoClosingEnumerable<ISQLRecord>> GenericSelect(SQLDB sql)
        {
            AutoClosingEnumerable<T> toReturn = await Select(sql);
            return new AutoClosingEnumerable<ISQLRecord>(toReturn, toReturn);
        }
        public static async Task<AutoClosingEnumerable<T>> Select(SQLDB sql)
        {
            AutoClosingDataReader acdr = await sql.ExecuteReader($"SELECT {GetEscapedParameterNames(RecordParameters)} FROM {TableName};");
            return acdr.ReadRecords<T>(sql);
        }
        public static async Task<AutoClosingEnumerable<ISQLRecord>> GenericSelect(SQLDB sql, Conditional conditional)
        {
            AutoClosingEnumerable<T> toReturn = await Select(sql, conditional);
            return new AutoClosingEnumerable<ISQLRecord>(toReturn, toReturn);
        }
        public static Task<AutoClosingEnumerable<T>> Select(SQLDB sql, Conditional conditional)
        {
            List<object> parameters = new List<object>();
            string condition = conditional.Build(sql, RecordParameters, ref parameters, true);
            return SelectBase(sql, condition, parameters.ToArray());
        }
        public static async Task<AutoClosingEnumerable<ISQLRecord>> GenericSelect(SQLDB sql, string condition, params object?[] parameters)
        {
            AutoClosingEnumerable<T> toReturn = await SelectBase(sql, condition, parameters);
            return new AutoClosingEnumerable<ISQLRecord>(toReturn, toReturn);
        }
        public static Task<AutoClosingEnumerable<T>> Select(SQLDB sql, string condition, params object?[] parameters) => SelectBase(sql, condition, parameters);
        private static async Task<AutoClosingEnumerable<T>> SelectBase(SQLDB sql, string condition, object?[] parameters)
        {
            AutoClosingDataReader acdr = await sql.ExecuteReader($"SELECT {GetEscapedParameterNames(RecordParameters)} FROM {TableName} WHERE {condition};", sql.ConvertToFriendlyParameters(parameters));
            return acdr.ReadRecords<T>(sql);
        }
        #endregion
        #region SelectArray
        public static async Task<ISQLRecord[]> GenericSelectArray(SQLDB sql) => await SelectArray(sql);
        public static async Task<T[]> SelectArray(SQLDB sql)
        {
            using (AutoClosingEnumerable<T> ace = await Select(sql))
            {
                return ace.ToArray();
            }
        }
        public static async Task<ISQLRecord[]> GenericSelectArray(SQLDB sql, Conditional conditional) => await SelectArray(sql, conditional);
        public static async Task<T[]> SelectArray(SQLDB sql, Conditional conditional)
        {
            using (AutoClosingEnumerable<T> ace = await Select(sql, conditional))
            {
                return ace.ToArray();
            }
        }
        public static async Task<ISQLRecord[]> GenericSelectArray(SQLDB sql, string condition, params object?[] parameters) => await SelectArrayBase(sql, condition, parameters);
        public static Task<T[]> SelectArray(SQLDB sql, string condition, params object?[] parameters) => SelectArrayBase(sql, condition, parameters);
        private static async Task<T[]> SelectArrayBase(SQLDB sql, string condition, object?[] parameters)
        {
            using (AutoClosingEnumerable<T> ace = await SelectBase(sql, condition, parameters))
            {
                return ace.ToArray();
            }
        }
        #endregion
        #endregion
        #region DELETE
        public static Task<int> Delete(SQLDB sql, Conditional conditional)
        {
            List<object> parameters = new List<object>();
            string condition = conditional.Build(sql, RecordParameters, ref parameters, true) + ";";
            object?[] parameterarr = parameters.ToArray();
            return Delete(sql, condition, parameterarr);
        }
        public static Task<int> Delete(SQLDB sql, string condition, params object?[] parameters) => sql.ExecuteNonQuery($"DELETE FROM {TableName} WHERE {condition};", sql.ConvertToFriendlyParameters(parameters));

        public Task<int> Delete(SQLDB sql) => sql.ExecuteNonQuery($"DELETE FROM {TableName} WHERE {JoinANDs(PKStrings)};", sql.ConvertToFriendlyParameters(ToArray()));
        #endregion
        #region UPDATE INSERT and UPSERT
        public Task<int> Insert(SQLDB sql) => sql.ExecuteNonQuery(
                $"INSERT INTO {TableName} ({GetEscapedParameterNames(RecordParameters)}) " +
                $"VALUES({JoinCommas(ParameterNumbers)}) " +
                $"ON CONFLICT({GetEscapedParameterNames(PKs)}) DO NOTHING;", sql.ConvertToFriendlyParameters(ToArray()));
        public Task<int> Update(SQLDB sql) => sql.ExecuteNonQuery(
                $"UPDATE {TableName} " +
                $"SET {JoinCommas(DataStrings)} " +
                $"WHERE {JoinANDs(PKStrings)};", sql.ConvertToFriendlyParameters(ToArray()));
        public Task<int> Upsert(SQLDB sql) => sql.ExecuteNonQuery(
                $"INSERT INTO {TableName} ({GetEscapedParameterNames(RecordParameters)}) " +
                $"VALUES({JoinCommas(ParameterNumbers)}) " +
                $"ON CONFLICT({GetEscapedParameterNames(PKs)}) DO UPDATE " +
                $"SET {JoinCommas(DataStrings)};", sql.ConvertToFriendlyParameters(ToArray()));
        #endregion
    }
}