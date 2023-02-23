using System;
using CSL.SQL;
namespace ExampleNamespace.SomeSubNamespace;
public record Example4(Guid ID, byte ByteTest, ulong UnsignedLongTest) : SQLRecord<Example4>;