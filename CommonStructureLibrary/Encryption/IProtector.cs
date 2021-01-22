using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSL.Encryption
{
    public interface IProtector : IDisposable
    {
        /// <summary>
        /// Protects some data such that it cannot be read without calling Unprotect using the same protector, and the same purpose string.
        /// </summary>
        /// <param name="input">Data to protect</param>
        /// <param name="purpose">Purpose string</param>
        /// <returns>Protected Data</returns>
        Task<string> Protect(string input, string purpose = null);
        /// <summary>
        /// Protects some data such that it cannot be read without calling Unprotect using the same protector, and the same purpose string.
        /// </summary>
        /// <param name="input">Data to protect</param>
        /// <param name="purpose">Purpose string</param>
        /// <returns>Protected Data</returns>
        Task<string> Protect(byte[] input, string purpose = null);
        /// <summary>
        /// Unprotects some data that was protected using the same protector, and the same purpose string.
        /// </summary>
        /// <param name="input">Data to unprotect</param>
        /// <param name="purpose">Purpose string</param>
        /// <returns>Unprotected Data</returns>
        Task<string> Unprotect(string input, string purpose = null);
        /// <summary>
        /// Unprotects some data that was protected using the same protector, and the same purpose string.
        /// </summary>
        /// <param name="input">Data to unprotect</param>
        /// <param name="purpose">Purpose string</param>
        /// <returns>Unprotected Data</returns>
        Task<byte[]> UnprotectRaw(string input, string purpose = null);
    }
}
