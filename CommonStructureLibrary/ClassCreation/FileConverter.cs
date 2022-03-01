using CSL.SQL.ClassCreator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSL.ClassCreation
{
    public static class FileConverter
    {
        public static void Convert(FileInfo fi)
        {
            switch (fi.Extension.ToLower())
            {
                case ".tabledef":
                    TableDefinition tabledef = TableDefinition.ParseTabledef(File.ReadAllText(fi.FullName));
                    File.WriteAllText(fi.FullName + ".cs", tabledef.GenerateCode(true));
                    break;
                case ".funcdef":
                    FunctionDefinition funcdef = FunctionDefinition.ParseFunctiondef(File.ReadAllText(fi.FullName));
                    File.WriteAllText(fi.FullName + ".cs", funcdef.GenerateCode());
                    break;
                case ".cs":
                    return;
            }
        }
        public static void Convert(DirectoryInfo di, bool subfolders = true)
        {
            foreach (FileInfo fi in di.GetFiles("*", subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                Convert(fi);
            }
        }
    }
}
