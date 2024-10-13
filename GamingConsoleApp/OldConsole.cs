using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GamingConsoleApp
{
    public class OldConsole
    {
        public int ConsoleNumber { get; private set; }
        public string PlayerName { get; private set; }
        public int Score { get; private set; }
        public string Status { get; private set; }
        private HttpListener httpListener;

        public OldConsole(int consoleNumber, string playerName)
        {
            ConsoleNumber = consoleNumber;
            PlayerName = playerName;
            Score = 0;
            Status = "Stopped";
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:500{consoleNumber}/");
        }

        public void Start()
        {
            Status = "Running";
            httpListener.Start();
            Console.WriteLine($"Old Console {ConsoleNumber} started for {PlayerName}.");

            Task.Run(() => ListenForRequests());
        }

        private async Task ListenForRequests()
        {
            while (true)
            {
                var context = await httpListener.GetContextAsync();
                var response = context.Response;

                // Prepare data as JSON
                var consoleData = new
                {
                    ConsoleNumber = ConsoleNumber,
                    PlayerName = PlayerName,
                    Score = Score,
                    Status = Status
                };
                var jsonResponse = JsonSerializer.Serialize(consoleData);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                // Send response
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }

        public void IncrementScore()
        {
            Score++;
            Console.WriteLine($"Console {ConsoleNumber} Score: {Score}");
        }

        public void Stop()
        {
            Status = "Stopped";
            Console.WriteLine($"Old Console {ConsoleNumber} stopped. Final score: {Score}");
            httpListener.Stop();
        }
    }
}
