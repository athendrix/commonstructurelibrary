using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;
using CSL.Blazor;
using CSL.Testing;
using System.Threading.Tasks;
using System.IO;

namespace CommonStructureLibraryTester.Testing
{
    public class BlazorTests : Tests
    {
        private static IJSRuntime? js;
        public static void SetJSRuntime(IJSRuntime js) => BlazorTests.js = js;
        [TestType(TestType.ClientSide)]
        protected static async Task<TestResponse> WebWorkerTest()
        {
            if(js == null)
            {
                return FAIL("JS Runtime Environment not set.");
            }
            await js.InitCSLJS();
            using (WebWorker ww = new WebWorker(js, "/DemoWorker.js"))
            {
                await ww.Init();
                TaskCompletionSource<string?> tcs = new TaskCompletionSource<string?>();
                ww.OnMessage += (object? data) => tcs.TrySetResult(data?.ToString() ?? "");
                ww.OnError += (object? error) => tcs.TrySetException(new Exception(error?.ToString() ?? ""));
                ww.OnMessageError += (object? error) => tcs.TrySetException(new Exception(error?.ToString() ?? ""));
                await ww.PostMessage("This is a test!");
                string? toReturn = await await Task.WhenAny(tcs.Task, Task.Delay(3000).ContinueWith<string?>((x) => null));
                if(toReturn == "Message received: This is a test!")
                {
                    return PASS();
                }
                return FAIL(toReturn ?? "No response from WebWorker.");
            }
        }
    }
}

