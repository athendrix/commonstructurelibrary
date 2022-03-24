using CSL.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLClassCreator
{
    public record ComplexRecord(ComplexRecordLayer1 item) : APIRecord;
    public record ComplexRecordLayer1(string someval, ComplexRecordLayer2 item) : APIRecord;
    public record ComplexRecordLayer2(int val1, long val2, byte val3, [Attribute] ComplexRecordLayer3 layer1, ComplexRecordLayer3 layer2, ComplexRecordLayer3_2 layer3) : APIRecord;
    public record ComplexRecordLayer3(byte[] data, uint val1, sbyte val0, ulong bigint, DateTime dt) : APIRecord;
    public record ComplexRecordLayer3_2(string id, decimal value, ComplexRecordArray[] lowerArray) : APIRecord;
    public record ComplexRecordArray(string words, int score) : APIRecord;
}
