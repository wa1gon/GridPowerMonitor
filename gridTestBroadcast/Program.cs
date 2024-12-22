using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace gridBroadCast
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 9123);
            string message = "Device: Test-Grid";
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);

            while (true)
            {
                try
                {
                    udpClient.Send(sendBytes, sendBytes.Length, endPoint);
                    Console.WriteLine("Broadcast message sent: " + message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(2000); // Wait for 2 seconds
            }
        }
    }
}
