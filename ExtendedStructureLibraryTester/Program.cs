using System;

namespace ExtendedStructureLibraryTester
{
    public class Program
    {
        //main function of ESL tester
        static void Main(string[] args)
        {
            Console.WriteLine("Tester Init!");

            //add logic

            CSL.Sockets.Server.TCPServer("25565", null, "127.0.0.1");
            CSL.Sockets.Client.TCPClient("88081", "Hello World!");
        }
    }
}
