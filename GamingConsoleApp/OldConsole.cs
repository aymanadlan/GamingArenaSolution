using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GamingConsoleApp
{
    public class OldConsole
    {
        private TcpListener tcpListener;
        public int ConsoleNumber { get; private set; }
        public string PlayerName { get; private set; }
        public int Score { get; private set; }
        public string Status { get; private set; }

        public OldConsole(int consoleNumber, string playerName)
        {
            ConsoleNumber = consoleNumber;
            PlayerName = playerName;
            Score = 0;
            Status = "Stopped";
            tcpListener = new TcpListener(IPAddress.Any, 5000 + consoleNumber); // Unique port for each console
        }

        public void Start()
        {
            tcpListener.Start();
            Console.WriteLine($"Old Console {ConsoleNumber} started for {PlayerName}. Status: {Status}");

            // Start listening for incoming requests
            Task.Run(() => ListenForRequests());
        }

        private async Task ListenForRequests()
        {
            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine($"Old Console {ConsoleNumber} accepted a connection.");
                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received request: {request}");

                // Respond with current score and status
                var response = new
                {
                    ConsoleNumber = ConsoleNumber,
                    PlayerName = PlayerName,
                    Score = Score,
                    Status = Status
                };
                var jsonResponse = JsonSerializer.Serialize(response);
                byte[] responseData = Encoding.UTF8.GetBytes(jsonResponse);
                stream.Write(responseData, 0, responseData.Length);
            }

            client.Close();
        }

        public void IncrementScore()
        {
            Score++;
            Status = "Running";
        }

        public void Stop()
        {
            Status = "Stopped";
            Console.WriteLine($"Old Console {ConsoleNumber} stopped. Final score: {Score}");
            tcpListener.Stop();
        }
    }
}
