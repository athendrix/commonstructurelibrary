using CSL.API;
using Newtonsoft.Json.Linq;
using Namespace.Data;
using SQLClassCreator;
using SomeNamespace.Somesubspace;

string JSON = "{'glossary': { 'title': 'example glossary','GlossDiv': { 'title': 'S','GlossList': { 'GlossEntry': { 'ID': 'SGML','SortAs': 'SGML','GlossTerm': 'Standard Generalized Markup Language','Acronym': 'SGML','Abbrev': 'ISO 8879:1986','GlossDef': { 'para': 'A meta-markup language, used to create markup languages such as DocBook.','GlossSeeAlso': ['GML', 'XML'] },'GlossSee': 'markup' } } } }}";

string JSON2 = "{\"name\" : \"Natalie West\",\"street\" : \"208 Alfonzo Springs\",\"city\" : \"Murazikhaven\",\"zip\" : 45060,\"user_name\" : \"Elfrieda_Koelpin88\",\"catch_phrase\" : \"You can't calculate the protocol without programming the virtual HDD panel!\",\"user_id\" : 9248,\"user_id\" : \"Desmond83@hotmail.com\",\"favorite_colors\" : [\"#0b2832\",\"#1f6427\"],\"reg_date\" : \"Mon, 05 Oct 2015 11:24:34 GMT\",\"sign_ups\" : {\"newsletter\" : false},\"long_int\" : 8827444846,\"neg_long_int\" : -11216392804,\"guid\" : \"8b8c0e38-c026-4f90-a8a9-bbe35b3435cc\",\"float\" : 3.14159265358979323846264338}";
Console.WriteLine(APIRecord.CreateRecordFromJSONTemplate(JSON2, "TestExample", "SomeNamespace.Somesubspace"));
globject? test = APIRecord.FromJSONObject<globject>(JObject.Parse(JSON));
TestExample? exam = APIRecord.FromJSON<TestExample>(JSON2);
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