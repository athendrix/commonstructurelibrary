using System;
using System.Collections.Generic;
//#if !BRIDGE
//using System.Data;
//#endif
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSL.Data
{
    public static class CSV
    {
        /// <summary>
        /// Lazily reads a CSV file line by line, using a designated a separator, escape character, and list of values to consider null, into an IEnumerable of string arrays.
        /// <para>Headers are treated like any other row, and beyond handling escape characters and new lines, no additional formatting is performed.</para>
        /// <para>The defaults are appropriate for most csv files, but if you have CSVs that use alternative separators, escape characters, or null values, you'll need to specify them.</para>
        /// </summary>
        /// <param name="CSVString">CSV in string form to use as input.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="separator">Character to separate on. Usually a comma, sometimes a tab.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `</param>
        /// <returns>An IEnumerable of string arrays each containing a single line of the parsed data.</returns>
        /// <exception cref="ArgumentException">Will throw an ArgumentException if the separator and escapechar are the same.</exception>
        public static IEnumerable<string?[]> ReadCSVRawFromString(string CSVString, string[]? nullValues = null, char separator = ',', char escapechar = '"')
        {
            using (TextReader reader = new StringReader(CSVString))
            {
                foreach(string?[] row in reader.ReadCSVRaw(nullValues,separator,escapechar))
                {
                    yield return row;
                }
            }
            yield break;
        }
        /// <summary>
        /// Lazily reads a CSV file line by line, using a designated a separator, escape character, and list of values to consider null, into an IEnumerable of string arrays.
        /// <para>Headers are treated like any other row, and beyond handling escape characters and new lines, no additional formatting is performed.</para>
        /// <para>The defaults are appropriate for most csv files, but if you have CSVs that use alternative separators, escape characters, or null values, you'll need to specify them.</para>
        /// </summary>
        /// <param name="CSVFile">File path to CSV file to use as input.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="separator">Character to separate on. Usually a comma, sometimes a tab.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `</param>
        /// <returns>An IEnumerable of string arrays each containing a single line of the parsed data.</returns>
        /// <exception cref="ArgumentException">Will throw an ArgumentException if the separator and escapechar are the same.</exception>
        public static IEnumerable<string?[]> ReadCSVRawFromFile(string CSVFile, string[]? nullValues = null, char separator = ',', char escapechar = '"')
        {
            using (TextReader reader = new StreamReader(CSVFile))
            {
                foreach (string?[] row in reader.ReadCSVRaw(nullValues, separator, escapechar))
                {
                    yield return row;
                }
            }
            yield break;
        }
        /// <summary>
        /// Lazily reads a CSV file line by line, using a designated a separator, escape character, and list of values to consider null, into an IEnumerable of string arrays.
        /// <para>Headers are treated like any other row, and beyond handling escape characters and new lines, no additional formatting is performed.</para>
        /// <para>The defaults are appropriate for most csv files, but if you have CSVs that use alternative separators, escape characters, or null values, you'll need to specify them.</para>
        /// </summary>
        /// <param name="input">TextReader to use as input.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="separator">Character to separate on. Usually a comma, sometimes a tab.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `</param>
        /// <returns>An IEnumerable of string arrays each containing a single line of the parsed data.</returns>
        /// <exception cref="ArgumentException">Will throw an ArgumentException if the separator and escapechar are the same.</exception>
        public static IEnumerable<string?[]> ReadCSVRaw(this TextReader input, string[]? nullValues = null, char separator = ',', char escapechar = '"')
        {
            if (nullValues == null) { nullValues = new string[] { "" }; }//We let users specify, because some programs (Looking at you R with your NA values) have custom null values.
            if (escapechar == separator) { throw new ArgumentException("The separator and the escape character cannot be the same!"); }
            StringBuilder sb = new StringBuilder();
            List<string?> toReturn = new List<string?>();
            bool escaped = false;
            int c;
            while ((c = input.Read()) != -1)
            {
                char cc = (char)c;
                if (cc == escapechar)
                {
                    sb.Append(cc);
                    escaped = !escaped;
                    continue;
                }
                if (!escaped && (cc == separator || cc == '\n'))
                {
                    string protoval = sb.ToString();
                    if (cc == '\n' && protoval.EndsWith("\r")) { protoval = protoval.Substring(0, protoval.Length - 1); }
                    if (nullValues.Contains(protoval))
                    {
                        toReturn.Add(null);
                    }
                    else
                    {
                        toReturn.Add(protoval.Trim().UnescapeString(escapechar));
                    }
                    sb.Clear();
                    if (cc == '\n')
                    {
                        yield return toReturn.ToArray();
                        toReturn.Clear();
                    }
                    continue;
                }
                else
                {
                    sb.Append(cc);
                }
            }
            if (toReturn.Count > 0 || sb.Length > 0)
            {
                string protoval = sb.ToString();
                if (protoval.EndsWith("\r")) { protoval = protoval.Substring(0, protoval.Length - 1); }
                if (nullValues.Contains(protoval))
                {
                    toReturn.Add(null);
                }
                else
                {
                    toReturn.Add(protoval.Trim().UnescapeString(escapechar));
                }
                yield return toReturn.ToArray();
            }
            yield break;
        }

        /// <summary>
        /// Lazily reads a CSV file line by line, using a designated a separator, escape character, and list of values to consider null, into an IEnumerable of DataStores.
        /// <para>If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you'll need to supply headers.</para>
        /// <para>The defaults are appropriate for most csv files, but if you have CSVs that use alternative separators, escape characters, or null values, you'll need to specify them.</para>
        /// </summary>
        /// <param name="CSVString">CSV in string form to use as input.</param>
        /// <param name="headers">If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you'll need to supply headers.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="separator">Character to separate on. Usually a comma, sometimes a tab.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `</param>
        /// <returns>An IEnumerable of immutable DataStore objects representing the parsed data.</returns>
        public static IEnumerable<DataStore<string>> ReadCSVFromString(string CSVString, string[]? headers = null, string[]? nullValues = null, char separator = ',', char escapechar = '"')
        {
            using (TextReader reader = new StringReader(CSVString))
            {
                foreach (DataStore<string> row in reader.ReadCSV(headers, nullValues, separator, escapechar))
                {
                    yield return row;
                }
            }
            yield break;
        }
        /// <summary>
        /// Lazily reads a CSV file line by line, using a designated a separator, escape character, and list of values to consider null, into an IEnumerable of DataStores.
        /// <para>If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you'll need to supply headers.</para>
        /// <para>The defaults are appropriate for most csv files, but if you have CSVs that use alternative separators, escape characters, or null values, you'll need to specify them.</para>
        /// </summary>
        /// <param name="CSVFile">File path to CSV file to use as input.</param>
        /// <param name="headers">If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you'll need to supply headers.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="separator">Character to separate on. Usually a comma, sometimes a tab.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `</param>
        /// <returns>An IEnumerable of immutable DataStore objects representing the parsed data.</returns>
        public static IEnumerable<DataStore<string>> ReadCSVFromFile(string CSVFile, string[]? headers = null, string[]? nullValues = null, char separator = ',', char escapechar = '"')
        {
            using (TextReader reader = new StreamReader(CSVFile))
            {
                foreach (DataStore<string> row in reader.ReadCSV(headers, nullValues, separator, escapechar))
                {
                    yield return row;
                }
            }
            yield break;
        }
        /// <summary>
        /// Lazily reads a CSV file line by line, using a designated a separator, escape character, and list of values to consider null, into an IEnumerable of DataStores.
        /// <para>If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you'll need to supply headers.</para>
        /// <para>The defaults are appropriate for most csv files, but if you have CSVs that use alternative separators, escape characters, or null values, you'll need to specify them.</para>
        /// </summary>
        /// <param name="input">TextReader to use as input.</param>
        /// <param name="headers">If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you'll need to supply headers.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="separator">Character to separate on. Usually a comma, sometimes a tab.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `</param>
        /// <returns>An IEnumerable of immutable DataStore objects representing the parsed data.</returns>
        public static IEnumerable<DataStore<string>> ReadCSV(this TextReader input, string[]? headers = null, string[]? nullValues = null, char separator = ',',  char escapechar = '"')
        {
            if (nullValues == null) { nullValues = new string[] { "" }; }//We let users specify, because some programs (Looking at you R with your NA values) have custom null values.
            IEnumerable<string?[]> CSV = ReadCSVRaw(input, nullValues,separator, escapechar);
            if(headers == null)
            {
                headers = CSV.FirstOrDefault()?.Select(x => x ?? "NULL").ToArray();
            }
            if (headers == null)
            {
                yield break;
            }
            for (int i = 0; i < headers.Length - 1; i++)
            {
                int count = 0;
                for (int j = i + 1; j < headers.Length; j++)
                {
                    if (headers[i] == headers[j])
                    {
                        headers[j] = headers[j] + $"({++count})";
                    }
                }
            }

            foreach(string?[] CSVRow in CSV)//Skips the header because the IEnumerable remembers it's already read the header.
            {
                string?[] values = CSVRow;
                if(values.Length != headers.Length)
                {
                    string?[] newValues = new string?[headers.Length];
                    Array.Copy(values, newValues, Math.Min(values.Length,headers.Length));//any unfilled in slots will default to null, and any extra values will be ignored.
                    values = newValues;
                }
                yield return new DataStore<string>(headers, values, true);
            }
            yield break;
        }

        /// <summary>
        /// Replace double escaped escape characters with just one copy.
        /// </summary>
        /// <param name="input">String to unescape.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `.</param>
        /// <returns></returns>
        private static string? UnescapeString(this string? input, char escapechar)
        {
            string e1 = new string(escapechar, 1);
            string e2 = new string(escapechar, 2);
            if(input != null && input.StartsWith(e1) && input.EndsWith(e1) && (input.Where(x => x == escapechar).Count() % 2 == 0))
            {
                return input.Substring(1, input.Length - 2).Replace(e2, e1);
            }
            return input;
        }
        /// <summary>
        /// Escapes the String prepatory to writing to a CSV File
        /// </summary>
        /// <param name="input">String to escape.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `.</param>
        /// <returns></returns>
        private static string? EscapeString(this string? input, char separator, char escapechar)
        {
            string e1 = new string(escapechar, 1);
            string e2 = new string(escapechar, 2);
            if (input == null) { return null; }
            if (input == "") { return e2; }
            if(input.Where(x => x == separator || x == escapechar || x == '\n').Any() || input.Trim() != input)
            {
                return e1 + input.Replace(e1, e2) + e1;
            }
            return input;
        }
        /// <summary>
        /// Writes a CSV out line by line allowing the caller to end a line with whichever NewLine method they want.
        /// Each string is a new line, and can be directly written as such.
        /// </summary>
        /// <param name="DataStores">The datastores to pull data from.</param>
        /// <param name="headers">The headers to write on the first line, and to use to lookup data in the datastores.</param>
        /// <param name="nullValue">The string to write when a null value is encountered. This defaults to the empty string. (i.e. no space between separators)</param>
        /// <param name="separator">The character to use to separate values on a line. Usually a comma ,</param>
        /// <param name="escapechar">The character to use to escape. Usually a double quote "</param>
        /// <returns></returns>
        public static IEnumerable<string> WriteCSV(this IEnumerable<DataStore<string>> DataStores, string[] headers, string nullValue = "", char separator = ',', char escapechar = '"')
        {
            yield return string.Join(new string(separator, 1), headers.Select(x => EscapeString(x, separator, escapechar) ?? nullValue));
            foreach(DataStore<string> DS in DataStores)
            {
                yield return string.Join(new string(separator, 1), headers.Select(x => EscapeString(DS[x], separator, escapechar) ?? nullValue));
            }
            yield break;
        }
    }
}
