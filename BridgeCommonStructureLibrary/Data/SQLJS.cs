using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Html5;
using Console = Bridge.Html5.Console;
using CSL.Helpers;


//TODO Add timeout with failure for all promises
//Add Worker test so that we can check if the worker is working (use something like the scalar with select 1 and a timeout)
//On a failure, we should try and find out why...
namespace CSL.Old
{
    /// <summary>
    /// Note that there are a lot of unsafe casts and dynamic data
    /// This is due to the fact that Bridge doesn't have a "Worker" object,
    /// So we have to use the Retyped Worker.
    /// Also, some of the data we receive from the Worker needs to be coerced into a proper format.
    /// So while there ARE unsafe casts and dynamic data, you shouldn't need to worry about them outside this class.
    /// </summary>

    /*
    public class SQLjs : IDisposable
    {
        dynamic worker = Script.Get<dynamic>("new Worker","worker.sql.js");
        bool SQLWorkerRunning = false;
        Queue<SQLQueueObject> SQLQueue = new Queue<SQLQueueObject>();
        public event EventHandler QueueFinished;
        public event EventHandler OnLoad;

        HTMLInputElement FileInput = new HTMLInputElement();
        string DataName = null;

        public SQL(HTMLStructure structure)
        {
            FileInput.Type = InputType.File;
            FileInput.Accept = ".Metrika";
            FileInput.Hidden = true;
            FileInput.OnChange = (Event<HTMLInputElement> e) =>
            {
                FileReader fr = new FileReader();
                fr.OnLoad = (Event ev) =>
                {
                    ArrayBuffer buffer = (ArrayBuffer)fr.Result;
                    string name = FileInput.Files[0].Name;
                    if (name.IndexOf('.') >= 0)
                    {
                        name = name.Substring(0, name.LastIndexOf('.'));
                    }
                    LoadData(new Uint8Array(buffer, 0, (uint)buffer.ByteLength), name);
                };
                fr.ReadAsArrayBuffer(FileInput.Files[0]);
            };
            Document.Body.AppendChild(FileInput);
            structure.CreateButton("Open Metrika File", FileInput.Click);
            structure.CreateButton("Save Metrika File", () => { SaveData(); });
        }

        private void Run()
        {
            if (SQLQueue.Count > 0)
            {
                if (!SQLWorkerRunning)
                {
                    SQLWorkerRunning = true;
                    SQLQueueObject SQLQueryToRun = SQLQueue.Dequeue();
                    worker.onmessage = (Action<MessageEvent>)((MessageEvent e) =>
                    {
                        SQLWorkerRunning = false;
                        SQLQueryToRun.onmessage?.Invoke(e);
                    });
                    worker.onerror = (Action<Error>)((Error e) =>
                    {
                        SQLWorkerRunning = false;
                        Console.Error(e.Message);
                    });
                    //@this.worker.onmessageerror = this.worker.onerror;
                    worker.postMessage(SQLQueryToRun.postMessage);
                }
                Window.SetTimeout(Run, 100);
            }
            else
            {
                if (!SQLWorkerRunning)
                {
                    QueueFinished?.Invoke(this, new EventArgs());
                }
                else
                {
                    Window.SetTimeout(Run, 100);
                }
            }
        }
        public Task LoadData(Uint8Array buffer, string Name)
        {
            Console.Info("Loading SQL Database...");
            Task toReturn = new Promise((Resolve, Reject) =>
            {
                SQLQueue.Enqueue(new SQLQueueObject()
                {
                    onmessage = (me) =>
                    {
                        DataName = Name;
                        Resolve();
                        OnLoad?.Invoke(this, new EventArgs());
                    },
                    onerror = (me) =>
                    {
                        DataName = null;
                        Reject(null);
                    },
                    postMessage = new PostMessage() { action = "open", buffer = buffer }
                });
                Run();
            }).ToTask();
            return toReturn;
        }
        public Task SaveData()
        {
            if (DataName == null)
            {
                return null;
            }
            return new Promise((Resolve, Reject) =>
            {
                SQLQueue.Enqueue(new SQLQueueObject()
                {
                    postMessage = new PostMessage() { action = "export" },
                    onmessage = (me) =>
                    {
                        try
                        {
                            HTMLAnchorElement a = new HTMLAnchorElement();
                            a.Style.Display = "none";
                            Document.Body.AppendChild(a);
                            //it should have a buffer object on it...
                            byte[] buffer = new BufferResult(me).buffer;
                            a.Href = Helpers.CreateObjectURL(buffer, "application/x-sqlite3");
                            string filename = Window.Prompt("Enter file name:", DataName);
                            if (filename != null)
                            {
                                a.Download = filename + ".Metrika";
                                a.Click();
                            }
                            Helpers.RevokeObjectURL(a.Href);
                            Document.Body.RemoveChild(a);
                            Resolve();
                        }
                        catch
                        {
                            Reject(null);
                        }
                    },
                    onerror = (me) =>
                    {
                        Reject(null);
                    }
                });
                Run();
            }).ToTask();
        }

        public Task<QueryResult> Query(string sql)
        {
            Task<QueryResult> toReturn = new Promise<QueryResult>((Resolve, Reject) =>
            {
                SQLQueue.Enqueue(new SQLQueueObject() { onmessage = (me) => Resolve(new QueryResult(me)), onerror = (me) => Reject(null), postMessage = new PostMessage() { action = "exec", sql = sql } });
                Run();
            }).ToTask();
            return toReturn;
        }
        public Task<T> Scalar<T>(string sql)
        {
            Task<T> toReturn = new Promise<T>((Resolve, Reject) =>
            {
                SQLQueue.Enqueue(new SQLQueueObject()
                {
                    onmessage = (me) =>
                    {
                        T toResolve;
                        try
                        {
                            toResolve = (T)new QueryResult(me).values[0][0];
                        }
                        catch
                        {
                            Reject(null);
                            return;
                        }
                        Resolve(toResolve);
                    },
                    onerror = (me) => Reject(null),
                    postMessage = new PostMessage() { action = "exec", sql = sql }
                });
                Run();
            }).ToTask();
            return toReturn;
        }
        public Task<T> NonQuery<T>(string sql)
        {
            Task<T> toReturn = new Promise<T>((Resolve, Reject) =>
            {
                SQLQueue.Enqueue(new SQLQueueObject()
                {
                    onmessage = (me) =>
                    {
                        T toResolve;
                        try
                        {
                            toResolve = me.Data.As<T>();
                        }
                        catch
                        {
                            Reject(null);
                            return;
                        }
                        Resolve(toResolve);
                    },
                    onerror = (me) => Reject(null),
                    postMessage = new PostMessage() { action = "each", sql = sql }
                });
                Run();
            }).ToTask();
            return toReturn;
        }


        public void Dispose()
        {
            worker.terminate();
        }
    }
    */
    public class SQLQueueObject
    {
        public Action<MessageEvent> onmessage = null;
        public Action<MessageEvent> onerror = null;
        public PostMessage postMessage = null;
    }
    public class PostMessage
    {
        public string action;
        public string sql;
        public Uint8Array buffer;
    }
    public class QueryResult
    {
        public string[] columns = null;
        public object[][] values = null;
        public QueryResult(MessageEvent me)
        {
            try
            {
                dynamic Data = me.Data;
                columns = Data.results[0].columns;
                values = Data.results[0].values;
            }
            catch
            {
            }
        }
    }
    public class BufferResult
    {
        public byte[] buffer = null;
        public BufferResult(MessageEvent me)
        {
            try
            {
                dynamic Data = me.Data;
                buffer = Data.buffer;
            }
            catch
            {
            }
        }
    }
}
