using System;

namespace CSL
{
    public class Class1
    {
        public static string PassGen(int length = 16)
        {
            int templen = length/4 + (length %4 > 0?1:0);
            byte[] toReturn = new byte[templen * 3];
            using(System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create()) {rng.GetBytes(toReturn);}
            return Convert.ToBase64String(toReturn).Substring(0, length).Replace('/', '_').Replace('+', '-');
        }
    }
}
