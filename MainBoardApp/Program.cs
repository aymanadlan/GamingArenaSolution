using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace MainBoardApp
{
    public class MainBoard
    {
        private TcpListener tcpListener;
        private Dictionary<int, ConsoleData> scoreboard;

        public MainBoard()
        {
            // Listen on any IP address on port 5000
            tcpListener = new TcpListener(IPAddress.Any, 5000);
            scoreboard = new Dictionary<int, ConsoleData>();
        }

        public void Start()
        {
            tcpListener.Start();
            Console.WriteLine("MainBoard started. Waiting for consoles to connect...");

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                Console.WriteLine("Console connected.");

                // Handle the client in a separate thread
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received data: {message}");

                // Parse the message and update the scoreboard
                var consoleData = JsonSerializer.Deserialize<ConsoleData>(message);
                if (consoleData != null)
                {
                    UpdateScoreboard(consoleData);
                }
            }

            client.Close();
        }

        private void UpdateScoreboard(ConsoleData consoleData)
        {
            // Update or add the console data in the scoreboard
            scoreboard[consoleData.ConsoleNumber] = consoleData;

            // Display the current scoreboard sorted by score
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
            mainBoard.Start();
        }
    }
}
