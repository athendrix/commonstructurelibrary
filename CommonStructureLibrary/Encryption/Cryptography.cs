using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CSL.Encryption
{
    public static class Cryptography
    {
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        public static byte[] GetRandomBytes(int length)
        {
            byte[] toReturn = new byte[length];
            GetRandomBytes(toReturn);
            return toReturn;
        }
        public static void GetRandomBytes(byte[] data)
        {
            lock (rng)
            {
                rng.GetBytes(data);
            }
        }
        //MD5 is not supported on Blazor
        #region MD5
        private static readonly object md5locker = new object();
        private static MD5? _md5 = null;
        private static MD5 md5 => _md5 ??= MD5.Create();
        [Obsolete("MD5 is a deprecated hashing function", false)]
        public static byte[] MD5Hash(byte[] data)
        {
            lock (md5locker)
            {
                return md5.ComputeHash(data);
            }
        }
        [Obsolete("MD5 is a deprecated hashing function", false)]
        public static byte[] MD5Hash(string data) => MD5Hash(Encoding.UTF8.GetBytes(data));
        [Obsolete("MD5 is a deprecated hashing function", false)]
        public static Guid MD5Guid(byte[] data) => new Guid(MD5Hash(data));
        [Obsolete("MD5 is a deprecated hashing function", false)]
        public static Guid MD5Guid(string data) => MD5Guid(Encoding.UTF8.GetBytes(data));
        #endregion
        #region SHA1

        private static readonly SHA1 sha1 = SHA1.Create();
        [Obsolete("SHA1 is a deprecated hashing function", false)]
        public static byte[] SHA1Hash(byte[] data)
        {
            lock (sha1)
            {
                return sha1.ComputeHash(data);
            }
        }
        [Obsolete("SHA1 is a deprecated hashing function", false)]
        public static byte[] SHA1Hash(string data) => SHA1Hash(Encoding.UTF8.GetBytes(data));
        [Obsolete("SHA1 is a deprecated hashing function", false)]
        public static Guid SHA1Guid(byte[] data) => new Guid(SHA1Hash(data));
        [Obsolete("SHA1 is a deprecated hashing function", false)]
        public static Guid SHA1Guid(string data) => SHA1Guid(Encoding.UTF8.GetBytes(data));
        #endregion
        #region SHA256
        private static readonly SHA256 sha256 = SHA256.Create();
        public static byte[] SHA256Hash(byte[] data)
        {
            lock (sha256)
            {
                return sha256.ComputeHash(data);
            }
        }
        public static byte[] SHA256Hash(string data) => SHA256Hash(Encoding.UTF8.GetBytes(data));
        public static Guid[] SHA256Guids(byte[] data)
        {
            Span<byte> hash = SHA256Hash(data);
            return new Guid[]
            {
                new Guid(hash.Slice(0, 16).ToArray()),
                new Guid(hash.Slice(16, 16).ToArray())
            };
        }
        public static Guid[] SHA256Guids(string data) => SHA256Guids(Encoding.UTF8.GetBytes(data));
        #endregion
        #region SHA384
        private static readonly SHA384 sha384 = SHA384.Create();
        public static byte[] SHA384Hash(byte[] data)
        {
            lock (sha384)
            {
                return sha384.ComputeHash(data);
            }
        }
        public static byte[] SHA384Hash(string data) => SHA384Hash(Encoding.UTF8.GetBytes(data));
        public static Guid[] SHA384Guids(byte[] data)
        {
            Span<byte> hash = SHA384Hash(data);
            return new Guid[]
            {
                new Guid(hash.Slice(0, 16).ToArray()),
                new Guid(hash.Slice(16, 16).ToArray()),
                new Guid(hash.Slice(32, 16).ToArray()),
            };
        }
        public static Guid[] SHA384Guids(string data) => SHA384Guids(Encoding.UTF8.GetBytes(data));
        #endregion
        #region SHA512
        private static readonly SHA512 sha512 = SHA512.Create();
        public static byte[] SHA512Hash(byte[] data)
        {
            lock (sha512)
            {
                return sha512.ComputeHash(data);
            }
        }
        public static byte[] SHA512Hash(string data) => SHA512Hash(Encoding.UTF8.GetBytes(data));
        public static Guid[] SHA512Guids(byte[] data)
        {
            Span<byte> hash = SHA512Hash(data);
            return new Guid[]
            {
                new Guid(hash.Slice(0, 16).ToArray()),
                new Guid(hash.Slice(16, 16).ToArray()),
                new Guid(hash.Slice(32, 16).ToArray()),
                new Guid(hash.Slice(48, 16).ToArray()),
            };
        }
        public static Guid[] SHA512Guids(string data) => SHA512Guids(Encoding.UTF8.GetBytes(data)); 
        #endregion
    }
}
