using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CSL.SQL
{
    public static class Common
    {
        public static string NameParser(string input)
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
        public static T Get<T>(this IDataReader reader, string item)
        {
            object toReturn = reader[item];
            return !DBNull.Value.Equals(toReturn) ? (T)toReturn : default;
        }
        public static T Get<T>(this IDataReader reader, int index)
        {
            object toReturn = reader[index];
            return !DBNull.Value.Equals(toReturn) ? (T)toReturn : default;
        }
        public static bool IsDBNull(this IDataReader reader, string item) => DBNull.Value.Equals(reader[item]);
        public static T Get<T>(this DataRow row, string item)
        {
            object toReturn = row[item];
            return !DBNull.Value.Equals(toReturn) ? (T)toReturn : default;
        }
        public static T Get<T>(this DataRow row, int index)
        {
            object toReturn = row[index];
            return !DBNull.Value.Equals(toReturn) ? (T)toReturn : default;
        }

        public static ulong ToUlong(this long item)
        {
            return unchecked((ulong)(item - long.MinValue));
        }
        public static long ToLong(this ulong item)
        {
            return unchecked((long)item + long.MinValue);
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
