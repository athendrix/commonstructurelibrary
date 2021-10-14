using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CSL.Sockets
{
    public class Server
    {
        public static void TCPServer(string port, string? data, string host = "localhost")
        {
            IPAddress ip = IPAddress.Parse(host);

            try
            {
                TcpListener Listener = new TcpListener(ip, Convert.ToInt32(port));

                Listener.Start();

                Console.WriteLine($"({host}) | Listening on {port}");
                Console.WriteLine($"Local endpoint is {Listener.LocalEndpoint}");

                Console.WriteLine("Waiting for connection...");

                Socket socket = Listener.AcceptSocket();
                Console.WriteLine($"New connection ({socket.RemoteEndPoint})");

                byte[] x = new byte[100];
                int y = socket.Receive(x);
                Console.WriteLine("Incoming data...");

                for (int m = 0; m < y; m++)
                {
                    Console.Write(Convert.ToChar(x[m]));
                }
                Console.WriteLine("");

            } catch (Exception ex)
            {
                Console.WriteLine("Failed to bind to port " + port + ". Maybe it is being used by another process?");
                Console.WriteLine("EXCEPTION: " + ex.Message);
            }

        }
    
    }
}
