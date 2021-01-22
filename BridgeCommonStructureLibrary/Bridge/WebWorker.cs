using Bridge;
using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Bridge
{
    public class WebWorker :IDisposable
    {
        private dynamic innerWorker;
        public WebWorker(string url)
        {
            innerWorker = Script.Call<dynamic>("new Worker", url);
            innerWorker.onmessage = (Action<MessageEvent>)OnMessageCallback;
        }
        #region OnMessage Event
        List<EventHandler<dynamic>> delegates = new List<EventHandler<dynamic>>();
        private event EventHandler<dynamic> innerOnMessage;
        public event EventHandler<dynamic> OnMessage
        {
            add
            {
                innerOnMessage += value;
                delegates.Add(value);
            }
        
            remove
            {
                innerOnMessage -= value;
                delegates.Remove(value);
            }
        }
        private void ClearEventListeners()
        {
            foreach (EventHandler<dynamic> eh in delegates)
            {
                innerOnMessage -= eh;
            }
            delegates.Clear();
        }
        #endregion

        #region WorkerLock

        private Queue<Action> waiters = new Queue<Action>();
        private bool locked = false;
        public Task<WorkerLock> LockWorker()
        {
            if (locked)
            {
                return BridgeAssist.MakePromise<WorkerLock>((Resolve, Reject) =>
                {
                    waiters.Enqueue(() =>
                    {
                        locked = true;
                        Resolve(new WorkerLock(ReleaseLock));
                    });
                });
            }
            else
            {
                locked = true;
                return Task.FromResult(new WorkerLock(ReleaseLock));
            }
        }
        private void ReleaseLock()
        {
            locked = false;
            ClearEventListeners();
            if (waiters.Count > 0)
            {
                waiters.Dequeue()();
            }
        }
        #endregion
        public void PostMessage<TRequest>(TRequest message)
        {
            innerWorker.postMessage(message);
        }
        public Task<TResponse> PostAndAwaitMessage<TRequest,TResponse>(TRequest message)
        {
            Task<TResponse> toReturn = BridgeAssist.MakePromise<TResponse>((Resolve, Reject) =>
            {
                EventHandler<dynamic> toAdd = null;
                toAdd = (object sender, dynamic e) =>
                {
                    OnMessage -= toAdd;
                    Resolve(e);
                };
                OnMessage += toAdd;
            });
            PostMessage(message);
            return toReturn;
        }
        private void OnMessageCallback(MessageEvent e)
        {
            if(e == null)
            {
                innerOnMessage?.Invoke(this, null);
                return;
            }
            //if(typeof(TResponse) == typeof(MessageEvent))
            //{
            //    innerOnMessage?.Invoke(this, e.As<TResponse>());
            //    return;
            //}
            if (e.Data == null)
            {
                innerOnMessage?.Invoke(this, null);
                return;
            }
            innerOnMessage?.Invoke(this, e.Data.As<dynamic>());
            //if (e.Data.GetType() == typeof(string))
            //{
            //    if (typeof(TResponse) == typeof(string))
            //    {
            //        innerOnMessage?.Invoke(this, e.Data.As<TResponse>());
            //    }
            //    else
            //    {
            //        object response = JSON.Parse((string)e.Data);
            //        innerOnMessage?.Invoke(this, response.As<TResponse>());
            //    }
            //    return;
            //}

            //TODO: Cast other types appropriately?
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        innerWorker.terminate();
                    }
                    catch { }
                }
                disposedValue = true;
            }
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
    public class WorkerLock : IDisposable
    {
        Action OnDispose;
        public WorkerLock(Action OnDispose)
        {
            this.OnDispose = OnDispose;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        OnDispose();
                    }
                    catch { }
                }
                OnDispose = null;
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
