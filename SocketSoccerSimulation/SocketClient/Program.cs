using System;
using System.Net;
using System.Net.Sockets;

namespace SocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 5500);
            using (Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(endPoint);
                using (NetworkStream stream = new NetworkStream(socket, true))
                {
                    int length = 3;
                    byte[] data = new byte[length];
                    Random random = new Random();
                    for (int i = 0; i < length; i++)
                    {
                        data[i] = (byte)random.Next(255);
                    }

                    foreach (var singleByte in data)
                    {
                        Console.WriteLine(singleByte);
                    }

                    stream.WriteByte((byte)length);
                    stream.Write(data, 0, length);
                }
            }

            Console.ReadLine();
        }
    }
}
