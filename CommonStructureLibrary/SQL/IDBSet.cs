using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public interface IDBSet<T>
    {
        T PK { get; }
        Task<int> Insert(SQLDB sql);
        Task<int> Update(SQLDB sql);
        Task<int> Upsert(SQLDB sql);
    }
    public interface IDBSet<T, U>
    {
        T PK1 { get; }
        U PK2 { get; }
        Task<int> Insert(SQLDB sql);
        Task<int> Update(SQLDB sql);
        Task<int> Upsert(SQLDB sql);
    }
    public interface IDBSet<T, U, V>
    {
        T PK1 { get; }
        U PK2 { get; }
        V PK3 { get; }
        Task<int> Insert(SQLDB sql);
        Task<int> Update(SQLDB sql);
        Task<int> Upsert(SQLDB sql);
    }
    public interface IDBSet<T, U, V, W>
    {
        T PK1 { get; }
        U PK2 { get; }
        V PK3 { get; }
        W PK4 { get; }
        Task<int> Insert(SQLDB sql);
        Task<int> Update(SQLDB sql);
        Task<int> Upsert(SQLDB sql);
    }
    public interface IDBSetFactory<T>
    {
        Task<int> CreateDB(SQLDB sql);
        IAsyncEnumerable<IDBSet<T>> Select(SQLDB sql);
        IAsyncEnumerable<IDBSet<T>> Select(SQLDB sql, string query, params object[] parameters);
        Task<IDBSet<T>> SelectByPK(SQLDB sql, T PK);
        IEnumerable<IDBSet<T>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK(SQLDB sql, T PK);
    }
    public interface IDBSetFactory<T, U>
    {
        Task<int> CreateDB(SQLDB sql);
        IAsyncEnumerable<IDBSet<T, U>> Select(SQLDB sql);
        IAsyncEnumerable<IDBSet<T, U>> Select(SQLDB sql, string query, params object[] parameters);
        IAsyncEnumerable<IDBSet<T, U>> SelectByPK1(SQLDB sql, T PK1);
        IAsyncEnumerable<IDBSet<T, U>> SelectByPK2(SQLDB sql, U PK2);
        Task<IDBSet<T, U>> SelectByPK(SQLDB sql, T PK1, U PK2);
        IEnumerable<IDBSet<T, U>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK1(SQLDB sql, T PK1);
        Task<int> DeleteByPK2(SQLDB sql, U PK2);
        Task<int> DeleteByPK(SQLDB sql, T PK1, U PK2);
    }
    public interface IDBSetFactory<T, U, V>
    {
        Task<int> CreateDB(SQLDB sql);
        IAsyncEnumerable<IDBSet<T, U, V>> Select(SQLDB sql);
        IAsyncEnumerable<IDBSet<T, U, V>> Select(SQLDB sql, string query, params object[] parameters);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK1(SQLDB sql, T PK1);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK2(SQLDB sql, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK3(SQLDB sql, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK12(SQLDB sql, T PK1, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK13(SQLDB sql, T PK1, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK23(SQLDB sql, U PK2, V PK3);
        Task<IDBSet<T, U, V>> SelectByPK(SQLDB sql, T PK1, U PK2, V PK3);
        IEnumerable<IDBSet<T, U, V>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK1(SQLDB sql, T PK1);
        Task<int> DeleteByPK2(SQLDB sql, U PK2);
        Task<int> DeleteByPK3(SQLDB sql, V PK3);
        Task<int> DeleteByPK12(SQLDB sql, T PK1, U PK2);
        Task<int> DeleteByPK13(SQLDB sql, T PK1, V PK3);
        Task<int> DeleteByPK23(SQLDB sql, U PK2, V PK3);
        Task<int> DeleteByPK(SQLDB sql, T PK1, U PK2, V PK3);
    }
    public interface IDBSetFactory<T, U, V, W>
    {
        Task<int> CreateDB(SQLDB sql);
        IAsyncEnumerable<IDBSet<T, U, V, W>> Select(SQLDB sql);
        IAsyncEnumerable<IDBSet<T, U, V, W>> Select(SQLDB sql, string query, params object[] parameters);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK1(SQLDB sql, T PK1);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK2(SQLDB sql, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK3(SQLDB sql, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK4(SQLDB sql, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK12(SQLDB sql, T PK1, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK13(SQLDB sql, T PK1, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK14(SQLDB sql, T PK1, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK23(SQLDB sql, U PK2, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK24(SQLDB sql, U PK2, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK34(SQLDB sql, V PK3, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK123(SQLDB sql, T PK1, U PK2, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK124(SQLDB sql, T PK1, U PK2, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK134(SQLDB sql, T PK1, V PK3, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK234(SQLDB sql, U PK2, V PK3, W PK4);
        Task<IDBSet<T, U, V, W>> SelectByPK(SQLDB sql, T PK1, U PK2, V PK3, W PK4);
        IEnumerable<IDBSet<T, U, V, W>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK1(SQLDB sql, T PK1);
        Task<int> DeleteByPK2(SQLDB sql, U PK2);
        Task<int> DeleteByPK3(SQLDB sql, V PK3);
        Task<int> DeleteByPK4(SQLDB sql, W PK4);
        Task<int> DeleteByPK12(SQLDB sql, T PK1, U PK2);
        Task<int> DeleteByPK13(SQLDB sql, T PK1, V PK3);
        Task<int> DeleteByPK14(SQLDB sql, T PK1, W PK4);
        Task<int> DeleteByPK23(SQLDB sql, U PK2, V PK3);
        Task<int> DeleteByPK24(SQLDB sql, U PK2, W PK4);
        Task<int> DeleteByPK34(SQLDB sql, V PK3, W PK4);
        Task<int> DeleteByPK123(SQLDB sql, T PK1, U PK2, V PK3);
        Task<int> DeleteByPK124(SQLDB sql, T PK1, U PK2, W PK4);
        Task<int> DeleteByPK134(SQLDB sql, T PK1, V PK3, W PK4);
        Task<int> DeleteByPK234(SQLDB sql, U PK2, V PK3, W PK4);
        Task<int> DeleteByPK(SQLDB sql, T PK1, U PK2, V PK3, W PK4);
    }
}