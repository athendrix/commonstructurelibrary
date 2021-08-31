using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSL.Helpers;

namespace CSL.Blazor
{
    /// <summary>
    /// Wrapper around the IJSRuntime interface so it can be injected.
    /// </summary>
    public class CSLJSRuntime
    {
        #region Main Stuff
        private dynamic JSRuntime;
        public CSLJSRuntime(dynamic JSRuntime)
        {
            this.JSRuntime = JSRuntime;
        }

        public async ValueTask InvokeVoidAsync(string identifier, params object[] args) => await JSRuntime.InvokeAsync<object>(identifier, args);

        //
        // Summary:
        //     Invokes the specified JavaScript function asynchronously.
        //     Microsoft.JSInterop.JSRuntime will apply timeouts to this operation based on
        //     the value configured in Microsoft.JSInterop.JSRuntime.DefaultAsyncTimeout. To
        //     dispatch a call with a different timeout, or no timeout, consider using Microsoft.JSInterop.IJSRuntime.InvokeAsync``1(System.String,System.Threading.CancellationToken,System.Object[]).
        //
        // Parameters:
        //   identifier:
        //     An identifier for the function to invoke. For example, the value "someScope.someFunction"
        //     will invoke the function window.someScope.someFunction.
        //
        //   args:
        //     JSON-serializable arguments.
        //
        // Type parameters:
        //   TValue:
        //     The JSON-serializable return type.
        //
        // Returns:
        //     An instance of TValue obtained by JSON-deserializing the return value.
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object[] args) => JSRuntime.InvokeAsync<TValue>(identifier, args);
        //
        // Summary:
        //     Invokes the specified JavaScript function asynchronously.
        //
        // Parameters:
        //   identifier:
        //     An identifier for the function to invoke. For example, the value "someScope.someFunction"
        //     will invoke the function window.someScope.someFunction.
        //
        //   cancellationToken:
        //     A cancellation token to signal the cancellation of the operation. Specifying
        //     this parameter will override any default cancellations such as due to timeouts
        //     (Microsoft.JSInterop.JSRuntime.DefaultAsyncTimeout) from being applied.
        //
        //   args:
        //     JSON-serializable arguments.
        //
        // Type parameters:
        //   TValue:
        //     The JSON-serializable return type.
        //
        // Returns:
        //     An instance of TValue obtained by JSON-deserializing the return value.
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, params object[] args) => InvokeAsync<TValue>(identifier, cancellationToken, args);
        #endregion
        #region Local Storage
        public async Task<T> GetLocal<T>(string key)
        {
            string item = await InvokeAsync<string>("localStorage.getItem", key);
            if (Generics.TryParse(item, out T toReturn))
            {
                return toReturn;
            }
            return default(T);
        }
        public ValueTask SetLocal<T>(string key, T value)
        {
            if (value == null)
            {
                return InvokeVoidAsync("localStorage.removeItem", key);
            }
            return InvokeVoidAsync("localStorage.setItem", key, Generics.ToString(value));
        }
        public ValueTask ClearLocal()
        {
            return InvokeVoidAsync("localStorage.clear");
        }
        #endregion
        #region Local Storage
        public async Task<T> GetSession<T>(string key)
        {
            string item = await InvokeAsync<string>("sessionStorage.getItem", key);
            if (Generics.TryParse(item, out T toReturn))
            {
                return toReturn;
            }
            return default(T);
        }
        public ValueTask SetSession<T>(string key, T value)
        {
            if (value == null)
            {
                return InvokeVoidAsync("sessionStorage.removeItem", key);
            }
            return InvokeVoidAsync("sessionStorage.setItem", key, Generics.ToString(value));
        }
        public ValueTask ClearSession()
        {
            return InvokeVoidAsync("sessionStorage.clear");
        }
        #endregion
    }
}
