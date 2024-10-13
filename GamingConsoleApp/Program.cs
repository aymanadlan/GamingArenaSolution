using System;

namespace GamingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: GamingConsoleApp.exe <type> <ConsoleNumber> <PlayerName>");
                Console.WriteLine("<type>: 'new' for New Console or 'old' for Old Console");
                return;
            }

            string type = args[0].ToLower();
            int consoleNumber = int.Parse(args[1]);
            string playerName = args[2];

            if (type == "new")
            {
                NewConsole console = new NewConsole(consoleNumber, playerName);
                console.Start();
            }
            else if (type == "old")
            {
                OldConsole console = new OldConsole(consoleNumber, playerName);
                console.Start();
            }
            else
            {
                Console.WriteLine("Invalid console type. Use 'new' or 'old'.");
            }
        }
    }
}
