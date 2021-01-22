using CSL.Encryption;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommonStructureLibraryTester
{
    class Program
    {
        public static void Main(string[] args)
        {
            AsyncMain(args).GetAwaiter().GetResult();
        }
        public static async Task AsyncMain(string[] args)
        {
            Console.WriteLine("Enter Decryption Key (Enter no key to Encrypt with new key.)");
            string keybase = Console.ReadLine();
            if(string.IsNullOrWhiteSpace(keybase))
            {
                using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector())
                {
                    Console.WriteLine("Key:" + Convert.ToBase64String(protector.GetKey()));
                    Console.WriteLine("Enter Plain Text");
                    string PlaintextString = Console.ReadLine();
                    Console.WriteLine("Protected Text:");
                    Console.WriteLine(await protector.Protect(PlaintextString));
                }
            }
            else
            {
                byte[] key = Convert.FromBase64String(keybase);
                Console.WriteLine("Enter Protected Text");
                string ProtectedString = Console.ReadLine();
                using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector(key))
                {
                    Console.WriteLine("Decrypted Text:");
                    Console.WriteLine(await protector.Unprotect(ProtectedString));
                }
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
