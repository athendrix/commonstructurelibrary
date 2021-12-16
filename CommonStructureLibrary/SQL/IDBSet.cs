using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public interface IDBSet
    {
        Task<int> Insert(SQLDB sql);
        Task<int> Update(SQLDB sql);
        Task<int> Upsert(SQLDB sql);
        object?[] ToArray();
    }
}