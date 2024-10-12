using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MainBoardApp;

public class MainBoard
{
    private TcpListener tcpListener;

    public MainBoard()
    {
        // Listen on any IP address on port 5000
        tcpListener = new TcpListener(IPAddress.Any, 5000);
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
            // You can parse the message and update your scoreboard here.
        }

        client.Close();
    }

    static void Main(string[] args)
    {
        MainBoard mainBoard = new MainBoard();
        mainBoard.Start();
    }
}
