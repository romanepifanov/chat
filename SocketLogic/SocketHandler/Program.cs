using SocketHandler.Controllers;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SocketHandler
{
    class Program
    {
        static void Main()
        {
            string ip = "127.0.0.1";
            int port = 80;
            var server = new TcpListener(IPAddress.Parse(ip), port);

            server.Start();
            Console.WriteLine("Server has started on {0}:{1}, Waiting for a connection...", ip, port);

            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("A client connected.");

            NetworkStream stream = client.GetStream();
            SocketService socketService = new SocketService(stream);

            while (true)
            {
                while (!stream.DataAvailable) ;
                while (client.Available < 3) ;

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, client.Available);
                string message = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(message, "^GET", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("=====Handshaking from client=====\n{0}", message);
                    socketService.CheckConnnect(message);
                }
                else
                {
                    bool mask = (bytes[1] & 0b10000000) != 0;
                    int msglen = bytes[1] - 128;
                    int offset = 2;

                    if (msglen == 126)
                    {
                        msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                        offset = 4;
                    }

                    if (msglen == 0)
                    {
                        Console.WriteLine("msglen == 0");
                    }
                    else if (mask)
                    {
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                        offset += 4;

                        for (int i = 0; i < msglen; ++i)
                        {
                            decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);
                        }

                        string text = Encoding.UTF8.GetString(decoded);
                        Console.WriteLine("{0}", text);
                    }
                    else
                    {
                        Console.WriteLine("mask bit not set");
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}
