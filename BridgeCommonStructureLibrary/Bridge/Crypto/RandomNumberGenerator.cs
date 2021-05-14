using Bridge;
using Bridge.Html5;

namespace CSL.Bridge.Crypto
{
    public class RandomNumberGenerator
    {
        public static RandomNumberGenerator Create()
        {
            return new RandomNumberGenerator();
        }
        public static void Fill(Uint8Array toFill)
        {
            Script.Call("window.crypto.getRandomValues", toFill);
        }
        public void GetBytes(Uint8Array toFill) => Fill(toFill);
            
    }
}
