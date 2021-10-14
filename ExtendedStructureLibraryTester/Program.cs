using System;
using System.Threading.Tasks;

namespace ExtendedStructureLibraryTester
{
    class Program
    {
        private static Task testClient()
        {
            Task main = new Task(() => CSL.Sockets.Clients.TCPClient("25892", "hello", "127.0.0.1"));

            return main;
        }
        private static Task testServer()
        {
            Task main = new Task(() => CSL.Sockets.Servers.TCPServer(new CSL.Sockets.ServerInfo("127.0.0.1", "25892", null)));

            return main;
        }
        
        //main function of ESL tester
        static void Main(string[] args)
        {
            Console.WriteLine("Tester Init!");

            //add logic
        }
    }
}
