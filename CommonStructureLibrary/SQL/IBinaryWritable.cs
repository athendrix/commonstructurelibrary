using System;
using System.Collections.Generic;
using System.Text;

namespace CSL.SQL
{
    public interface IBinaryWritable
    {
        byte[] ToByteArray();
    }
}
