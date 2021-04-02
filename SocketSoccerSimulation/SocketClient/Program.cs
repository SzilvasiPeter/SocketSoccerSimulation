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
                    ReadTwoByte(stream);
                    // ReadToByteBuffer(stream);
                }
            }

            Console.ReadLine();
        }

        private static void ReadTwoByte(NetworkStream stream)
        {
            Random random = new Random();
            byte xCoordinate = (byte)(random.Next(11) + 1);
            byte yCoordinate = (byte)(random.Next(91) + 10);

            stream.WriteByte(xCoordinate);
            stream.WriteByte(yCoordinate);

            Console.WriteLine(xCoordinate);
            Console.WriteLine(yCoordinate);
        }

        private static void ReadToByteBuffer(NetworkStream stream)
        {
            int length = 2;
            byte[] data = new byte[length];
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte)(random.Next(100) + 1);
            }

            foreach (var singleByte in data)
            {
                Console.WriteLine(singleByte);
            }

            stream.WriteByte((byte)length);
            stream.Write(data, 0, length);
        }
    }
}
