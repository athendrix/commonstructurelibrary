using Bridge.Html5;
using CSL.Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Data
{
    internal class SQLiteMultiMessageHandler
    {
        WebWorker SQLWorker;
        WorkerLock SQLWorkerLock;
        bool finished = false;

        internal static async Task<SQLiteMultiMessageHandler> RunQuery(WebWorker SQLWorker, string SQLQuery)
        {
            WorkerLock SQLWorkerLock = await SQLWorker.LockWorker();
            return new SQLiteMultiMessageHandler(SQLWorkerLock, SQLWorker, SQLQuery);
        }
        private SQLiteMultiMessageHandler(WorkerLock SQLWorkerLock, WebWorker SQLWorker, string SQLQuery)
        {
            this.SQLWorker = SQLWorker;
            this.SQLWorkerLock = SQLWorkerLock;
            SQLWorker.OnMessage += OnMessage;
            SQLWorker.PostMessage(new SQLite.PostMessage() { action = "each", sql = SQLQuery });
        }
        private void OnMessage(object sender, dynamic message)
        {
            Window.Alert(JSON.Stringify(message));
            if(message.finished == true)
            {
                //Window.Alert(message);
                Finish();
            }
            if(message.finished == false)
            {
                //Window.Alert(message);
            }
        }
        private void Finish()
        {
            if (!finished)
            {
                finished = true;
                SQLWorkerLock.Dispose();
                //SQLWorker

            }
        }
    }
}
