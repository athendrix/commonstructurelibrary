using CSL.API;
using Newtonsoft.Json.Linq;
using Namespace.Data;
using SQLClassCreator;

string JSON = "{'glossary': { 'title': 'example glossary','GlossDiv': { 'title': 'S','GlossList': { 'GlossEntry': { 'ID': 'SGML','SortAs': 'SGML','GlossTerm': 'Standard Generalized Markup Language','Acronym': 'SGML','Abbrev': 'ISO 8879:1986','GlossDef': { 'para': 'A meta-markup language, used to create markup languages such as DocBook.','GlossSeeAlso': ['GML', 'XML'] },'GlossSee': 'markup' } } } }}";

globject? test = APIRecord.FromJSONObject<globject>(JObject.Parse(JSON));
string newJSON = test?.ToJSON() ?? "";

ComplexRecord cr = new ComplexRecord(
      new ComplexRecordLayer1("somevalval",
        new ComplexRecordLayer2(69, 420, 5,
          new ComplexRecordLayer3(Guid.NewGuid().ToByteArray(), 99, -3, ulong.MaxValue, DateTime.Now),
          new ComplexRecordLayer3(Guid.NewGuid().ToByteArray(), 95, -69, 900001, DateTime.UtcNow),
          new ComplexRecordLayer3_2("ahendrix", decimal.MaxValue, new ComplexRecordArray[] { new ComplexRecordArray("Pokemon are fun!", 69), new ComplexRecordArray("I'm winning!", 22) }
          )
        )
      )
    );
string newerJSON = cr.ToJSON();
ComplexRecord? cr2 = APIRecord.FromJSON<ComplexRecord>(newerJSON);
if(cr.ToJSON() == cr2?.ToJSON() )
{
    Console.WriteLine("I guess it's working!");
}

if (args != null && args.Length > 0 && Directory.Exists(args[0]))
{
    CSL.ClassCreation.FileConverter.Convert(new DirectoryInfo(args[0]));
}
else if (args != null && args.Length > 0 && File.Exists(args[0]))
{
    CSL.ClassCreation.FileConverter.Convert(new FileInfo(args[0]));
}