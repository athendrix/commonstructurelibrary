using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public record Example2(Guid ID, Guid ID2, Guid ID3, Guid ID4, bool Data1, bool? Data2, byte Data3, byte? Data4,
        char Data5, char? Data6, short Data7, short? Data8, int Data9, int? Data10, long Data11, long? Data12,
        ulong Data13, ulong? Data14, float Data15, float? Data16, double Data17, double? Data18, decimal Data19,
        decimal? Data20, string Data21, string? Data22, byte[] Data23, byte[]? Data24, Guid Data25, Guid? Data26,
        DateTime Data27, DateTime? Data28, Data29 Data29, Data30? Data30) : SQLRecord<Example2>;
    
}