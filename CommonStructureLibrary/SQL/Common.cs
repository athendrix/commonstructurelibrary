using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace CSL.SQL
{
    public static class Common
    {
        public static string NameParser(string? input)
        {
            if (input == null)
            {
                return "GUID_" + Guid.Empty.ToString().Replace('-', '_');
            }
            //string parsedname = input.Trim().Replace(' ', '_');
            string parsedname = input.Trim();
            char[] AcceptableFirstChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();
            char[] AcceptableChars = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_ ".ToCharArray();
            while (parsedname != "" && !AcceptableFirstChars.Contains(parsedname.ToCharArray()[0]))
            {
                parsedname = parsedname.Substring(1);
            }
            for (int i = 1; i < parsedname.Length; i++)
            {
                if (!AcceptableChars.Contains(parsedname.ToCharArray()[i]))
                {
                    parsedname = parsedname.Remove(i--, 1);
                }
            }
            if (parsedname == "")
            {
                using (MD5 md5Hasher = MD5.Create())
                {
                    Guid toReturn = new Guid(md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input)));
                    return "GUID_" + toReturn.ToString().Replace("-", "_");
                }
            }
            return parsedname;
        }
        //Extentions
        public static T? Get<T>(this IDataReader reader, string item)
        {
            object toReturn = reader[item];
            if(DBNull.Value.Equals(toReturn))
            {
                Debug.Assert(default(T?) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
                return default(T?);//null
            }
            return (T)toReturn;
        }
        public static T? Get<T>(this IDataReader reader, int index)
        {
            object toReturn = reader[index];
            if (DBNull.Value.Equals(toReturn))
            {
                Debug.Assert(default(T?) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
                return default(T?);//null
            }
            return (T)toReturn;
        }
        public static bool IsDBNull(this IDataReader reader, string item) => DBNull.Value.Equals(reader[item]);
        public static T? Get<T>(this DataRow row, string item)
        {
            object toReturn = row[item];
            if (DBNull.Value.Equals(toReturn))
            {
                Debug.Assert(default(T?) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
                return default(T?);//null
            }
            return (T)toReturn;
        }
        public static T? Get<T>(this DataRow row, int index)
        {
            object toReturn = row[index];
            if (DBNull.Value.Equals(toReturn))
            {
                Debug.Assert(default(T?) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
                return default(T?);//null
            }
            return (T)toReturn;
        }

        [Obsolete("This method is obsolete. You can just cast from a long to a ulong and back safely.")]
        public static ulong ToUlong(this long item) => (ulong)item;
        [Obsolete("This method is obsolete. You can just cast from a ulong to a long and back safely.")]
        public static long ToLong(this ulong item) => (long)item;

        public static void WriteStruct<T>(this BinaryWriter bw, T value) where T : struct => bw.Write(value.ToByteArray());
        public static byte[] ToByteArray<T>(this T value) where T : struct => MemoryMarshal.AsBytes<T>(new T[] { value }).ToArray();
        public static T ReadStruct<T>(this BinaryReader br) where T : struct
        {
            int length = MemoryMarshal.AsBytes<T>(new T[] { default(T) }).Length;
            byte[] read = br.ReadBytes(length);
            if (read.Length < length)
            {
                byte[] temp = new byte[length];
                Array.Copy(read, temp, read.Length);
                read = temp;
            }
            return read.ToStruct<T>();
        }
        public static T ToStruct<T>(this byte[] data) where T : struct => MemoryMarshal.Cast<byte, T>(data)[0];
        public static void WriteMany(this BinaryWriter bw, params object[] items)
        {
            foreach(object o in items)
            {
                if (o is bool bl) { bw.Write(bl); continue; }
                if (o is byte bt) { bw.Write(bt); continue; }
                //if (o is byte[] buffer) { bw.Write(buffer); continue; } //not wise, unless you encode the length first like string. We don't control the reading so we can't reliably do this.
                if (o is byte[] buffer) { throw new ArgumentException("Cannot write byte[] because we won't know the length on retrieval! We recomend writing the length and then the byte[] manually."); }
                if (o is char ch) { bw.Write(ch); continue; }
                if (o is char[] chars) { bw.Write(new string(chars)); continue; } //turn it into a string so it works better (writing length first)
                if (o is decimal dec) { bw.Write(dec); continue; }
                if (o is double d) { bw.Write(d); continue; }
                if (o is float f) { bw.Write(f); continue; }
                if (o is int i) { bw.Write(i); continue; }
                if (o is long l) { bw.Write(l); continue; }
                if (o is sbyte sb) { bw.Write(sb); continue; }
                if (o is short sh) { bw.Write(sh); continue; }
                if (o is string s) { bw.Write(s); continue; }
                if (o is uint ui) { bw.Write(ui); continue; }
                if (o is ulong ul) { bw.Write(ul); continue; }
                if (o is ushort us) { bw.Write(us); continue; }
                if (o is Guid id) { bw.WriteStruct<Guid>(id); continue; }
                if (o is DateTime dt) { bw.WriteStruct<DateTime>(dt); continue; }
                if (o is TimeSpan ts) { bw.WriteStruct<TimeSpan>(ts); continue; }
                if (o is IBinaryWritable ibw) { bw.Write(ibw.ToByteArray()); continue; }
                throw new ArgumentException("object " + o.ToString() + " is an unknown type, and cannot be converted to a byte[]");
            }
        }

        public static Guid HashToGuid(this byte[] data) => HashToGuid(data, out Guid _);
        public static Guid HashToGuid(this byte[] data, out Guid secondaryGuid)
        {
            using (SHA256 sha = SHA256.Create())
            {
                Span<byte> shahash = sha.ComputeHash(data);
                secondaryGuid = new Guid(shahash.Slice(16, 16).ToArray());
                return new Guid(shahash.Slice(0, 16).ToArray());
            }
        }

        //public static KeyValuePair<string, object> BindTo(this string key, object value)
        //{
        //    return new KeyValuePair<string, object>(key, value);
        //}
        //public static string[] ValidSQLFunctions = new string[] { "MIN", "MAX", "COUNT", "AVG", "SUM", "UPPER", "LOWER" };
        //public static string ColumnParser(string input, string preEscape, string postEscape, bool allowAlias = true)
        //{
        //    string tosplit = input;
        //    string[] aliasarray = input.Split(new string[] { " AS ", " as ", " As ", " aS " }, StringSplitOptions.RemoveEmptyEntries);
        //    string alias = null;
        //    if (allowAlias && aliasarray.Length > 1)
        //    {
        //        alias = NameParser(aliasarray[1]);
        //        tosplit = aliasarray[0];
        //    }
        //    string[] tokens = tosplit.Split("()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    if (tokens.Length > 1)
        //    {
        //        string sqlFunction = tokens[0].ToUpper().Trim();
        //        if (ValidSQLFunctions.Contains(sqlFunction))
        //        {
        //            return sqlFunction + "(" + preEscape + NameParser(tokens[1]) + postEscape + ")" + (alias == null ? "" : " AS " + preEscape + alias + postEscape);
        //        }
        //    }
        //    return preEscape + NameParser(tosplit) + postEscape + (alias == null ? "" : " AS " + preEscape + alias + postEscape);

        //}
    }
}
