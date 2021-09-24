using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CSL.SQL
{
    public class AutoClosingDataReader : IDataReader, IDisposable
    {
        private readonly IDataReader innerReader;
        private readonly IDisposable toDispose;

        public AutoClosingDataReader(IDataReader innerReader, IDisposable toDispose)
        {
            this.innerReader = innerReader;
            this.toDispose = toDispose;
        }

        public object this[int i] => innerReader[i];
        public object this[string name] => innerReader[name];
        public int Depth => innerReader.Depth;
        public bool IsClosed => innerReader.IsClosed;
        public int RecordsAffected => innerReader.RecordsAffected;
        public int FieldCount => innerReader.FieldCount;
        public void Close() => innerReader.Close();
        public bool GetBoolean(int i) => innerReader.GetBoolean(i);
        public byte GetByte(int i) => innerReader.GetByte(i);
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => innerReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        public char GetChar(int i) => innerReader.GetChar(i);
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => innerReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        public IDataReader GetData(int i) => innerReader.GetData(i);
        public string GetDataTypeName(int i) => innerReader.GetDataTypeName(i);
        public DateTime GetDateTime(int i) => innerReader.GetDateTime(i);
        public decimal GetDecimal(int i) => innerReader.GetDecimal(i);
        public double GetDouble(int i) => innerReader.GetDouble(i);
        public Type GetFieldType(int i) => innerReader.GetFieldType(i);
        public float GetFloat(int i) => innerReader.GetFloat(i);
        public Guid GetGuid(int i) => innerReader.GetGuid(i);
        public short GetInt16(int i) => innerReader.GetInt16(i);
        public int GetInt32(int i) => innerReader.GetInt32(i);
        public long GetInt64(int i) => innerReader.GetInt64(i);
        public string GetName(int i) => innerReader.GetName(i);
        public int GetOrdinal(string name) => innerReader.GetOrdinal(name);
        public DataTable GetSchemaTable() => innerReader.GetSchemaTable();
        public string GetString(int i) => innerReader.GetString(i);
        public object GetValue(int i) => innerReader.GetValue(i);
        public int GetValues(object[] values) => innerReader.GetValues(values);
        public bool IsDBNull(int i) => innerReader.IsDBNull(i);
        public bool NextResult() => innerReader.NextResult();
        public bool Read() => innerReader.Read();

        public DataTable CreateDataTableFromSchema(string? tableName = null)
        {
            DataTable SchemaTable = GetSchemaTable();
            DataTable toReturn = new DataTable(tableName);
            List<DataColumn> PKList = new List<DataColumn>();
            for (int i = 0; i < SchemaTable.Rows.Count; i++)
            {
                DataColumn toAdd = new DataColumn()
                {
                    ColumnName = Common.NameParser(SchemaTable.Rows[i].Get<string>("ColumnName")),
                    DataType = SchemaTable.Rows[i].Get<Type>("DataType"),
                    Unique = SchemaTable.Rows[i].Get<bool?>("IsUnique") == true,
                    AutoIncrement = SchemaTable.Rows[i].Get<bool?>("IsAutoIncrement") == true,
                    AllowDBNull = SchemaTable.Rows[i].Get<bool?>("AllowDBNull") != false,
                };
                if ((bool)SchemaTable.Rows[i]["IsKey"])
                {
                    PKList.Add(toAdd);
                }
                toReturn.Columns.Add(toAdd);
            }
            toReturn.PrimaryKey = PKList.ToArray();
            return toReturn;
        }

        public DataTable DumpReader(string? tableName = null, bool leaveOpen = false)
        {
            if (!leaveOpen)
            {
                using (this)
                {
                    return DumpReader(tableName, true);
                }
            }
            DataTable toReturn = CreateDataTableFromSchema(tableName);
            object[] toAdd = new object[FieldCount];
            while (Read())
            {
                for (int i = 0; i < FieldCount; i++)
                {
                    toAdd[i] = this[i];
                }
                toReturn.Rows.Add(toAdd);
            }
            return toReturn;
        }

        public void Dispose()
        {
            innerReader.Dispose();
            toDispose.Dispose();
        }
    }
}
