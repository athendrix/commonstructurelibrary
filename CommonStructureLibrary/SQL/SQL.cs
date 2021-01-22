using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public abstract class SQL : IDisposable
    {

        public static string QueryIn<T>(IEnumerable<T> inputList, out IEnumerable<KeyValuePair<string, object>> parameters)
        {
            T[] enumerableArray = inputList as T[] ?? inputList.ToArray();
            parameters = enumerableArray.Select((x, y) => new KeyValuePair<string, object>("@param" + y, x));
            string joinedString = string.Join(", @param", Enumerable.Range(0, enumerableArray.Length));
            return String.IsNullOrEmpty(joinedString) ? "(NULL)" : $"(@param{joinedString})";
        }

        public DbConnection InternalConnection { get; protected set; }
        protected DbTransaction currentTransaction = null;
        #region Server Calls
        public async Task<AutoClosingDataReader> ExecuteReader(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null, CommandType commandType = CommandType.Text)
        {
            DbCommand cmd = InternalConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            cmd.Transaction = currentTransaction;
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = parameter.Key;
                    toAdd.Value = parameter.Value ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
            }
            return new AutoClosingDataReader(await cmd.ExecuteReaderAsync(), cmd);
        }
        public Task<int> ExecuteNonQuery(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null, CommandType commandType = CommandType.Text)
        {
            using DbCommand cmd = InternalConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            cmd.Transaction = currentTransaction;
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = parameter.Key;
                    toAdd.Value = parameter.Value ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
            }
            return cmd.ExecuteNonQueryAsync();
        }
        public async Task<T> ExecuteScalar<T>(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null, CommandType commandType = CommandType.Text)
        {
            Debug.Assert(default(T) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
            using DbCommand cmd = InternalConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            cmd.Transaction = currentTransaction;
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = parameter.Key;
                    toAdd.Value = parameter.Value ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
            }
            object toReturn = await cmd.ExecuteScalarAsync();
            if (DBNull.Value.Equals(toReturn))
            {
                return default;
            }
            return (T)toReturn;
        }
        #endregion
        #region Transaction Management
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            if (currentTransaction != null) { throw new NotSupportedException("Starting a transaction while in a transaction is not supported by this class."); }
            currentTransaction = InternalConnection.BeginTransaction(isolationLevel);
        }
        public void CommitTransaction()
        {
            if (currentTransaction == null) { throw new NotSupportedException("Not currently in a transaction."); }
            currentTransaction.Commit();
            try { currentTransaction.Dispose(); } catch (Exception) { }
            currentTransaction = null;
        }
        public void RollbackTransaction()
        {
            if (currentTransaction == null) { throw new NotSupportedException("Not currently in a transaction."); }
            currentTransaction.Rollback();
            try { currentTransaction.Dispose(); } catch (Exception) { }
            currentTransaction = null;
        }
        #endregion
        public void Dispose()
        {
            try { if (currentTransaction != null) { currentTransaction.Dispose(); } } catch (Exception) { }
            try { if (InternalConnection != null) { InternalConnection.Dispose(); } } catch (Exception) { }
        }
    }

}
