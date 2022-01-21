if (args != null && args.Length > 0 && Directory.Exists(args[0]))
{
    CSL.SQL.ClassCreator.FileConverter.Convert(new DirectoryInfo(args[0]));
}
else if (args != null && args.Length > 0 && File.Exists(args[0]))
{
    CSL.SQL.ClassCreator.FileConverter.Convert(new FileInfo(args[0]));
}