using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MainBoardApp
{
    public class MainBoard
    {
        private Dictionary<int, ConsoleData> scoreboard;
        private List<int> oldConsoleNumbers;
        private bool running;
        private TcpListener tcpListener;

        public MainBoard()
        {
            scoreboard = new Dictionary<int, ConsoleData>();
            oldConsoleNumbers = new List<int>();
            running = true;
            tcpListener = new TcpListener(IPAddress.Any, 5000);
        }

        public void Start()
        {
            Console.WriteLine("MainBoard started. Waiting for consoles to connect...");

            // Start TCP listener in a separate thread for new consoles
            Thread tcpListenerThread = new Thread(StartTcpListener);
            tcpListenerThread.IsBackground = true;
            tcpListenerThread.Start();

            // Start polling old consoles in a separate thread
            Thread pollingThread = new Thread(() => PollOldConsoles());
            pollingThread.IsBackground = true;
            pollingThread.Start();

            // Keep the application running until manually stopped
            while (running)
            {
                Thread.Sleep(1000);
            }
        }

        private void StartTcpListener()
        {
            tcpListener.Start();
            Console.WriteLine("TCP listener started on port 5000.");

            while (running)
            {
                try
                {
                    var client = tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in TCP listener: {ex.Message}");
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var consoleData = JsonSerializer.Deserialize<ConsoleData>(json);

                    if (consoleData != null)
                    {
                        UpdateScoreboard(consoleData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        public void RegisterOldConsole(int consoleNumber)
        {
            oldConsoleNumbers.Add(consoleNumber);
            Console.WriteLine($"Registered Old Console {consoleNumber}");

            // Initialize the console data with "Running" status
            UpdateScoreboard(new ConsoleData
            {
                ConsoleNumber = consoleNumber,
                PlayerName = $"Player{consoleNumber}",
                Score = 0,
                Status = "Running"
            });
        }

        private async void PollOldConsoles()
        {
            while (running)
            {
                foreach (var consoleNumber in oldConsoleNumbers.ToList())
                {
                    await FetchDataFromOldConsole(consoleNumber);
                }

                Thread.Sleep(5000); // Poll every 5 seconds
            }
        }

        private async Task FetchDataFromOldConsole(int consoleNumber)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"http://localhost:500{consoleNumber}/";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var consoleData = JsonSerializer.Deserialize<ConsoleData>(jsonResponse);

                        if (consoleData != null)
                        {
                            UpdateScoreboard(consoleData);
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                if (scoreboard.ContainsKey(consoleNumber) && scoreboard[consoleNumber].Status != "Stopped")
                {
                    UpdateScoreboard(new ConsoleData
                    {
                        ConsoleNumber = consoleNumber,
                        PlayerName = scoreboard[consoleNumber].PlayerName,
                        Score = scoreboard[consoleNumber].Score,
                        Status = "Stopped"
                    });

                    Console.WriteLine($"Old Console {consoleNumber} stopped. No data received.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data from Old Console {consoleNumber}: {ex.Message}");
            }
        }

        private void UpdateScoreboard(ConsoleData consoleData)
        {
            scoreboard[consoleData.ConsoleNumber] = consoleData;

            Console.WriteLine("Current Scoreboard:");
            foreach (var entry in scoreboard.OrderByDescending(c => c.Value.Score))
            {
                Console.WriteLine($"Console Number: {entry.Value.ConsoleNumber}, Score: {entry.Value.Score}, Player Name: {entry.Value.PlayerName}, Status: {entry.Value.Status}");
            }
            Console.WriteLine();
        }

        private class ConsoleData
        {
            public int ConsoleNumber { get; set; }
            public string PlayerName { get; set; }
            public int Score { get; set; }
            public string Status { get; set; }
        }

        static void Main(string[] args)
        {
            MainBoard mainBoard = new MainBoard();
            mainBoard.RegisterOldConsole(1); // Register Old Console with ConsoleNumber = 1
            mainBoard.Start();
        }
    }
}
