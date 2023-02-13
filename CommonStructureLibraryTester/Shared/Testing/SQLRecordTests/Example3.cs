using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public record Example3(Guid ID, string Data1, string? Data2, int Data25) : SQLRecord<Example3>;
}