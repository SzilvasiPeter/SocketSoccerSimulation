using System;
using System.Net;
using System.Net.Sockets;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 5500);
            using (Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                socket.Listen(100);

                while (true)
                {
                    ReadTwoByte(socket);
                    //ReadToBuffer(socket);
                }
            }
        }

        private static void ReadToBuffer(Socket socket)
        {
            Socket acceptedSocket = socket.Accept();
            using (NetworkStream stream = new NetworkStream(acceptedSocket, true))
            {
                int length = stream.ReadByte();

                byte[] buffer = new byte[length];
                int remaining = length;
                int offset = 0;

                while (remaining > 0)
                {
                    int numberOfBytesRead = stream.Read(buffer, offset, remaining);
                    offset += numberOfBytesRead;
                    remaining -= numberOfBytesRead;
                }

                foreach (var singleByte in buffer)
                {
                    Console.WriteLine(singleByte);
                }
            }
        }

        private static void ReadTwoByte(Socket socket)
        {
            Socket acceptedSocket = socket.Accept();
            int xCoordinate;
            int yCoordinate;
            using (NetworkStream stream = new NetworkStream(acceptedSocket, true))
            {
                xCoordinate = stream.ReadByte();
                yCoordinate = stream.ReadByte();
            }

            Console.WriteLine(xCoordinate);
            Console.WriteLine(yCoordinate);

            float normalizedXCoordinate = (float)xCoordinate / 10f;
            float normalizedYCoordinate = (float)yCoordinate / 10f;

            Console.WriteLine(normalizedXCoordinate);
            Console.WriteLine(normalizedYCoordinate);
        }
    }
}
