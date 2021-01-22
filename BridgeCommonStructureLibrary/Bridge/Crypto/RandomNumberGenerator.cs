using Bridge;
using Bridge.Html5;

namespace CSL.Bridge.Crypto
{
    public static class RandomNumberGenerator
    {
        public static void Fill(Uint8Array toFill)
        {
            Script.Call("window.crypto.getRandomValues", toFill);
        }
            
    }
}
