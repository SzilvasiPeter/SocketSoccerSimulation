using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SoccerSimulation.Modell;

namespace SoocerGoalkeeperClient
{
    class Program
    {
        public static Socket _socket;

        public static void Main()
        {
            //init
            IPAddress ipAd = IPAddress.Loopback;
            TcpListener myList = new TcpListener(ipAd, 8001);

            while (true)
                try
                {
                    myList.Start();
                    Socket s = myList.AcceptSocket();
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
                    //recive
                    byte[] MessageRecive = new byte[1000];
                    s.Receive(MessageRecive);
                    var MessageReciveString = System.Text.Encoding.Default.GetString(MessageRecive).Split("\0")
                        .FirstOrDefault();
                    var shot = JsonSerializer.Deserialize<SoccerSimulation.Modell.Shot>(MessageReciveString);
                    //calc

                    //send

                   var minimum = shot.ScreenWidth * 0.375;
                    var maximum= shot.ScreenWidth * 0.623;
                    var MessageSend = new GoalkeeperRequestedPosition() { X =(int) (new Random().Next((int)minimum,(int)maximum))};
                    s.Send(System.Text.Encoding.Default.GetBytes(JsonSerializer.Serialize(MessageSend)));
                    Console.WriteLine("end");
                    s.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error..... " + e.StackTrace);
                    myList.Stop();
                }

            //close

            myList.Stop();
        }
    }
}
