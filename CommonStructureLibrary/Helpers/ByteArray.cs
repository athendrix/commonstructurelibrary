using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CSL.Helpers
{
    public static class ByteArray
    {
        public static string EncodeToWebBase64(this byte[] input) => Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        public static byte[] DecodeFromWebBase64(this string input)
        {
            int len = input.Length;
            return Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/').PadRight(len + ((4 - (len % 4)) % 4), '='));
        }
        public static string EncodeToHexString(this byte[] input) => BitConverter.ToString(input).Replace("-", "");
        public static byte[] DecodeFromHexString(this string input)
        {
            byte[] toReturn = new byte[input.Length / 2];
            for(int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            return toReturn;
        }
            
        
    }
}
