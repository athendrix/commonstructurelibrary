using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace CSL.Sockets
{
    public class Clients
    {
        public static void TCPClient(string? port = null, string? data = null, string? target = "localhost")
        {
            TcpClient client = new TcpClient();

            Console.WriteLine($"Connecting to {target}:{port}");

            try
            {
                client.Connect(target, Convert.ToInt32(port));

                Console.WriteLine("Connected!");

                if (data != null)
                {
                    Console.WriteLine("data is: " + data);

                    Stream stream = client.GetStream();

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] x = encoding.GetBytes(data);

                    Console.WriteLine("Sending data...");

                    stream.Write(x, 0, x.Length);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to {target}:{port}");
            }


        }
    }
}
