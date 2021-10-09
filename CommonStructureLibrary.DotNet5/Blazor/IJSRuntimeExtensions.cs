using CSL.Helpers;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Blazor
{
    public static class IJSRuntimeExtensions
    {
        public static ValueTask InitCSLJS(this IJSRuntime js) => js.InvokeVoidAsync("eval", @"""usestrict"";
if (window.CSL === undefined) {
    window.CSL = {};
    window.CSL.Workers = {};
    window.CSL.CreateWorker = function (objref, id, WorkerPath) {
        if (id === undefined || id === null) { return false; }
        let newid = 'ID_' + id;
        if (window.CSL.Workers[newid] === undefined || window.CSL.Workers[newid] === null) {
            window.CSL.Workers[newid] = new Worker(WorkerPath);
            window.CSL.Workers[newid].objref = objref;
            window.CSL.Workers[newid].onmessage = (e) => {
                e.currentTarget.objref.invokeMethodAsync('OnMessageCallback', e.data)
            };
            window.CSL.Workers[newid].onerror = (e) => {
                e.currentTarget.objref.invokeMethodAsync('OnErrorCallback', e.message)
            };
            window.CSL.Workers[newid].onmessageerror = (e) => {
                e.currentTarget.objref.invokeMethodAsync('OnMessageErrorCallback', e.message)
            };
            return true;
        }
        return false;
    }
    window.CSL.PostMessage = function (id, message) {
        if (id === undefined || id === null) { return false; }
        let newid = 'ID_' + id;
        if (window.CSL.Workers[newid] !== undefined && window.CSL.Workers[newid] !== null) {
            window.CSL.Workers[newid].postMessage(message);
            return true;
        }
        return false;
    }
    window.CSL.DestroyWorker = function (id) {
        if (id === undefined || id === null) { return false; }
        let newid = 'ID_' + id;
        if (window.CSL.Workers[newid] !== undefined && window.CSL.Workers[newid] !== null) {
            window.CSL.Workers[newid].terminate();
            delete window.CSL.Workers[newid];
            return true;
        }
        return false;
    }
}
null;");
        #region Local Storage
        public static async Task<T?> GetLocal<T>(this IJSRuntime js, string key)
        {
            string item = await js.InvokeAsync<string>("localStorage.getItem", key);
            if (Generics.TryParse(item, out T? toReturn))
            {
                return toReturn;
            }
            return default(T?);
        }
        public static ValueTask SetLocal<T>(this IJSRuntime js, string key, T? value)
        {
            string? strvalue = Generics.ToString(value);
            if (strvalue == null)
            {
                return js.InvokeVoidAsync("localStorage.removeItem", key);
            }
            return js.InvokeVoidAsync("localStorage.setItem", key, strvalue);
        }
        public static ValueTask ClearLocal(this IJSRuntime js) => js.InvokeVoidAsync("localStorage.clear");
        #endregion
        #region Session Storage
        public static async Task<T?> GetSession<T>(this IJSRuntime js, string key)
        {
            string item = await js.InvokeAsync<string>("sessionStorage.getItem", key);
            if (Generics.TryParse(item, out T? toReturn))
            {
                return toReturn;
            }
            return default(T?);
        }
        public static ValueTask SetSession<T>(this IJSRuntime js, string key, T? value)
        {
            string? strvalue = Generics.ToString(value);
            if (strvalue == null)
            {
                return js.InvokeVoidAsync("sessionStorage.removeItem", key);
            }
            return js.InvokeVoidAsync("sessionStorage.setItem", key, strvalue);
        }
        public static ValueTask ClearSession(this IJSRuntime js) => js.InvokeVoidAsync("sessionStorage.clear");
        #endregion
    }
}
