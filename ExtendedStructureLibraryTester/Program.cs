using System;
using System.Threading.Tasks;

namespace ExtendedStructureLibraryTester
{
    class Program
    {
        private static Task testClient()
        {
            CSL.Sockets.ServerInfo info = new CSL.Sockets.ServerInfo("127.0.0.1", "52869", null);

            Task main = new Task(() => CSL.Sockets.Clients.TCPClient(info));

            return main;
        }
        private static Task testServer()
        {
            CSL.Sockets.ServerInfo info = new CSL.Sockets.ServerInfo("127.0.0.1", "52869", null);

            Task main = new Task(() => CSL.Sockets.Servers.TCPServer(info));

            return main;
        }

        //main function of ESL tester
        static void Main(string[] args)
        {
            CSL.Sockets.ServerInfo info = new CSL.Sockets.ServerInfo("127.0.0.1", "52869", null);

            Console.WriteLine("Tester Init!");

            //add logic
            Task Server = testServer();
            Task Client = testClient();

            Server.Start();
            Client.Start();
        }
    }
}