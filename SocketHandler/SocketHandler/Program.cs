using System;
using System.Net.Sockets;

namespace SocketHandler
{
    class Program
    {
        static void Main()
        {
            TcpListener listener = new TcpListener(1000);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                string date = DateTime.Now.ToLongDateString();
                byte[] buf = System.Text.Encoding.ASCII.GetBytes(date);
                stream.Write(buf, 0, date.Length);
            }
        }
    }
}
