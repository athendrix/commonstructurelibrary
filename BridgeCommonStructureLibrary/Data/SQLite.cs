using System;
//using System.Data;
//using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Bridge;
using Bridge.Html5;
using Console = Bridge.Html5.Console;
using CSL.Helpers;
using CSL.Bridge;

namespace CSL.Data
{
    public class SQLite : IDisposable//, ISQLInterface
    {
        //public DbConnection InternalConnection { get; protected set; }
        //private dynamic worker;
        private readonly WebWorker SQLWorker;
        //private Queue<SQLQueueObject> SQLQueue;
        //private bool SQLWorkerRunning;
        //private bool Transaction;
        //private event EventHandler QueueFinished;

        public SQLite()
        {
            //worker = Script.Call<dynamic>("new Worker", "worker.sql-wasm.js");
            SQLWorker = new WebWorker("worker.sql-wasm.js");
            //Transaction = false;
            //SQLQueue = new Queue<SQLQueueObject>();
            //SQLWorkerRunning = false;
        }
        #region Worker Call Wrappers
        public async Task OpenFile()
        {
            ArrayBuffer buffer;
            try
            {
                buffer = await BridgeAssist.OpenFile();
                await Open(new Uint8Array(buffer, 0, (uint)buffer.ByteLength));
            }
            catch { }
        }
        public async Task Open(Uint8Array buffer = null)
        {
            using (await SQLWorker.LockWorker())
            {
                _ = await SQLWorker.PostAndAwaitMessage<PostMessage, object>(buffer == null ? new PostMessage() { action = "open" } : new PostMessage() { action = "open", buffer = buffer });
                //Window.Alert(test.ToString());
            }

            //Task toReturn = BridgeAssist.MakePromise((Resolve, Reject) =>
            //{
            //SQLQueue.Enqueue(new SQLQueueObject()
            //{
            //    onmessage = (me) =>
            //    {
            //        Resolve();
            //        //OnLoad?.Invoke(this, new EventArgs());
            //    },
            //    onerror = (me) =>
            //    {
            //        Reject(null);
            //    },

            //    postMessage = buffer == null ? new PostMessage() { action = "open" } : new PostMessage() { action = "open", buffer = buffer }
            //});
            //RunQueue();
            //});
            //return toReturn;

        }
        public async Task Export(string filename)
        {
            byte[] buffer;
            using (await SQLWorker.LockWorker())
            {
                BufferResult br = await SQLWorker.PostAndAwaitMessage<PostMessage, BufferResult>(new PostMessage() { action = "export" });
                buffer = br.buffer;
            }
            HTMLAnchorElement a = new HTMLAnchorElement();
            a.Style.Display = "none";
            Document.Body.AppendChild(a);
            a.Href = BridgeAssist.CreateObjectURL(new Blob(new BlobDataObject[] { buffer.As<BlobDataObject>() }, new BlobPropertyBag() { Type = "application/x-sqlite3" }));
            if (filename == null)
            {
                filename = "Data";
            }
            a.Download = filename + ".sqlite";
            a.Click();

            BridgeAssist.RevokeObjectURL(a.Href);
            Document.Body.RemoveChild(a);

            //return BridgeAssist.MakePromise((Resolve, Reject) =>
            //{
            //    SQLQueue.Enqueue(new SQLQueueObject()
            //    {
            //        postMessage = new PostMessage() { action = "export" },
            //        onmessage = (me) =>
            //        {
            //            try
            //            {
            //                HTMLAnchorElement a = new HTMLAnchorElement();
            //                a.Style.Display = "none";
            //                Document.Body.AppendChild(a);
            //                //it should have a buffer object on it...
            //                byte[] buffer = new BufferResult(me).buffer;
            //                a.Href = BridgeAssist.CreateObjectURL(new Blob(new BlobDataObject[]{ buffer.As<BlobDataObject>() }, new BlobPropertyBag() { Type = "application/x-sqlite3" }));
            //                if (filename == null)
            //                {
            //                    filename = "Data";
            //                }
            //                a.Download = filename + ".sqlite";
            //                a.Click();

            //                BridgeAssist.RevokeObjectURL(a.Href);
            //                Document.Body.RemoveChild(a);
            //                Resolve();
            //            }
            //            catch
            //            {
            //                Reject(null);
            //            }
            //        },
            //        onerror = (me) =>
            //        {
            //            Reject(null);
            //        }
            //    });
            //    RunQueue();
            //});
        }
        public async Task<Query[]> Exec(string sql)
        {
            using (await SQLWorker.LockWorker())
            {
                QueryResult qr = await SQLWorker.PostAndAwaitMessage<PostMessage, QueryResult>(new PostMessage() { action = "exec", sql = sql });
                return qr.results;
            }
            //return  BridgeAssist.MakePromise<QueryResult>((Resolve, Reject) =>
            //{
            //    SQLQueue.Enqueue(new SQLQueueObject()
            //    {
            //        onmessage = (me) =>
            //        {
            //            QueryResult toResolve;
            //            try
            //            {
            //                toResolve = new QueryResult(me);
            //            }
            //            catch
            //            {
            //                Reject(null);
            //                return;
            //            }
            //            Resolve(toResolve);
            //        },
            //        onerror = (me) => Reject(null),
            //        postMessage = new PostMessage() { action = "exec", sql = sql }
            //    });
            //    RunQueue();
            //});
        }
        internal Task<SQLiteMultiMessageHandler> Each(string sql) => SQLiteMultiMessageHandler.RunQuery(SQLWorker, sql);
        //    return BridgeAssist.MakePromise<QueryResult>((Resolve, Reject) =>
        //    {
        //        SQLQueue.Enqueue(new SQLQueueObject()
        //        {
        //            onmessage = (me) =>
        //            {
        //                QueryResult toResolve;
        //                try
        //                {
        //                    toResolve = new QueryResult(me);
        //                }
        //                catch
        //                {
        //                    Reject(null);
        //                    return;
        //                }
        //                Resolve(toResolve);
        //            },
        //            onerror = (me) => Reject(null),
        //            postMessage = new PostMessage() { action = "exec", sql = sql }
        //        });
        //        RunQueue();
        //    });
        #endregion
        /*
        #region Server Calls
        public Task<IEnumerable<ISQLRow>> ExecuteReader(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null)
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
            return new AutoClosingDataReader(cmd.ExecuteReader(), cmd);
        }
        public Task<int> ExecuteNonQuery(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
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
                return cmd.ExecuteNonQuery();
            }
        }
        public Task<T> ExecuteScalar<T>(string commandText, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            Debug.Assert(default(T) == null, "Type must be Nullable. Try adding a ? to the end of the type to make it Nullable. (e.g. 'int?')");
            using (DbCommand cmd = InternalConnection.CreateCommand())
            {
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
                object toReturn = cmd.ExecuteScalar();

                if (DBNull.Value.Equals(toReturn))
                {
                    return default;
                }
                return (T)toReturn;
            }
        }
        #endregion
        #region Transaction Management
        public async Task BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            if (Transaction) { throw new NotSupportedException("Starting a transaction while in a transaction is not supported by this class."); }
            if (isolationLevel != IsolationLevel.Serializable) { throw new NotSupportedException("Transactions in SQLite are by nature serializable unless using special modes that don't apply to web worker based SQLite"); }
            await ExecuteNonQuery("BEGIN TRANSACTION;");
            Transaction = true;
        }
        public async Task CommitTransaction()
        {
            if (!Transaction) { throw new NotSupportedException("Not currently in a transaction."); }
            await ExecuteNonQuery("COMMIT TRANSACTION;");
            Transaction = false;
        }
        public async Task RollbackTransaction()
        {
            if (!Transaction) { throw new NotSupportedException("Not currently in a transaction."); }
            await ExecuteNonQuery("ROLLBACK TRANSACTION;");
            Transaction = false;
        }
        #endregion
        */
        public void Dispose()
        {
            //try { if (Transaction) { RollbackTransaction().Wait(); } } catch (Exception) { }
            try { if (SQLWorker != null) { SQLWorker.Dispose(); } } catch (Exception) { }
        }
        #region Queue Management
        //private void RunQueue()
        //{
        //    if (SQLQueue.Count > 0)
        //    {
        //        if (!SQLWorkerRunning)
        //        {
        //            SQLWorkerRunning = true;
        //            SQLQueueObject SQLQueryToRun = SQLQueue.Dequeue();
        //            worker.onmessage = (Action<MessageEvent>)((MessageEvent e) =>
        //            {
        //                SQLWorkerRunning = false;
        //                SQLQueryToRun.onmessage?.Invoke(e);
        //            });
        //            worker.onerror = (Action<Error>)((Error e) =>
        //            {
        //                SQLWorkerRunning = false;
        //                Console.Error(e.Message);
        //            });
        //            worker.onmessageerror = worker.onerror;
        //            worker.postMessage(SQLQueryToRun.postMessage);
        //        }
        //        Window.SetTimeout(RunQueue, 100);
        //    }
        //    else
        //    {
        //        if (!SQLWorkerRunning)
        //        {
        //            QueueFinished?.Invoke(this, new EventArgs());
        //        }
        //        else
        //        {
        //            Window.SetTimeout(RunQueue, 100);
        //        }
        //    }
        //}
        [ObjectLiteral]
        public class Query
        {
            public string[] columns = null;
            public object[][] values = null;
        }
        [ObjectLiteral]
        public class QueryResult
        {
            public Query[] results;
            //public QueryResult(MessageEvent me)
            //{
            //    try
            //    {
            //        dynamic Data = me.Data;
            //        columns = Data.results[0].columns;
            //        values = Data.results[0].values;
            //    }
            //    catch
            //    {
            //    }
            //}
        }
        [ObjectLiteral]
        private class BufferResult
        {
            public byte[] buffer = null;
        }
        //private class SQLQueueObject
        //{
        //    public Action<MessageEvent> onmessage = null;
        //    public Action<MessageEvent> onerror = null;
        //    public PostMessage postMessage = null;
        //}
        [ObjectLiteral]
        internal class PostMessage
        {
            public string action;
            public string sql;
            public Uint8Array buffer;
        }
        #endregion
    }

}

