using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Blazor
{
    public delegate void WorkerMessage(object? data);
    public class WebWorker : IDisposable
    {
        private readonly IJSRuntime js;
        private readonly DotNetObjectReference<WebWorker> objref;
        private readonly string id;
        private readonly string WorkerPath;
        public event WorkerMessage? OnMessage;
        public event WorkerMessage? OnError;
        public event WorkerMessage? OnMessageError;
        private bool initialized = false;

        public WebWorker(IJSRuntime js, string WorkerPath)
        {
            this.js = js;
            this.WorkerPath = WorkerPath;
            id = "ID_" + Guid.NewGuid().ToString("N");
            objref = DotNetObjectReference.Create(this);

        }
        public async ValueTask Init()
        {
            if(disposed) { throw new ObjectDisposedException("WebWorker"); }
            await js.InvokeVoidAsync("CSL.CreateWorker", new object[] { objref, id, WorkerPath });
            initialized = true;
        }

        [JSInvokable]
        public void OnMessageCallback(object? data) => OnMessage?.Invoke(data);
        [JSInvokable]
        public void OnErrorCallback(object? data) => OnError?.Invoke(data);
        [JSInvokable]
        public void OnMessageErrorCallback(object data) => OnMessageError?.Invoke(data);
        public ValueTask PostMessage(object data) => js.InvokeVoidAsync("CSL.PostMessage", new object[] { id, data });

        private bool disposed = false;
        public async void Dispose()
        {
            if (initialized)
            {
                await js.InvokeVoidAsync("CSL.DestroyWorker", new object[] { id });
                initialized = false;
            }
            objref.Dispose();
            disposed = true;
        }
    }
}
