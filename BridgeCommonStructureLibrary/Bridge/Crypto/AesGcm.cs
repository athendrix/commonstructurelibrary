using Bridge;
using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Bridge.Crypto
{
    public class AesGcm : IDisposable
    {
        Uint8Array key;
        public AesGcm(Uint8Array key)
        {
            this.key = key;
        }

        public void Dispose()
        {
            if (key != null)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = 0;
                }
                key = null;
            }
        }

        internal async Task Decrypt(Uint8Array iv, Uint8Array ciphertext, Uint8Array plaintext, Uint8Array tag, Uint8Array additionalData)
        {
            int tLength = tag.Length;
            Uint8Array input = new Uint8Array(tLength + ciphertext.Length);
            for (int i = 0; i < tLength; i++)
            {
                input[i] = tag[i];
            }
            for (int i = 0; i < ciphertext.Length; i++)
            {
                input[i + tLength] = ciphertext[i];
            }
            Uint8Array output = (Uint8Array)(await Task.FromPromise(Script.Call<IPromise>("window.crypto.subtle.decrypt", new { name = "AES-GCM", iv, additionalData, tagLength = tLength * 8 }, key, input)))[0];
            for (int i = 0; i < output.Length; i++)
            {
                plaintext[i] = output[i];
            }
        }

        internal async Task Encrypt(Uint8Array iv, Uint8Array plaintext, Uint8Array ciphertext, Uint8Array tag, Uint8Array additionalData)
        {
            int tLength = tag.Length;
            Uint8Array result = (Uint8Array)(await Task.FromPromise(Script.Call<IPromise>("window.crypto.subtle.encrypt", new { name = "AES-GCM", iv, additionalData, tagLength = tLength * 8 }, key, plaintext)))[0];

            //Array.Copy(result, 0, tag, 0, tag.Length);
            for (int i = 0; i < tLength; i++)
            {
                tag[i] = result[i];
            }
            //Array.Copy(result, tag.Length, ciphertext, 0, ciphertext.Length);
            for (int i = 0; i < ciphertext.Length; i++)
            {
                ciphertext[i] = result[i + tLength];
            }
        }
    }
}
