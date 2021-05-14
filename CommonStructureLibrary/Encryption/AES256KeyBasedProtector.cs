using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static CSL.DependencyInjection;

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
        private static RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        private byte[] key;

        public AES256KeyBasedProtector()
        {
            key = new byte[32];
            RNG.GetBytes(key);
        }

        public AES256KeyBasedProtector(byte[] key)
        {
            if (key.Length != 32)
            {
                throw new ArgumentException("Invalid Key Size");
            }
            this.key = (byte[])key.Clone();
        }
        public byte[] GetKey()
        {
            return (byte[])key.Clone();
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

            byte[] toReturn;
            byte[] purposebytes = purpose == null ? null : Encoding.UTF8.GetBytes(purpose);
            using (IAesGcm cipher = CreateIAesGcm(key))
            {
                byte[] plaintext = input;
                toReturn = await Task.Run(() => EasyEncrypt(cipher, plaintext, tagsize, noncesize, purposebytes));
                plaintext = null;
            }
            return Convert.ToBase64String(toReturn);
        }

        private static byte[] EasyEncrypt(IAesGcm cipher, byte[] plaintext, int tagsize, int noncesize, byte[] purposebytes)
        {
            byte[] tag = new byte[tagsize];
            byte[] nonce = new byte[noncesize];
            byte[] ciphertext = new byte[plaintext.Length];
            RNG.GetBytes(nonce);
            cipher.Encrypt(nonce, plaintext, ciphertext, tag, purposebytes);
            byte[] toReturn = new byte[tagsize + noncesize + plaintext.Length];
            tag.CopyTo(toReturn,0);
            nonce.CopyTo(toReturn, tagsize);
            ciphertext.CopyTo(toReturn, tagsize + noncesize);
            return toReturn;
        }


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

            using (IAesGcm cipher = CreateIAesGcm(key))
            {
                byte[] protectedText = Convert.FromBase64String(input);
                byte[] plaintext = await Task.Run(() => EasyDecrypt(cipher, protectedText, tagsize, noncesize, purposebytes));
                return plaintext;
            }
        }

        private static byte[] EasyDecrypt(IAesGcm cipher, byte[] protectedText, int tagsize, int noncesize, byte[] purposebytes)
        {
            
            byte[] toReturn = new byte[protectedText.Length - (tagsize + noncesize)];
            byte[] tag = protectedText.AsSpan(0, tagsize).ToArray();
            byte[] nonce = protectedText.AsSpan(tagsize, noncesize).ToArray();
            byte[] ciphertext = protectedText.AsSpan(tagsize + noncesize).ToArray();
            cipher.Decrypt(nonce, ciphertext, tag, toReturn, purposebytes);
            return toReturn;
        }

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
