using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace GamingConsoleApp;

public class NewConsole
{
    private TcpClient tcpClient;
    private NetworkStream stream;
    public int ConsoleNumber { get; private set; }
    public string PlayerName { get; private set; }
    public int Score { get; private set; }
    public bool IsRunning { get; private set; }

    public NewConsole(int consoleNumber, string playerName)
    {
        ConsoleNumber = consoleNumber;
        PlayerName = playerName;
        Score = 0;
        tcpClient = new TcpClient();
    }

    public void Start()
    {
        IsRunning = true;
        Console.WriteLine($"New Console {ConsoleNumber} started for {PlayerName}. Status: Running");

        ConnectToMainBoard();

        while (IsRunning)
        {
            string command = Console.ReadLine();
            if (command == "increment")
            {
                IncrementScore();
                SendUpdateToMainBoard();
            }
            else if (command == "stop")
            {
                Stop();
            }
            else
            {
                Console.WriteLine("Unknown command. Use 'increment' to increase score or 'stop' to stop the console.");
            }
        }
    }

    private void IncrementScore()
    {
        Score++;
        Console.WriteLine($"Console {ConsoleNumber} Score: {Score}");
    }

    public void Stop()
    {
        IsRunning = false;
        Console.WriteLine($"Console {ConsoleNumber} stopped. Final score: {Score}");
        SendStopToMainBoard();
        stream?.Close();
        tcpClient?.Close();
    }

    private void ConnectToMainBoard()
    {
        try
        {
            tcpClient.Connect("localhost", 5000);
            stream = tcpClient.GetStream();
            Console.WriteLine("Connected to MainBoard.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to MainBoard: {ex.Message}");
        }
    }

    private void SendUpdateToMainBoard()
    {
        var data = new
        {
            ConsoleNumber = ConsoleNumber,
            PlayerName = PlayerName,
            Score = Score,
            Status = "Running"
        };

        SendData(data);
    }

    private void SendStopToMainBoard()
    {
        var data = new
        {
            ConsoleNumber = ConsoleNumber,
            PlayerName = PlayerName,
            Score = Score,
            Status = "Stopped"
        };

        SendData(data);
    }

    private void SendData(object data)
    {
        if (stream != null)
        {
            var jsonData = JsonSerializer.Serialize(data);
            var buffer = Encoding.UTF8.GetBytes(jsonData);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
                Console.WriteLine($"Data sent to MainBoard: {jsonData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data: {ex.Message}");
            }
        }
    }

    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: GamingConsoleApp.exe <ConsoleNumber> <PlayerName>");
            return;
        }

        int consoleNumber = int.Parse(args[0]);
        string playerName = args[1];
        NewConsole console = new NewConsole(consoleNumber, playerName);
        console.Start();
    }
}
