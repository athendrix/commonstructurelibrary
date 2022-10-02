using CSL.API;
using Newtonsoft.Json.Linq;
using StandardRecordTest;

//string XML = File.ReadAllText("C:\\Users\\Public\\smalltest.xml");
//Console.WriteLine(APIRecord.CreateRecordFromXMLTemplate(XML, "Standard", "StandardRecordTest"));
//StandardRecordTest.Standard.FromXML(XML);

if (args != null && args.Length > 0 && Directory.Exists(args[0]))
{
    CSL.ClassCreation.FileConverter.Convert(new DirectoryInfo(args[0]));
}
else if (args != null && args.Length > 0 && File.Exists(args[0]))
{
    CSL.ClassCreation.FileConverter.Convert(new FileInfo(args[0]));
}