using CSL.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL
{
    public static class SettingsParser
    {
        public static DataStore<string> ReadSettings(string path, bool caseInsensitiveLookup = true, bool immutable = false) =>
            Parse(File.ReadLines(path), caseInsensitiveLookup, immutable);
        public static DataStore<string> Parse(IEnumerable<string> lines, bool caseInsensitiveLookup = true, bool immutable = false)
        {
            DataStore<string> toReturn = new DataStore<string>(caseInsensitiveLookup);
            Stack<string> CurrentSection = new Stack<string>();
            foreach (string line in lines)
            {
                if(string.IsNullOrWhiteSpace(line)) { continue; }
                string linecomp = line.Trim();
                if (linecomp == "" || linecomp.StartsWith("#") || linecomp.StartsWith("//") || linecomp.StartsWith(";")) { continue; }
                if(linecomp.StartsWith("[") && linecomp.EndsWith("]"))
                {
                    
                    int depth = 0;
                    for(depth = 0; linecomp.StartsWith("[") && linecomp.EndsWith("]"); depth++)
                    {
                        linecomp = linecomp.Substring(1, linecomp.Length - 2).Trim();
                    }

                    while (depth > CurrentSection.Count + 1) { CurrentSection.Push("."); }
                    while (depth < CurrentSection.Count + 1){ CurrentSection.Pop();}
                    CurrentSection.Push(linecomp + ".");

                    continue;
                }
                if (linecomp.Contains('='))
                {
                    int EqualsPos = linecomp.IndexOf('=');
                    string key = linecomp.Substring(0, EqualsPos).Trim();
                    string value = ParseEscapeCharacters(linecomp.Substring(EqualsPos + 1).Trim());
                    toReturn[string.Join("",CurrentSection) + key] = value;
                }
                else
                {
                    toReturn[string.Join("",CurrentSection) + linecomp] = "";
                }
            }
            if(immutable)
            {
                toReturn.SetImmutable();
            }
            return toReturn;
        }
        private static IEnumerable<string> AllButLast(string[] lines) => lines.Take(lines.Length - 1);
        public static IEnumerable<string> ToLines(this DataStore<string> input)
        {
            int CurrentDepth = 0;
            foreach (IGrouping<string, KeyValuePair<string, string?>> group in input.GroupBy(x => string.Join(".", AllButLast(x.Key.Split('.')))).OrderBy(x => x.Key))
            {
                if (group.Key is "")
                {
                    if (CurrentDepth != 0) { throw new Exception("This shouldn't happen. If \"\" is in the group, it should be first, and depth should be 0"); }

                }
                else
                {
                    string[] domains = group.Key.Split('.');
                    if (domains.Length <= CurrentDepth) { CurrentDepth = domains.Length - 1; }
                    while (domains.Length > CurrentDepth)
                    {
                        CurrentDepth++;
                        yield return new string('[', CurrentDepth) + domains[CurrentDepth - 1] + new string(']', CurrentDepth);
                    }
                }
                foreach(KeyValuePair<string, string?> setting in group)
                {
                    yield return $"{setting.Key.Substring(setting.Key.LastIndexOf('.') + 1)}={EscapeCharacters(setting.Value ?? "")}";
                }

            }
        }
        public static void WriteSettings(this DataStore<string> input, string path) => File.WriteAllLines(path, input.ToLines());
        private static string ParseEscapeCharacters(string input)
        {
            input = input.Trim();
            if (input.Length < 2 || !input.StartsWith("\"") || !input.EndsWith("\""))
            {
                return input;
            }
            input = input.Substring(1, input.Length - 2);
            string[] splits = input.Split(new string[] { "\\\\" }, StringSplitOptions.None);
            for (int i = 0; i < splits.Length; i++)
            {
                splits[i] = splits[i]
                    .Replace("\\v", "\v")
                    .Replace("\\b", "\b")
                    .Replace("\\f", "\f")
                    .Replace("\\0", "\0")
                    .Replace("\\t", "\t")
                    .Replace("\\r", "\r")
                    .Replace("\\n", "\n")
                    .Replace("\\\"", "\"")
                    .Replace("\\","");//Clear out unsupported escapes
#warning TODO: Replace unicode escape sequences?
            }
            return string.Join("\\", splits);
        }
        private static string EscapeCharacters(string input)
        {
            if(input.IndexOfAny("\"\n\r\t\0\f\b\v".ToCharArray()) is -1 && input == input.Trim())
            {
                return input;
            }
            string toReturn = input
                                .Replace("\\", "\\\\")
                                .Replace("\"", "\\\"")
                                .Replace("\n", "\\n")
                                .Replace("\r", "\\r")
                                .Replace("\t", "\\t")
                                .Replace("\0", "\\0")
                                .Replace("\f", "\\f")
                                .Replace("\b", "\\b")
                                .Replace("\v", "\\v");
            return $"\"{toReturn}\"";
        }

#warning TODO: Replace non-ascii characters with Unicode escape sequences?
    }
}
