using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleNamespace.SomeSubNamespace
{

    //Example Enum
    [Flags]
    //Specifying ulong allows data to be auto converted for your convenience into the database.
    public enum Data29 : ulong
    {
        NoFlags = 0,
        Flag1 = 1UL << 0,
        Flag2 = 1UL << 1,
        Flag3 = 1UL << 2,
        Flag4 = 1UL << 3,
        Flag5 = 1UL << 4,
        Flag6 = 1UL << 5,
        Flag7 = 1UL << 6,
        Flag8 = 1UL << 7,
        Flag9 = 1UL << 8,
        Flag10 = 1UL << 9,
        Flag11 = 1UL << 10,
        Flag12 = 1UL << 11,
        Flag13 = 1UL << 12,
        Flag14 = 1UL << 13,
        Flag15 = 1UL << 14,
        Flag16 = 1UL << 15,
    }

    //Example Enum
    [Flags]
    //Specifying ulong allows data to be auto converted for your convenience into the database.
    public enum Data30 : ulong
    {
        NoFlags = 0,
        Flag1 = 1UL << 0,
        Flag2 = 1UL << 1,
        Flag3 = 1UL << 2,
        Flag4 = 1UL << 3,
        Flag5 = 1UL << 4,
        Flag6 = 1UL << 5,
        Flag7 = 1UL << 6,
        Flag8 = 1UL << 7,
        Flag9 = 1UL << 8,
        Flag10 = 1UL << 9,
        Flag11 = 1UL << 10,
        Flag12 = 1UL << 11,
        Flag13 = 1UL << 12,
        Flag14 = 1UL << 13,
        Flag15 = 1UL << 14,
        Flag16 = 1UL << 15,
    }
}
