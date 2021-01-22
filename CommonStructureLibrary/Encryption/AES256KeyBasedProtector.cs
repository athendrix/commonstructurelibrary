using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if BRIDGE
using Bridge.Html5;
using CSL.Bridge;
using CSL.Bridge.Crypto;
#else
using System.Security.Cryptography;
#endif




namespace CSL.Encryption
{
    public class AES256KeyBasedProtector : IProtector
    {
#if BRIDGE
        private Uint8Array key;
#else
        private byte[] key;
#endif
        public AES256KeyBasedProtector()
        {
#if BRIDGE
            key = new Uint8Array(32);
#else
            key = new byte[32];
#endif
            RandomNumberGenerator.Fill(key);
        }
        public AES256KeyBasedProtector(byte[] key)
        {
            if (key.Length != 32)
            {
                throw new ArgumentException("Invalid Key Size");
            }
#if BRIDGE
            this.key = key.ToUint8Array();
#else
            this.key = (byte[])key.Clone();
#endif
        }
        public byte[] GetKey()
        {
#if BRIDGE
            return key.ToArray();
#else
            return (byte[])key.Clone();
#endif

        }

        public async Task<string> Protect(string input, string purpose = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input cannot be null.");
            }
            return await Protect(Encoding.UTF8.GetBytes(input), purpose);
        }

        public async Task<string> Protect(byte[] input, string purpose = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input cannot be null.");
            }
            //int tagsize = AesGcm.TagByteSizes.MaxSize;
            int tagsize = 16;
            //int noncesize = AesGcm.NonceByteSizes.MaxSize;
            int noncesize = 12;

#if BRIDGE
            Uint8Array toReturn;
            Uint8Array purposebytes = purpose == null ? null : Encoding.UTF8.GetBytes(purpose).ToUint8Array();
#else
            byte[] toReturn;
            byte[] purposebytes = purpose == null ? null : Encoding.UTF8.GetBytes(purpose);
#endif
            using (AesGcm cipher = new AesGcm(key))
            {
#if BRIDGE
                Uint8Array plaintext = input.ToUint8Array();
                toReturn = new Uint8Array(tagsize + noncesize + plaintext.Length);
                Uint8Array tag = toReturn.SubArray(0, tagsize);
                Uint8Array nonce = toReturn.SubArray(tagsize, noncesize);
                Uint8Array ciphertext = toReturn.SubArray(tagsize + noncesize);
                RandomNumberGenerator.Fill(nonce);
                await cipher.Encrypt(nonce, plaintext, ciphertext, tag, purposebytes);
#else
                byte[] plaintext = input;
                toReturn = await Task.Run(() => EasyEncrypt(cipher, plaintext, tagsize, noncesize, purposebytes));
#endif
                plaintext = null;
            }
#if BRIDGE
            return Convert.ToBase64String(toReturn.ToArray());
#else
            return Convert.ToBase64String(toReturn);
#endif
        }
#if !BRIDGE
        private static byte[] EasyEncrypt(AesGcm cipher, byte[] plaintext, int tagsize, int noncesize, byte[] purposebytes)
        {
            byte[] toReturn = new byte[tagsize + noncesize + plaintext.Length];
            Span<byte> tag = toReturn.AsSpan(0, tagsize);
            Span<byte> nonce = toReturn.AsSpan(tagsize, noncesize);
            Span<byte> ciphertext = toReturn.AsSpan(tagsize + noncesize);
            RandomNumberGenerator.Fill(nonce);
            cipher.Encrypt(nonce, plaintext, ciphertext, tag, purposebytes);
            return toReturn;
        }
#endif

        public async Task<string> Unprotect(string input, string purpose = null)
        {
            return Encoding.UTF8.GetString(await UnprotectRaw(input, purpose));
        }

        public async Task<byte[]> UnprotectRaw(string input, string purpose = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input cannot be null.");
            }
            //int tagsize = AesGcm.TagByteSizes.MaxSize;
            int tagsize = 16;
            //int noncesize = AesGcm.NonceByteSizes.MaxSize;
            int noncesize = 12;
            byte[] purposebytes = purpose == null ? null : Encoding.UTF8.GetBytes(purpose);

            using (AesGcm cipher = new AesGcm(key))
            {

#if BRIDGE
                Uint8Array protectedText = Convert.FromBase64String(input).ToUint8Array();
                //Uint8Array plaintext = input.ToUint8Array();
                Uint8Array plaintexttemp = new Uint8Array(protectedText.Length - (tagsize + noncesize));
                Uint8Array tag = protectedText.SubArray(0, tagsize);
                Uint8Array nonce = protectedText.SubArray(tagsize, noncesize);
                Uint8Array ciphertext = protectedText.SubArray(tagsize + noncesize);
                await cipher.Decrypt(nonce, ciphertext, tag, plaintexttemp, purposebytes.ToUint8Array());
                byte[] plaintext = plaintexttemp.ToArray();
#else
                byte[] protectedText = Convert.FromBase64String(input);
                byte[] plaintext = await Task.Run(() => EasyDecrypt(cipher, protectedText, tagsize, noncesize, purposebytes));
#endif
                return plaintext;
            }
        }
#if !BRIDGE
        private static byte[] EasyDecrypt(AesGcm cipher, byte[] protectedText, int tagsize, int noncesize, byte[] purposebytes)
        {
            byte[] toReturn = new byte[protectedText.Length - (tagsize + noncesize)];
            Span<byte> tag = protectedText.AsSpan(0, tagsize);
            Span<byte> nonce = protectedText.AsSpan(tagsize, noncesize);
            Span<byte> ciphertext = protectedText.AsSpan(tagsize + noncesize);
            cipher.Decrypt(nonce, ciphertext, tag, toReturn, purposebytes);
            return toReturn;
        }
#endif

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    for (int i = 0; i < key.Length; i++)
                    {
                        key[i] = 0;
                    }
                }
                key = null;
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AES256KeyBasedProtector()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
