using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Homework_NP_4
{
    public class Program
    {
        private const int PORT = 8888;

        static void Main()
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);

            Console.WriteLine("Enter the name of the component to reguest a price (or 'exit' to exit): ");

            while (true)
            {
                Console.Write("Reguest: ");
                string reguest = Console.ReadLine();
                if (reguest.ToLower() == "exit") break;

                byte[] data = Encoding.UTF8.GetBytes(reguest);
                udpClient.Send(data, data.Length, serverEndPoint);

                IPEndPoint remoteEndPoint = null;
                byte[] response = udpClient.Receive(ref remoteEndPoint);
                string responseText = Encoding.UTF8.GetString(response);

                Console.WriteLine($"Response from the server: {responseText}");
            }
            udpClient.Close();
        }
    }
}
