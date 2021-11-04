using System;
using System.Collections.Generic;
using System.Text;

namespace CSL.Helpers
{
    public static class WebBase64
    {
        [Obsolete("Use ByteArray extensions.")]
        public static string Encode(byte[] input) => Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        [Obsolete("Use ByteArray extensions.")]
        public static byte[] Decode(string input)
        {
            int len = input.Length;
            return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/').PadRight(len + ((4 - (len % 4)) % 4), '='));
        }
    }
}
