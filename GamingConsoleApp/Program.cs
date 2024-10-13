using System;

namespace GamingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: GamingConsoleApp.exe <type> <ConsoleNumber> <PlayerName>");
                return;
            }

            string type = args[0].ToLower();
            int consoleNumber = int.Parse(args[1]);
            string playerName = args[2];

            if (type == "old")
            {
                OldConsole oldConsole = new OldConsole(consoleNumber, playerName);
                oldConsole.Start();
                Console.WriteLine("Press any key to stop the console...");
                Console.ReadKey();
                oldConsole.Stop();
            }
            else if (type == "new")
            {
                NewConsole newConsole = new NewConsole(consoleNumber, playerName);
                newConsole.Start();
            }
            else
            {
                Console.WriteLine("Invalid console type. Use 'old' or 'new'.");
            }
        }
    }
}
