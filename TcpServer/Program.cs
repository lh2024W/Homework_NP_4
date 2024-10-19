using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Timers;

namespace TcpServer
{
    public class Program
    {
        private const int PORT = 8888;
        private static UdpClient udpServer;
        private static Dictionary<string, string> components = new Dictionary<string, string>()
        {
            {"Processor", "489$" },
            {"Video card", "800$" },
            { "RAM", "250$" }
        };

        private static Dictionary<IPEndPoint, (int запросы, DateTime последнийЗапрос)> clientRequests = new ();
        private static int maxRequestsPerHour = 10;
        private static int maxClients = 100;
        private static TimeSpan inactiveTimeout = TimeSpan.FromMinutes(10);
        private static System.Timers.Timer cleanupTimer;

        static void Main()
        {
            udpServer = new UdpClient(PORT);
            Console.WriteLine($"The server is running on port {PORT}");

            cleanupTimer = new System.Timers.Timer(60000);//каждую минуту
            cleanupTimer.Elapsed += CleanupInactiveClients;
            cleanupTimer.Start();

            Listen();
        }

        private static void Listen()
        {
            while (true)
            {
                IPEndPoint remoteEndPoint = null;
                byte[] data = udpServer.Receive(ref remoteEndPoint);
                string reguest = Encoding.UTF8.GetString(data);

                Log($"Receive a reguest from {remoteEndPoint}: {reguest}");

                if (!IsClientAllowed(remoteEndPoint))
                {
                    string response = "Reguest limit has been reached. Try again later.";
                    SendResponse(remoteEndPoint, response);
                    continue;
                }

                string price = GetPrice(reguest);
                SendResponse(remoteEndPoint, price);
            }
        }

        private static void Log(string v)
        {
            Console.WriteLine(v);
        }

        private static bool IsClientAllowed(IPEndPoint client)
        {
            if (!clientRequests.ContainsKey(client))
            {
                if (clientRequests.Count >= maxClients)
                {
                    return false;//превышено количество подключений
                }
                clientRequests[client] = (0, DateTime.Now);
            }
            
            var (count, lastReguest) = clientRequests[client];
            if ((DateTime.Now - lastReguest).TotalHours >= 1)
            {
                return false ;
            }

            clientRequests[client] = (count +1, DateTime.Now);
            return true;
        }

        private static void SendResponse (IPEndPoint client, string response)
        {
            byte[] data = Encoding.UTF8.GetBytes(response);
            udpServer.Send(data, data.Length, client);
            Log($"Sent a replay to the client {client}: {response}");
        }

        private static string GetPrice (string component)
        {
            return components.ContainsKey(component) ?
                components[component] :
                "Component not found";
        }

        private static void CleanupInactiveClients(object sender, ElapsedEventArgs e)
        {
            var inactiveClients = new List<IPEndPoint>();

            foreach (var client in clientRequests)
            {
                if ((DateTime.Now - client.Value.последнийЗапрос) > inactiveTimeout)
                {
                    inactiveClients.Add(client.Key);
                }
            }

            foreach (var client in inactiveClients)
            {
                clientRequests.Remove(client);
                Log($"Client {client} was shut down due to inactivity.");
            }
        }
    }
}
