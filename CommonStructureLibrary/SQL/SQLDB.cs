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
    public abstract class SQLDB : IDisposable
    {
        [Obsolete("With the obsoletion of other functions in this class, this function is no longer necessary.")]
        public static string QueryIn<T>(IEnumerable<T> inputList, out IEnumerable<KeyValuePair<string, object?>> parameters)
        {
            T[] enumerableArray = inputList as T[] ?? inputList.ToArray();
            parameters = enumerableArray.Select((x, y) => new KeyValuePair<string, object?>("@param" + y, x));
            string joinedString = string.Join(", @param", Enumerable.Range(0, enumerableArray.Length));
            return string.IsNullOrEmpty(joinedString) ? "(NULL)" : $"(@param{joinedString})";
        }

        public DbConnection InternalConnection { get; protected set; }
        protected DbTransaction? currentTransaction = null;
        protected SQLDB(DbConnection internalConnection) => InternalConnection = internalConnection;

        #region Server Calls
        #region Reader
        [Obsolete("This version of ExecuteReader is deprecated. Please use the version with incrementing values.")]
        public Task<AutoClosingDataReader> ExecuteReader(string commandText, IEnumerable<KeyValuePair<string, object>> parameters, CommandType commandType = CommandType.Text) => InnerExecuteReader(commandText, parameters, commandType);
        private async Task<AutoClosingDataReader> InnerExecuteReader(string commandText, IEnumerable<KeyValuePair<string, object>> parameters, CommandType commandType)
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
        /// <summary>
        /// Use incrementing values for each parameter, prefixed with an @ symbol.
        /// for example, "SELECT * FROM users WHERE username = @0 AND site = @1;"
        /// </summary>
        /// <param name="commandText">The SQL Text to execute.</param>
        /// <param name="parameters">The Parameters passed in.</param>
        /// <returns>An AutoClosingDataReader object that should be wrapped in a using block.</returns>
        public async Task<AutoClosingDataReader> ExecuteReader(string commandText, params object[] parameters)
        {
            DbCommand cmd = InternalConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = currentTransaction;
            for (int i = 0; i < parameters.Length; i++)
            {
                DbParameter toAdd = cmd.CreateParameter();
                toAdd.ParameterName = "@" + i.ToString();
                toAdd.Value = parameters[i] ?? DBNull.Value;
                cmd.Parameters.Add(toAdd);
            }
            return new AutoClosingDataReader(await cmd.ExecuteReaderAsync(), cmd);
        }
        /// <summary>
        /// Use incrementing values for each parameter, prefixed with an @ symbol.
        /// for example, "SELECT * FROM users WHERE username = @0 AND site = @1;"
        /// </summary>
        /// <param name="commandText">The SQL Text to execute.</param>
        /// <param name="parameters">The Parameters passed in.</param>
        /// <returns>An AutoClosingDataReader object that should be wrapped in a using block.</returns>
        public AutoClosingDataReader ExecuteReaderSync(string commandText, params object[] parameters)
        {
            DbCommand cmd = InternalConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = currentTransaction;
            for (int i = 0; i < parameters.Length; i++)
            {
                DbParameter toAdd = cmd.CreateParameter();
                toAdd.ParameterName = "@" + i.ToString();
                toAdd.Value = parameters[i] ?? DBNull.Value;
                cmd.Parameters.Add(toAdd);
            }
            return new AutoClosingDataReader(cmd.ExecuteReader(), cmd);
        }
        #endregion
        #region NonQuery
        [Obsolete("This version of ExecuteNonQuery is deprecated. Please use the version with incrementing values.")]
        public Task<int> ExecuteNonQuery(string commandText, IEnumerable<KeyValuePair<string, object?>> parameters, CommandType commandType = CommandType.Text) => InnerExecuteNonQuery(commandText, parameters, commandType);
        private Task<int> InnerExecuteNonQuery(string commandText, IEnumerable<KeyValuePair<string, object?>> parameters, CommandType commandType = CommandType.Text)
        {
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                cmd.Transaction = currentTransaction;
                foreach (KeyValuePair<string, object?> parameter in parameters)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = parameter.Key;
                    toAdd.Value = parameter.Value ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
                return cmd.ExecuteNonQueryAsync();
            }
        }
        /// <summary>
        /// Use incrementing values for each parameter, prefixed with an @ symbol.
        /// for example, "UPDATE users SET site = @1 WHERE username = @0;"
        /// </summary>
        /// <param name="commandText">The SQL Text to execute.</param>
        /// <param name="parameters">The Parameters passed in.</param>
        /// <returns>An int representing how many rows were affected.</returns>
        public Task<int> ExecuteNonQuery(string commandText, params object?[] parameters)
        {
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = currentTransaction;
                for (int i = 0; i < parameters.Length; i++)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = "@" + i.ToString();
                    toAdd.Value = parameters[i] ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
                return cmd.ExecuteNonQueryAsync();
            }
        }
        /// <summary>
        /// Use incrementing values for each parameter, prefixed with an @ symbol.
        /// for example, "UPDATE users SET site = @1 WHERE username = @0;"
        /// </summary>
        /// <param name="commandText">The SQL Text to execute.</param>
        /// <param name="parameters">The Parameters passed in.</param>
        /// <returns>An int representing how many rows were affected.</returns>
        public int ExecuteNonQuerySync(string commandText, params object?[] parameters)
        {
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = currentTransaction;
                for (int i = 0; i < parameters.Length; i++)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = "@" + i.ToString();
                    toAdd.Value = parameters[i] ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
                return cmd.ExecuteNonQuery();
            }
        }
        #endregion
        #region Scalar
        [Obsolete("This version of ExecuteScalar is deprecated. Please use the version with incrementing values.")]
        public Task<T?> ExecuteScalar<T>(string commandText, IEnumerable<KeyValuePair<string, object?>> parameters, CommandType commandType = CommandType.Text) => InnerExecuteScalar<T>(commandText, parameters, commandType);
        private async Task<T?> InnerExecuteScalar<T>(string commandText, IEnumerable<KeyValuePair<string, object?>> parameters, CommandType commandType = CommandType.Text)
        {
            Debug.Assert(default(T?) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                cmd.Transaction = currentTransaction;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object?> parameter in parameters)
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
                    return default(T?);
                }
                return (T)toReturn;
            }
        }
        /// <summary>
        /// Use incrementing values for each parameter, prefixed with an @ symbol.
        /// for example, "SELECT userid FROM users WHERE username = @0 AND site = @1;"
        /// </summary>
        /// <param name="commandText">The SQL Text to execute.</param>
        /// <param name="parameters">The Parameters passed in.</param>
        /// <returns>The value in the first column and first row of the result.</returns>
        public async Task<T?> ExecuteScalar<T>(string commandText, params object[] parameters)
        {
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = currentTransaction;
                for (int i = 0; i < parameters.Length; i++)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = "@" + i.ToString();
                    toAdd.Value = parameters[i] ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
                object toReturn = await cmd.ExecuteScalarAsync();
                if (DBNull.Value.Equals(toReturn))
                {
                    return default;
                }
                return (T)toReturn;
            }
        }
        /// <summary>
        /// Use incrementing values for each parameter, prefixed with an @ symbol.
        /// for example, "SELECT userid FROM users WHERE username = @0 AND site = @1;"
        /// </summary>
        /// <param name="commandText">The SQL Text to execute.</param>
        /// <param name="parameters">The Parameters passed in.</param>
        /// <returns>The value in the first column and first row of the result.</returns>
        public T? ExecuteScalarSync<T>(string commandText, params object[] parameters)
        {
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = currentTransaction;
                for (int i = 0; i < parameters.Length; i++)
                {
                    DbParameter toAdd = cmd.CreateParameter();
                    toAdd.ParameterName = "@" + i.ToString();
                    toAdd.Value = parameters[i] ?? DBNull.Value;
                    cmd.Parameters.Add(toAdd);
                }
                object toReturn = cmd.ExecuteScalar();
                if (DBNull.Value.Equals(toReturn))
                {
                    return default;
                }
                return (T)toReturn;
            }
        }
        #endregion
        #endregion
        #region Transaction Management
        public virtual void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            if (currentTransaction != null) { throw new NotSupportedException("Starting a transaction while in a transaction is not supported by this class."); }
            currentTransaction = InternalConnection.BeginTransaction(isolationLevel);
        }
        public virtual void CommitTransaction()
        {
            if (currentTransaction == null) { throw new NotSupportedException("Not currently in a transaction."); }
            currentTransaction.Commit();
            try { currentTransaction.Dispose(); } catch (Exception) { }
            currentTransaction = null;
        }
        public virtual void RollbackTransaction()
        {
            if (currentTransaction == null) { throw new NotSupportedException("Not currently in a transaction."); }
            currentTransaction.Rollback();
            try { currentTransaction.Dispose(); } catch (Exception) { }
            currentTransaction = null;
        }
        #endregion
        public void Dispose()
        {
            try { if (currentTransaction != null) { currentTransaction.Rollback(); } } catch (Exception) { }
            try { if (currentTransaction != null) { currentTransaction.Dispose(); } } catch (Exception) { }
            try { if (InternalConnection != null) { InternalConnection.Close(); } } catch (Exception) { }
            try { if (InternalConnection != null) { InternalConnection.Dispose(); } } catch (Exception) { }
        }
    }

}
