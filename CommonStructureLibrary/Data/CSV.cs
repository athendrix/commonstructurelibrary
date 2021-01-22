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
        /// Reads a CSV file, separated by a separator, escaped by an escape character into a DataTable, and attempts to guess the Type of each Column based on the typeDictionary which is editable.
        /// <para>The defaults are appropriate for most csv files, but if you have custom data types you want to read, you'll have to make a typeDictionary. Use the default for guidance.</para>
        /// <para>Does not name the table, but will not change the name if a name exists.</para>
        /// <para>Can also append data to an existing table if the columns and datatypes match.</para>
        /// </summary>
        /// <param name="input">TextReader to use as input.</param>
        /// <param name="separator">Character to separate on. Usually a comma ,sometimes a tab.</param>
        /// <param name="headers">If headers are not supplied, we'll grab them from the first row. If the first row contains data, then you need to supply headers.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="typeDictionary">An ordered dictionary of types with methods for processing them from the underlying data. See the DefaultTypeDictionary for more info.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `, or even nothing at all.</param>
        /// <param name="startLine">How many lines to skip? This will skip after reading the header(if there is one), but before reading the data.</param>
        /// <param name="lineLimit">Maximum data lines to read. (Not counting the headerline, nor newlines within string values.)</param>
        /// <returns>An array of immutable DataStore objects representing the data.</returns>
        public static DataStore[] ReadCSV(TextReader input, char separator = ',', string[] headers = null, string[] nullValues = null, char? escapechar = '"', int startLine = 0, int? lineLimit = null)
        {
            if (nullValues == null) { nullValues = new string[] { "" }; }//Mostly to deal with R's NA values
            //List<object[]> readData = new List<object[]>();
            List<DataStore> readData = new List<DataStore>();
            string firstLine = input.AdvancedReadLine(escapechar);
            if (firstLine == null)
            {
                return null;
            }
            string[] firstLineArray = firstLine.AdvancedSplit(separator, nullValues, escapechar);
            int linesRead = 0;
            if(headers == null)
            {
                headers = firstLineArray;
            }
            else if (startLine <= 0 && lineLimit != 0)
            {
                readData.Add(new DataStore(headers,firstLineArray,true));
                linesRead = 1;
            }
            
            for (int i = 0; i < startLine; i++)
            {
                input.AdvancedReadLine(escapechar);//skip the lines while paying attention to the escape characters.
            }
            string currLine;
            while ((lineLimit == null || linesRead++ < lineLimit) && (currLine = input.AdvancedReadLine(escapechar)) != null)
            {
                string[] currLineArray = currLine.AdvancedSplit(separator, nullValues, escapechar);
                readData.Add(new DataStore(headers,currLineArray,true));
            }
            return readData.ToArray();
        }

        /// <summary>
        /// Suggested to call from certain custom TypeConverters to remove external escape characters and replace double escaped escape characters with just one copy.
        /// </summary>
        /// <param name="input">String to prune escape characters from.</param>
        /// <param name="escapechar"></param>
        /// <returns></returns>
        //public static string PruneEscapeCharacters(this string input, char escapechar)
        //{
        //    int firstpos;
        //    if ((firstpos = input.IndexOf(escapechar)) > -1)
        //    {
        //        input = input.Remove(firstpos, 1);
        //        input = input.Remove(input.LastIndexOf(escapechar), 1);
        //    }
        //    input = input.Replace(new string(escapechar, 2), new string(escapechar, 1));
        //    return input;
        //}
        /// <summary>
        /// Escapes the String prepatory to writing to a CSV File
        /// </summary>
        /// <param name="input">String to escape.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `.</param>
        /// <returns></returns>
        //public static string EscapeString(this string input, char escapechar)
        //{
        //    return escapechar + input.Replace(new string(escapechar, 1), new string(escapechar, 2)) + escapechar;
        //}
        /// <summary>
        /// Reads a line from the TextReader, but ignores newlines that have been escaped.
        /// </summary>
        /// <param name="input">TextReader to use as input.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `.</param>
        /// <returns>Line from TextReader</returns>
        private static string AdvancedReadLine(this TextReader input, char? escapechar)
        {
            string toReturn = input.ReadLine();
            if (toReturn == null)
            {
                return null;
            }
            if (escapechar != null)
            {
                int currCount = toReturn.Count(c => c == escapechar);
                while (currCount % 2 != 0)
                {
                    string nextLine = input.ReadLine();
                    if (nextLine == null)
                    {
                        return toReturn;
                    }
                    toReturn += "\r\n" + nextLine;
                    currCount += nextLine.Count(c => c == escapechar);
                }
            }
            return toReturn;
        }
        /// <summary>
        /// Like String.Split, but ignores separators that have been escaped, and returns certain values as null.
        /// </summary>
        /// <param name="input">String to split</param>
        /// <param name="separator">Character to separate on. Usually a comma.</param>
        /// <param name="nullValues">Values to treat as null. This defaults to only situations where there are no characters between two separators.</param>
        /// <param name="escapechar">Character to escape on. Usually a Double Quote " but may be a single ' or back quote `.</param>
        /// <returns>Array of strings that have been singled out like String.Split would.</returns>
        private static string[] AdvancedSplit(this string input, char separator, string[] nullValues, char? escapechar)
        {
            if (nullValues == null) { nullValues = nullValues = new string[] { "" }; }
            string[] naiveArray = input.Split(new char[] { separator }, StringSplitOptions.None);
            List<string> parsedLine = new List<string>();
            for (int i = 0; i < naiveArray.Length; i++)
            {
                string toAdd = naiveArray[i];
                if (escapechar != null)
                {
                    int currCount = toAdd.Count(c => c == escapechar);
                    while (currCount % 2 != 0 && i < naiveArray.Length - 1)
                    {
                        toAdd += "," + naiveArray[++i];
                        currCount += naiveArray[i].Count(c => c == escapechar);
                    }
                }
                if (nullValues.Contains(toAdd))
                {
                    parsedLine.Add(null);
                    continue;
                }
                parsedLine.Add(toAdd);
            }
            return parsedLine.ToArray();
        }
    }
}
