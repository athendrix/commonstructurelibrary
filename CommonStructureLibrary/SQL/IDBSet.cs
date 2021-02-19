using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public interface IDBSet<T>
    {
        T PK { get; }
        Task<int> Insert(SQL sql);
        Task<int> Update(SQL sql);
        Task<int> Upsert(SQL sql);
    }
    public interface IDBSet<T, U>
    {
        T PK1 { get; }
        U PK2 { get; }
        Task<int> Insert(SQL sql);
        Task<int> Update(SQL sql);
        Task<int> Upsert(SQL sql);
    }
    public interface IDBSet<T, U, V>
    {
        T PK1 { get; }
        U PK2 { get; }
        V PK3 { get; }
        Task<int> Insert(SQL sql);
        Task<int> Update(SQL sql);
        Task<int> Upsert(SQL sql);
    }
    public interface IDBSet<T, U, V, W>
    {
        T PK1 { get; }
        U PK2 { get; }
        V PK3 { get; }
        W PK4 { get; }
        Task<int> Insert(SQL sql);
        Task<int> Update(SQL sql);
        Task<int> Upsert(SQL sql);
    }
    public interface IDBSetFactory<T>
    {
        Task<int> CreateDB(SQL sql);
        IAsyncEnumerable<IDBSet<T>> Select(SQL sql);
        Task<IDBSet<T>> SelectByPK(SQL sql, T PK);
        IEnumerable<IDBSet<T>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK(SQL sql, T PK);
    }
    public interface IDBSetFactory<T, U>
    {
        Task<int> CreateDB(SQL sql);
        IAsyncEnumerable<IDBSet<T, U>> Select(SQL sql);
        IAsyncEnumerable<IDBSet<T, U>> SelectByPK1(SQL sql, T PK1);
        IAsyncEnumerable<IDBSet<T, U>> SelectByPK2(SQL sql, U PK2);
        Task<IDBSet<T, U>> SelectByPK(SQL sql, T PK1, U PK2);
        IEnumerable<IDBSet<T, U>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK1(SQL sql, T PK1);
        Task<int> DeleteByPK2(SQL sql, U PK2);
        Task<int> DeleteByPK(SQL sql, T PK1, U PK2);
    }
    public interface IDBSetFactory<T, U, V>
    {
        Task<int> CreateDB(SQL sql);
        IAsyncEnumerable<IDBSet<T, U, V>> Select(SQL sql);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK1(SQL sql, T PK1);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK2(SQL sql, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK3(SQL sql, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK12(SQL sql, T PK1, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK13(SQL sql, T PK1, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V>> SelectByPK23(SQL sql, U PK2, V PK3);
        Task<IDBSet<T, U, V>> SelectByPK(SQL sql, T PK1, U PK2, V PK3);
        IEnumerable<IDBSet<T, U, V>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK1(SQL sql, T PK1);
        Task<int> DeleteByPK2(SQL sql, U PK2);
        Task<int> DeleteByPK3(SQL sql, V PK3);
        Task<int> DeleteByPK12(SQL sql, T PK1, U PK2);
        Task<int> DeleteByPK13(SQL sql, T PK1, V PK3);
        Task<int> DeleteByPK23(SQL sql, U PK2, V PK3);
        Task<int> DeleteByPK(SQL sql, T PK1, U PK2, V PK3);
    }
    public interface IDBSetFactory<T, U, V, W>
    {
        Task<int> CreateDB(SQL sql);
        IAsyncEnumerable<IDBSet<T, U, V, W>> Select(SQL sql);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK1(SQL sql, T PK1);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK2(SQL sql, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK3(SQL sql, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK4(SQL sql, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK12(SQL sql, T PK1, U PK2);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK13(SQL sql, T PK1, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK14(SQL sql, T PK1, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK23(SQL sql, U PK2, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK24(SQL sql, U PK2, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK34(SQL sql, V PK3, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK123(SQL sql, T PK1, U PK2, V PK3);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK124(SQL sql, T PK1, U PK2, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK134(SQL sql, T PK1, V PK3, W PK4);
        IAsyncEnumerable<IDBSet<T, U, V, W>> SelectByPK234(SQL sql, U PK2, V PK3, W PK4);
        Task<IDBSet<T, U, V, W>> SelectByPK(SQL sql, T PK1, U PK2, V PK3, W PK4);
        IEnumerable<IDBSet<T, U, V, W>> GetEnumerator(IDataReader dr);
        Task<int> DeleteByPK1(SQL sql, T PK1);
        Task<int> DeleteByPK2(SQL sql, U PK2);
        Task<int> DeleteByPK3(SQL sql, V PK3);
        Task<int> DeleteByPK4(SQL sql, W PK4);
        Task<int> DeleteByPK12(SQL sql, T PK1, U PK2);
        Task<int> DeleteByPK13(SQL sql, T PK1, V PK3);
        Task<int> DeleteByPK14(SQL sql, T PK1, W PK4);
        Task<int> DeleteByPK23(SQL sql, U PK2, V PK3);
        Task<int> DeleteByPK24(SQL sql, U PK2, W PK4);
        Task<int> DeleteByPK34(SQL sql, V PK3, W PK4);
        Task<int> DeleteByPK123(SQL sql, T PK1, U PK2, V PK3);
        Task<int> DeleteByPK124(SQL sql, T PK1, U PK2, W PK4);
        Task<int> DeleteByPK134(SQL sql, T PK1, V PK3, W PK4);
        Task<int> DeleteByPK234(SQL sql, U PK2, V PK3, W PK4);
        Task<int> DeleteByPK(SQL sql, T PK1, U PK2, V PK3, W PK4);
    }
}