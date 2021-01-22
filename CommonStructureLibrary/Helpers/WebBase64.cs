using System;
using System.Collections.Generic;
using System.Text;

namespace CSL.Helpers
{
    public static class WebBase64
    {
        public static string Encode(byte[] input)
        {
            return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
        public static byte[] Decode(string input)
        {
            int len = input.Length;
            return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/').PadRight(len + ((4 - (len % 4)) % 4), '='));
        }
    }
}
