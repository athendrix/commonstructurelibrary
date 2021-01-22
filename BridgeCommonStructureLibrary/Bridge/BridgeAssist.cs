using Bridge;
using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Bridge
{
    public static class BridgeAssist
    {
        public static string CreateObjectURL(Blob blob)
        {
            return Script.Call<string>("window.URL.createObjectURL",blob);
        }
        public static void RevokeObjectURL(string href)
        {
            Script.Call("window.URL.revokeObjectURL", href);
        }
        public static Task<T> MakePromise<T>(Action<Action<T>, Action<Error>> promiseCallback)
        {
            IPromise promise = Script.Call<IPromise>("new Promise", promiseCallback);
            Func<Task<T>> toReturn = async () =>
            {
                return (T)(await Task.FromPromise(promise))[0];
            };
            return toReturn();
        }
        public static Task MakePromise(Action<Action, Action<Error>> promiseCallback)
        {
            IPromise promise = Script.Call<IPromise>("new Promise", promiseCallback);
            Func<Task> toReturn = async () =>
            {
                await Task.FromPromise(promise);
                return;
            };
            return toReturn();
        }
        public static string MakeFunctionURL<T,TResult>(Func<T,TResult> func, params string[] imports)
        {
            string code = "";
            HTMLAnchorElement A = new HTMLAnchorElement();
            foreach (string s in imports)
            {
                if (s.EndsWith(".js"))
                {
                    A.Href = s.ToLower();
                }
                else
                {
                    A.Href = s.ToLower() + ".js";
                }

                code += "importScripts('" + A.Href + "');\n";
            }
            code += "self.onmessage = function(e)\n" +
                "{\n" +
                "    self.postMessage(" + func.ToString() + "(e.data));\n" +
                "};";
            Blob blob = new Blob(new BlobDataObject[] { code }, new BlobPropertyBag() { Type = "text/javascript" });
            return CreateObjectURL(blob);
        }
        private static HTMLInputElement OpenFileInput = null;
        private static Action<Error> PreviousRejection = null;
        public static Task<ArrayBuffer> OpenFile(string accept = null)
        {
            return MakePromise<ArrayBuffer>((Resolve, Reject) =>
            {
                try
                {
                    if (OpenFileInput != null)
                    {
                        OpenFileInput.Remove();
                    }
                }
                catch { }
                OpenFileInput = new HTMLInputElement();
                try
                {
                    PreviousRejection?.Invoke(new Error() { Message = "User Canceled File Opening" });
                }
                catch { }
                PreviousRejection = Reject;
                OpenFileInput.Type = InputType.File;
                if (accept != null)
                {
                    OpenFileInput.Accept = accept;
                }
                OpenFileInput.Hidden = true;
                OpenFileInput.OnChange = (Event<HTMLInputElement> e) =>
                {
                    if (OpenFileInput.Files != null && OpenFileInput.Files.Length > 0)
                    {
                        FileReader fr = new FileReader();
                        fr.OnLoad = (Event ev) =>
                        {
                            PreviousRejection = null;
                            Resolve((ArrayBuffer)fr.Result);
                        };
                        fr.OnError = (Event ev) =>
                        {
                            PreviousRejection = null;
                            Reject(new Error() { Message = "File Read Error" });
                        };
                        fr.ReadAsArrayBuffer(OpenFileInput.Files[0]);
                    }
                    else
                    {
                        Reject(new Error() { Message = "File Not Found Error" });
                    }
                };
                Document.Body.AppendChild(OpenFileInput);
                OpenFileInput.Click();
            });
        }
        private static void ClearOpenFileInput(bool reject = true)
        {
            try
            {
                if (OpenFileInput != null)
                {
                    OpenFileInput.Remove();
                }
            }
            catch { }
            OpenFileInput = null;
            try
            {
                PreviousRejection?.Invoke(new Error() { Message = "User Canceled File Opening" });
            }
            catch { }
            PreviousRejection = null;
        }
        public static byte[] ToArray(this Uint8Array input)
        {
            int length = input.Length;
            byte[] toReturn = new byte[length];
            for(int i = 0; i < length; i++)
            {
                toReturn[i] = input[i];
            }
            return toReturn;
        }
        public static Uint8Array ToUint8Array(this byte[] input)
        {
            int length = input.Length;
            Uint8Array toReturn = new Uint8Array(length);
            for (int i = 0; i < length; i++)
            {
                toReturn[i] = input[i];
            }
            return toReturn;
        }
    }
}
