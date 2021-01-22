using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Data
{
    public interface ISQLRow
    {
        int Count { get; }
        T Get<T>(int i);
        T Get<T>(string name);
        int GetOrdinal(string name);
        string GetName(int i);
    }
}
