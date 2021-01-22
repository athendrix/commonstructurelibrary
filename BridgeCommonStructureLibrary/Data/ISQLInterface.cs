using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Data
{
    public interface ISQLInterface
    {
        Task<IEnumerable<ISQLRow>> ExecuteReader(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null);
        Task<int> ExecuteNonQuery(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null);
        Task<T> ExecuteScalar<T>(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task BeginTransaction(IsolationLevel isolationLevel);
        Task CommitTransaction();
        Task RollbackTransaction();
    }
    public enum IsolationLevel
    {
        Serializable = 0,
        RepeatableRead = 1,
        ReadCommitted = 2,
        ReadUncommitted = 3,
        Snapshot = 4,
        Chaos = 5
    }
}
