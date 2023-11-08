using System;
using static CSL.DependencyInjection;
using System.Data.Common;
using System.Reflection;
using static CSL.Helpers.Generics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public class Sqlite : SQLDB
    {
        private static DbConnection CreateConnection(string Filename, SqliteOpenMode Mode, SqliteCacheMode Cache)
        {
            ISqliteConnectionStringBuilder csb = CreateISqliteConnectionStringBuilder();
            csb.DataSource = Filename;
            csb.Mode = Mode;
            csb.Cache = Cache;
            return CreateSqliteConnection(csb.ConnectionString);
        }

        public Sqlite(string Filename, SqliteOpenMode Mode = SqliteOpenMode.ReadWriteCreate, SqliteCacheMode Cache = SqliteCacheMode.Default) : base(CreateConnection(Filename,Mode,Cache))
        {
            currentTransaction = null;
            InternalConnection.Open();
        }
        public Sqlite(DbConnection connection) : base(connection)
        {
            currentTransaction = null;
            InternalConnection.Open();
        }
        #region Abstract Implementations
        public override object? ConvertToFriendlyParameter(object? parameter)
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
            //if (ParameterType == typeof(char))
            //{
            //    char? val = (char?)toReturn;
            //    return val != null ? new string(val.Value, 1) : null;
            //}
            //if (ParameterType == typeof(ushort)) { return (short?)(ushort?)toReturn; }
            //if (ParameterType == typeof(uint)) { return (int?)(uint?)toReturn; }
            if (ParameterType == typeof(decimal)) { return Helpers.Generics.ToStringRT(toReturn); }
            if (ParameterType == typeof(DateTime)) { return Helpers.Generics.ToStringRT(toReturn); }
            if (ParameterType == typeof(ulong)) { return (long?)(ulong?)toReturn; }
            return toReturn;
        }

        public override object? ConvertFromFriendlyParameter(object? parameter, ParameterInfo pi)
        {
            if (parameter is null or DBNull)
            {
                if (!IsNullable(pi)) { throw new Exception($"{pi.Name} ParameterInfo does not match parameter!"); }
                return null;
            }
            Type ParameterType = pi.ParameterType;
            ParameterType = Nullable.GetUnderlyingType(ParameterType) ?? ParameterType;
            if (ParameterType == typeof(byte)) { return (byte)(long)parameter; }
            if (ParameterType == typeof(sbyte)) { return (sbyte)(long)parameter; }
            if (ParameterType == typeof(char)) { return ((string)parameter)[0]; }
            if (ParameterType == typeof(short)) { return (short)(long)parameter; }
            if (ParameterType == typeof(ushort)) { return (ushort)(long)parameter; }
            if (ParameterType == typeof(int)) { return (int)(long)parameter; }
            if (ParameterType == typeof(uint)) { return (uint)(long)parameter; }
            if (ParameterType == typeof(ulong)) { return (ulong)(long)parameter; }
            if (ParameterType == typeof(Guid)) { return Guid.Parse((string)parameter); }
            if (ParameterType == typeof(bool)) { return (long)parameter != 0L; }
            if (ParameterType == typeof(float)) { return (float)(double)parameter; }
            if (ParameterType == typeof(decimal))
            {
                if(Helpers.Generics.TryParse(parameter.ToStringRT(), out decimal? toReturn))
                {
                    return toReturn;
                }
                return null;
            }
            if (ParameterType == typeof(DateTime))
            {
                if (Helpers.Generics.TryParse(parameter.ToStringRT(), out DateTime? toReturn))
                {
                    return toReturn;
                }
                return null;
            }
            if (ParameterType.IsEnum) { return Enum.ToObject(ParameterType, Convert.ChangeType(parameter, Enum.GetUnderlyingType(ParameterType))); }
            return parameter;
        }
        private static readonly Dictionary<Type, string> SQLTypes = new Dictionary<Type, string>()
    {
        {typeof(bool),      "INTEGER"},
        {typeof(bool?),     "INTEGER"},
        {typeof(sbyte),     "INTEGER"},
        {typeof(sbyte?),    "INTEGER"},
        {typeof(byte),      "INTEGER"},
        {typeof(byte?),     "INTEGER"},
        {typeof(char),      "TEXT"},
        {typeof(char?),     "TEXT"},
        {typeof(short),     "INTEGER"},
        {typeof(short?),    "INTEGER"},
        {typeof(ushort),    "INTEGER"},
        {typeof(ushort?),   "INTEGER"},
        {typeof(int),       "INTEGER"},
        {typeof(int?),      "INTEGER"},
        {typeof(uint),      "INTEGER"},
        {typeof(uint?),     "INTEGER"},
        {typeof(long),      "INTEGER"},
        {typeof(long?),     "INTEGER"},
        {typeof(ulong),     "INTEGER"},
        {typeof(ulong?),    "INTEGER"},
        {typeof(float),     "REAL"},
        {typeof(float?),    "REAL"},
        {typeof(double),    "REAL"},
        {typeof(double?),   "REAL"},
        {typeof(decimal),   "TEXT"},
        {typeof(decimal?),  "TEXT"},
        {typeof(Guid),      "TEXT"},
        {typeof(Guid?),     "TEXT"},
        {typeof(DateTime),  "TEXT"},
        {typeof(DateTime?), "TEXT"},
        {typeof(string),    "TEXT"},
        {typeof(byte[]),    "BLOB" }
    };
        public override string? GetSQLType(Type t) => SQLTypes.ContainsKey(t) ? SQLTypes[t] : null;
        public override Task Truncate(string TableName, bool cascade = false)
        {
            if(cascade) { throw new NotSupportedException("Sqlite only allows cascade as part of the foreign key definition."); }
            return ExecuteNonQuery($"DELETE FROM {TableName};");
        }
        #endregion
    }
}
