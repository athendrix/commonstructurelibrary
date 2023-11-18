using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;
using static CSL.SQL.Conditional;

namespace ExampleNamespace.SomeSubNamespace;

[SQLRecord(4)]
public record Example2(
    Guid ID,
    Guid ID2,
    Guid ID3,
    Guid ID4,
    [FK("OtherTable","CompanyID",0)] string Company,
    [FK("WrongTable", "ClientID", 0)] string WrongClient,
    [FK("OtherTable", "ClientID", 0)] string Client,
    [FK("YetAnotherTable", "SomeID")] string NormalFK) : SQLRecord<Example2>;