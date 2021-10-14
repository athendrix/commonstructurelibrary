using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSL.Sockets
{
    public record ServerInfo(string target, string port, string? data, string? name = null);


    public class Servers
    {
        public static void TCPServer(ServerInfo args)
        {
            IPAddress ip = IPAddress.Parse(args.target);

            try
            {
                TcpListener Listener = new TcpListener(ip, Convert.ToInt32(args.port));

                Listener.Start();

                Console.WriteLine($"({args.target}) | Listening on {args.port}");
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

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to bind to port " + args.port + ". Maybe it is being used by another process?");
                Console.WriteLine("EXCEPTION: " + ex.Message);
            }
        }
    }
}
