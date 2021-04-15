using SoccerSimulation.Modell;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SoocerGoalkeeperClient
{
    internal class Program
    {
      
        public static void Main()
        {
            //init
            IPAddress ipAd = IPAddress.Loopback;
            TcpListener myList = new TcpListener(ipAd, 8001);

            while (true)
            {
                try
                {
                    myList.Start();
                    Socket s = myList.AcceptSocket();
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
                 
                    //recive
                    byte[] MessageRecive = new byte[1000];
                    s.Receive(MessageRecive);
                    string MessageReciveString = Encoding.Default.GetString(MessageRecive).Split("\0")
                        .FirstOrDefault();
                    Shot shot = JsonSerializer.Deserialize<Shot>(MessageReciveString);
                   
                    //calc
                    var shotPixelRangeReal = CalculatePixelCoordinate(shot);

                    //send
                    GoalkeeperRequestedPosition MessageSend = new GoalkeeperRequestedPosition { X = (int)shotPixelRangeReal };
                    s.Send(Encoding.Default.GetBytes(JsonSerializer.Serialize(MessageSend)));
                    Console.WriteLine("end");
                    s.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error..... " + e.StackTrace);
                    myList.Stop();
                }
            }

            //close

            myList.Stop();
        }

        private static double CalculatePixelCoordinate(Shot shot)
        {
            Random rand = new Random();
            float shootDirection = 1;
            if (rand.Next(100) <= 75)
            {
                shootDirection = -1;
            }

            double minimum = shot.ScreenWidth * 0.375;
            double maximum = shot.ScreenWidth * 0.623;
            double middle = (minimum + maximum) / 2;
            double range = maximum - minimum;
            double oneUnitPixelrange = range / 20;
            double shotPixelRange = oneUnitPixelrange * shot.X * shootDirection;
            double shotPixelRangeReal = shotPixelRange + middle;
            return shotPixelRangeReal;
        }
    }
}