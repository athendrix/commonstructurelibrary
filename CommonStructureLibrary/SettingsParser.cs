using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL
{
    public record Setting(string key, string value);
    public record SettingSection(string name, Setting[] settings);
    public static class SettingsParser
    {
        public static SettingSection[] Parse(IEnumerable<string> lines)
        {
            string CurrentSection = "";
            List<SettingSection> toReturn = new List<SettingSection>();
            List<Setting> toAdd = new List<Setting>();
            foreach(string line in lines)
            {
                if(string.IsNullOrWhiteSpace(line)) { continue; }
                string linecomp = line.Trim();
                if (linecomp.StartsWith("#") || linecomp.StartsWith("//") || linecomp.StartsWith(";")) { continue; }
                if(linecomp.StartsWith("[") && linecomp.EndsWith("]"))
                {
                    string NewSection = linecomp.Substring(1, linecomp.Length - 2).Trim();
                    if(NewSection == CurrentSection) { continue; }
                    toReturn.Add(new SettingSection(CurrentSection, toAdd.ToArray()));
                    SettingSection? DuplicateSection = toReturn.Where((x) => x.name == NewSection).FirstOrDefault();
                    if (DuplicateSection != null)
                    {
                        toReturn.Remove(DuplicateSection);
                        toAdd = new List<Setting>(DuplicateSection.settings);
                    }
                    else
                    {
                        toAdd = new List<Setting>();
                    }
                    CurrentSection = NewSection;
                    continue;
                }
                if (linecomp.Contains('='))
                {
                    int EqualsPos = linecomp.IndexOf('=');
                    string key = linecomp.Substring(0, EqualsPos).Trim();
                    string value = ParseEscapeCharacters(linecomp.Substring(EqualsPos + 1).Trim());
                    toAdd.Add(new Setting(key, value));
                }
                else
                {
                    toAdd.Add(new Setting(linecomp.Trim(), ""));
                }
            }
            toReturn.Add(new SettingSection(CurrentSection, toAdd.ToArray()));
            return toReturn.ToArray();
        }
        public static IEnumerable<string> ToSettings(SettingSection[] input)
        {
            foreach(SettingSection section in input)
            {
                yield return $"[{section.name}]";
                foreach (Setting setting in section.settings)
                {
                    yield return $"{setting.key}={EscapeCharacters(setting.value)}";
                }
            }
        }
        private static string ParseEscapeCharacters(string input)
        {
            string[] splits = input.Split(new string[] { "\\\\" }, StringSplitOptions.None);
            for (int i = 0; i < splits.Length; i++)
            {
                splits[i] = splits[i]
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\0", "\0")
                    .Replace("\\f", "\f")
                    .Replace("\\b", "\b")
                    .Replace("\\v", "\v")
                    //.Replace("\\'", "\'")
                    //.Replace("\\\"", "\"")
                    .Replace("\\","");//Clear out extras
#warning TODO: Replace unicode escape sequences
            }
            return string.Join("\\", splits);
        }
        private static string EscapeCharacters(string input) => input
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\0", "\\0")
                .Replace("\f", "\\f")
                .Replace("\b", "\\b")
                .Replace("\v", "\\v");
#warning TODO: Replace non-ascii characters with Unicode escape sequences
    }
}
